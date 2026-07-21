using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.Helpers;
using System.Linq;

namespace NPI.Server.Services
{
    public class NotificationBackgroundJob : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationBackgroundJob> _logger;

        private const int DueSoonDays = 3;
        private const int PlanningStuckHours = 48;
        private const int ApprovalStallHours = 72;

        public NotificationBackgroundJob(
            IServiceProvider services,
            ILogger<NotificationBackgroundJob> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = TimeUntilNextMidnight();

                _logger.LogInformation(
                    "NotificationBackgroundJob: next run in {hours:F1}h.",
                    delay.TotalHours);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                try
                {
                    await RunChecksAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "NotificationBackgroundJob encountered an error.");
                }
            }
        }

        private static TimeSpan TimeUntilNextMidnight()
        {
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            return nextMidnight - now;
        }

        private async Task RunChecksAsync(CancellationToken ct)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now;

            // Preload today's notifications once — avoids an AnyAsync per recipient.
            var sentToday = await context.Notifications
                .AsNoTracking()
                .Where(n => n.created_at >= now.Date)
                .Select(n => new { n.user_id, n.task_id, n.proj_id, n.notif_type })
                .ToListAsync(ct);

            var sentTaskKeys = sentToday
                .Where(n => n.task_id.HasValue)
                .Select(n => (n.user_id, n.task_id!.Value, n.notif_type))
                .ToHashSet();

            var sentProjKeys = sentToday
                .Where(n => n.proj_id.HasValue && !n.task_id.HasValue)
                .Select(n => (n.user_id, n.proj_id!.Value, n.notif_type))
                .ToHashSet();

            // ── N3: Overdue tasks ─────────────────────────────────────────────
            var overdueTasks = await context.Tasks
                .AsNoTracking()
                .Include(t => t.Project)
                .Where(t => t.status != TasksStatus.Completed
                         && t.status != TasksStatus.Cancelled
                         && t.planned_end_date.HasValue
                         && t.planned_end_date.Value < today)
                .ToListAsync(ct);

            foreach (var task in overdueTasks)
            {
                var daysOverdue = today.DayNumber - task.planned_end_date!.Value.DayNumber;

                var recipients = new List<int>();
                if (task.assigned_to.HasValue) recipients.Add(task.assigned_to.Value);
                if (task.Project != null) recipients.Add(task.Project.created_by);

                var pending = recipients
                    .Distinct()
                    .Where(uid => !sentTaskKeys.Contains((uid, task.task_id, "overdue")))
                    .ToList();

                if (pending.Count == 0) continue;

                await notifications.NotifyManyAsync(
                    pending,
                    "overdue",
                    $"Overdue task: {task.task_code ?? task.title}",
                    $"Task '{task.title}' in '{task.Project?.proj_name}' is {daysOverdue} day(s) overdue. " +
                    $"It was due {task.planned_end_date:dd MMM yyyy}.",
                    task.proj_id, task.task_id);
            }

            // ── N12: Tasks due within the next 3 days ─────────────────────────
            var dueSoonCutoff = today.AddDays(DueSoonDays);

            var dueSoonTasks = await context.Tasks
                .AsNoTracking()
                .Include(t => t.Project)
                .Where(t => t.status != TasksStatus.Completed
                         && t.status != TasksStatus.Cancelled
                         && t.planned_end_date.HasValue
                         && t.planned_end_date.Value >= today
                         && t.planned_end_date.Value <= dueSoonCutoff
                         && t.assigned_to.HasValue)
                .ToListAsync(ct);

            foreach (var task in dueSoonTasks)
            {
                var uid = task.assigned_to!.Value;
                if (sentTaskKeys.Contains((uid, task.task_id, "due_soon"))) continue;

                var daysLeft = task.planned_end_date!.Value.DayNumber - today.DayNumber;
                var whenText = daysLeft == 0 ? "today" : $"in {daysLeft} day(s)";

                await notifications.NotifyAsync(
                    uid,
                    "due_soon",
                    $"Due {whenText}: {task.task_code ?? task.title}",
                    $"Task '{task.title}' in '{task.Project?.proj_name}' is due " +
                    $"{task.planned_end_date:dd MMM yyyy}.",
                    task.proj_id, task.task_id);
            }

            // ── N4: Projects stuck in Planning ────────────────────────────────
            var stuckThreshold = now.AddHours(-PlanningStuckHours);
            var stuckProjects = await context.Projects
                .AsNoTracking()
                .Where(p => p.status == ProjectsStatus.Planning
                         && p.created_at <= stuckThreshold)
                .ToListAsync(ct);

            if (stuckProjects.Count > 0)
            {
                var adminIds = await context.Users
                    .AsNoTracking()
                    .Where(u => u.Role!.role_name == SystemRoles.Admin && u.is_active)
                    .Select(u => u.user_id)
                    .ToListAsync(ct);

                foreach (var proj in stuckProjects)
                {
                    var hoursStuck = (int)(now - proj.created_at).TotalHours;

                    var pending = adminIds
                        .Where(uid => !sentProjKeys.Contains((uid, proj.proj_id, "planning_stuck")))
                        .ToList();

                    if (pending.Count == 0) continue;

                    await notifications.NotifyManyAsync(
                        pending,
                        "planning_stuck",
                        $"Project not launched: {proj.proj_no}",
                        $"Project '{proj.proj_name}' has been in Planning status for {hoursStuck}h. " +
                        $"It may need team assignment and launch.",
                        proj.proj_id);
                }
            }

            // ── N10: Approval tasks stalled ───────────────────────────────────
            var approvalStallThreshold = now.AddHours(-ApprovalStallHours);

            var salesDept = await context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.dept_code == DeptCodes.Sales, ct);

            var stalledApprovalTasks = new List<Models.Tasks>();

            if (salesDept != null)
            {
                var approvalCodes = await context.TaskTemplates
                    .AsNoTracking()
                    .Where(t => t.is_active && t.has_link)
                    .Select(t => t.task_code)
                    .ToListAsync(ct);

                stalledApprovalTasks = await context.Tasks
                    .AsNoTracking()
                    .Include(t => t.Project)
                    .Where(t => t.status == TasksStatus.InProgress
                             && t.updated_at.HasValue
                             && t.updated_at.Value <= approvalStallThreshold
                             && t.task_code != null
                             && approvalCodes.Contains(t.task_code))
                    .ToListAsync(ct);

                var projIds = stalledApprovalTasks.Select(t => t.proj_id).Distinct().ToList();

                var salesByProject = await context.ProjectTeams
                    .AsNoTracking()
                    .Where(pt => projIds.Contains(pt.proj_id))
                    .Join(context.Users, pt => pt.user_id, u => u.user_id,
                          (pt, u) => new { pt.proj_id, u.user_id, u.dept_id, u.is_active })
                    .Where(x => x.dept_id == salesDept.dept_id && x.is_active)
                    .ToListAsync(ct);

                var salesLookup = salesByProject
                    .GroupBy(x => x.proj_id)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.user_id).ToList());

                foreach (var task in stalledApprovalTasks)
                {
                    if (!salesLookup.TryGetValue(task.proj_id, out var salesUserIds)) continue;

                    var hoursStalled = (int)(now - task.updated_at!.Value).TotalHours;

                    var pending = salesUserIds
                        .Where(uid => !sentTaskKeys.Contains((uid, task.task_id, "approval_stalled")))
                        .ToList();

                    if (pending.Count == 0) continue;

                    await notifications.NotifyManyAsync(
                        pending,
                        "approval_stalled",
                        $"Approval stalled: {task.task_code ?? task.title}",
                        $"Customer approval task '{task.title}' in '{task.Project?.proj_name}' " +
                        $"has been In Progress for {hoursStalled}h without an update. " +
                        $"Follow up with the customer.",
                        task.proj_id, task.task_id);
                }
            }

            _logger.LogInformation(
                "NotificationBackgroundJob: {overdue} overdue, {dueSoon} due soon, " +
                "{stuck} stuck projects, {stalled} stalled approvals.",
                overdueTasks.Count, dueSoonTasks.Count, stuckProjects.Count,
                stalledApprovalTasks.Count);
        }
    }
}