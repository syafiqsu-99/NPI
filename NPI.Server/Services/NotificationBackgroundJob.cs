using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;

namespace NPI.Server.Services
{
    /// <summary>
    /// Runs every 6 hours.
    /// Handles: N3 (overdue tasks), N4 (stuck in Planning), N10 (approval tasks stalled).
    /// </summary>
    public class NotificationBackgroundJob : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationBackgroundJob> _logger;
        private static readonly TimeSpan _interval = TimeSpan.FromHours(6);

        public NotificationBackgroundJob(
            IServiceProvider services,
            ILogger<NotificationBackgroundJob> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Stagger startup by 30 seconds to let the app warm up
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunChecksAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "NotificationBackgroundJob encountered an error.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task RunChecksAsync(CancellationToken ct)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now;

            // ── N3: Overdue tasks ─────────────────────────────────────────────
            var overdueTasks = await context.Tasks
                .Include(t => t.Project)
                .Where(t => t.status != "Completed"
                         && t.status != "Cancelled"
                         && t.planned_end_date.HasValue
                         && t.planned_end_date.Value < today
                         && t.assigned_to.HasValue)
                .ToListAsync(ct);

            foreach (var task in overdueTasks)
            {
                if (task.assigned_to.HasValue)
                {
                    // De-duplicate: only notify once per day using a simple check
                    // (notification type + task_id + today's date prefix)
                    var alreadyNotifiedToday = await context.Notifications
                        .AnyAsync(n => n.task_id == task.task_id
                                    && n.notif_type == "overdue"
                                    && n.created_at.Date == now.Date
                                    && n.user_id == task.assigned_to.Value, ct);

                    if (!alreadyNotifiedToday)
                    {
                        var daysOverdue = today.DayNumber - task.planned_end_date!.Value.DayNumber;
                        await notifications.NotifyAsync(
                            task.assigned_to.Value,
                            "overdue",
                            $"Overdue task: {task.task_code ?? task.title}",
                            $"Task '{task.title}' in '{task.Project?.proj_name}' is {daysOverdue} day(s) overdue. " +
                            $"It was due {task.planned_end_date:dd MMM yyyy}.",
                            task.proj_id, task.task_id);
                    }
                }

                // Notify project creator separately if different from assignee
                if (task.Project?.created_by != null &&
                    task.Project.created_by != task.assigned_to)
                {
                    var creatorNotified = await context.Notifications
                        .AnyAsync(n => n.task_id == task.task_id
                                    && n.notif_type == "overdue"
                                    && n.created_at.Date == now.Date
                                    && n.user_id == task.Project.created_by, ct);

                    if (!creatorNotified)
                    {
                        await notifications.NotifyAsync(
                            task.Project.created_by,
                            "overdue",
                            $"Overdue task: {task.task_code ?? task.title}",
                            $"Task '{task.title}' assigned to your project team is overdue.",
                            task.proj_id, task.task_id);
                    }
                }
            }

            // ── N4: Projects stuck in Planning > 48 hours ─────────────────────
            var stuckThreshold = now.AddHours(-48);
            var stuckProjects = await context.Projects
                .Where(p => p.status == "Planning"
                         && p.created_at <= stuckThreshold)
                .ToListAsync(ct);

            if (stuckProjects.Any())
            {
                var adminIds = await context.Users
                    .Where(u => u.Role!.role_name == "Admin" && u.is_active)
                    .Select(u => u.user_id)
                    .ToListAsync(ct);

                foreach (var proj in stuckProjects)
                {
                    var hoursStuck = (int)(now - proj.created_at).TotalHours;

                    foreach (var adminId in adminIds)
                    {
                        var alreadyNotified = await context.Notifications
                            .AnyAsync(n => n.proj_id == proj.proj_id
                                        && n.notif_type == "planning_stuck"
                                        && n.created_at.Date == now.Date
                                        && n.user_id == adminId, ct);

                        if (!alreadyNotified)
                        {
                            await notifications.NotifyAsync(
                                adminId,
                                "planning_stuck",
                                $"Project not launched: {proj.proj_no}",
                                $"Project '{proj.proj_name}' has been in Planning status for {hoursStuck}h. " +
                                $"It may need team assignment and launch.",
                                proj.proj_id);
                        }
                    }
                }
            }

            // ── N10: Approval tasks (hasLink tasks) stalled > 72 hours ─────────
            // task_code patterns for approval tasks: x.6, x.11 (customer approval)
            // and any task with title containing "approval" or "customer"
            var approvalStallThreshold = now.AddHours(-72);

            var stalledApprovalTasks = await context.Tasks
                .Include(t => t.Project)
                .Where(t => t.status == "In Progress"
                         && t.updated_at.HasValue
                         && t.updated_at.Value <= approvalStallThreshold
                         && (t.title.ToLower().Contains("approval")
                             || t.title.ToLower().Contains("customer")
                             || (t.task_code != null && (
                                    t.task_code.EndsWith(".6") ||
                                    t.task_code.EndsWith(".11") ||
                                    t.task_code == "5.10"))))
                .ToListAsync(ct);

            foreach (var task in stalledApprovalTasks)
            {
                // Find Sales dept users on this project
                var salesDept = await context.Departments
                    .FirstOrDefaultAsync(d => d.dept_name == "Sales", ct);

                if (salesDept == null) continue;

                var salesUserIds = await context.ProjectTeams
                    .Where(pt => pt.proj_id == task.proj_id)
                    .Join(context.Users,
                        pt => pt.user_id,
                        u => u.user_id,
                        (pt, u) => u)
                    .Where(u => u.dept_id == salesDept.dept_id && u.is_active)
                    .Select(u => u.user_id)
                    .ToListAsync(ct);

                var hoursStalled = task.updated_at.HasValue
                    ? (int)(now - task.updated_at.Value).TotalHours
                    : 72;

                foreach (var uid in salesUserIds)
                {
                    var alreadyNotified = await context.Notifications
                        .AnyAsync(n => n.task_id == task.task_id
                                    && n.notif_type == "approval_stalled"
                                    && n.created_at.Date == now.Date
                                    && n.user_id == uid, ct);

                    if (!alreadyNotified)
                    {
                        await notifications.NotifyAsync(
                            uid,
                            "approval_stalled",
                            $"Approval stalled: {task.task_code ?? task.title}",
                            $"Customer approval task '{task.title}' in '{task.Project?.proj_name}' " +
                            $"has been In Progress for {hoursStalled}h without an update. " +
                            $"Follow up with the customer.",
                            task.proj_id, task.task_id);
                    }
                }
            }

            _logger.LogInformation(
                "NotificationBackgroundJob: checked {overdue} overdue tasks, " +
                "{stuck} stuck projects, {stalled} stalled approvals.",
                overdueTasks.Count, stuckProjects.Count, stalledApprovalTasks.Count);
        }
    }
}