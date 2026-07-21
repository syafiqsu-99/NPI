using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Hubs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface INotificationService
    {
        // ── Delivery ──────────────────────────────────────────────────────────
        Task NotifyAsync(int userId, string type, string title, string body, int? projId = null, int? taskId = null);
        Task NotifyManyAsync(IEnumerable<int> userIds, string type, string title, string body, int? projId = null, int? taskId = null);
        Task NotifyDepartmentAsync(int deptId, string type, string title, string body, int? projId = null);
        Task NotifyProjectTeamAsync(int projId, string type, string title, string body, int? taskId = null, int? excludeUserId = null);

        // ── Reads ─────────────────────────────────────────────────────────────
        Task<List<NotificationDto>> GetUnreadAsync(int userId, int take = 50);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkReadAsync(int notifId, int userId);
        Task MarkAllReadAsync(int userId);

        // ── Business event triggers ───────────────────────────────────────────
        Task OnTaskCompletedAsync(int taskId, int projId, string? stageId);
        Task OnStageCompletedAsync(int projId, string stageId);
        Task OnProjectLaunchedAsync(int projId);
        Task OnFaiCompletedAsync(int projId, int taskId);
        Task OnTaskDatesRevisedAsync(int taskId, int projId, DateOnly? oldStart, DateOnly? oldEnd, DateOnly? newStart, DateOnly? newEnd);
        Task OnFileUploadedAsync(int projId, int uploadedByUserId);
        Task OnTaskAssignedAsync(int taskId, int projId, int assignedToUserId);
        Task OnProjectStatusChangedAsync(int projId, string status);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(ApplicationDbContext ctx, IHubContext<NotificationHub> hub)
        {
            _ctx = ctx;
            _hub = hub;
        }

        // ══ Delivery ══════════════════════════════════════════════════════════

        public async Task NotifyAsync(int userId, string type, string title, string body,
                                      int? projId = null, int? taskId = null)
        {
            var notif = new Notifications
            {
                user_id = userId,
                proj_id = projId,
                task_id = taskId,
                notif_type = type,
                subject = title,
                body = body,
                is_read = false,
                created_at = DateTime.Now
            };

            _ctx.Notifications.Add(notif);
            await _ctx.SaveChangesAsync();

            _ = _hub.Clients
                    .Group($"user_{userId}")
                    .SendAsync("NewNotification", new
                    {
                        notif.notif_id,
                        title,
                        type,
                        body,
                        proj_id = projId,
                        task_id = taskId,
                        created_at = notif.created_at
                    });
        }

        public async Task NotifyManyAsync(IEnumerable<int> userIds, string type, string title,
                                          string body, int? projId = null, int? taskId = null)
        {
            var distinctIds = userIds.Distinct().ToList();
            if (distinctIds.Count == 0) return;

            var now = DateTime.Now;

            var notifications = distinctIds.Select(uid => new Notifications
            {
                user_id = uid,
                proj_id = projId,
                task_id = taskId,
                notif_type = type,
                subject = title,
                body = body,
                is_read = false,
                created_at = now
            }).ToList();

            await _ctx.Notifications.AddRangeAsync(notifications);
            await _ctx.SaveChangesAsync();

            foreach (var notif in notifications)
            {
                _ = _hub.Clients
                        .Group($"user_{notif.user_id}")
                        .SendAsync("NewNotification", new
                        {
                            notif.notif_id,
                            title,
                            type,
                            body,
                            proj_id = projId,
                            task_id = taskId,
                            created_at = notif.created_at
                        });
            }
        }

        public async Task NotifyDepartmentAsync(int deptId, string type, string title,
                                                 string body, int? projId = null)
        {
            var userIds = await _ctx.Users
                .Where(u => u.dept_id == deptId && u.is_active)
                .Select(u => u.user_id)
                .ToListAsync();

            await NotifyManyAsync(userIds, type, title, body, projId);
        }

        public async Task NotifyProjectTeamAsync(int projId, string type, string title,
                                                  string body, int? taskId = null,
                                                  int? excludeUserId = null)
        {
            var userIds = await _ctx.ProjectTeams
                .Where(pt => pt.proj_id == projId)
                .Select(pt => pt.user_id)
                .Distinct()
                .ToListAsync();

            await NotifyManyAsync(
                userIds.Where(uid => uid != excludeUserId),
                type, title, body, projId, taskId);
        }

        // ══ Reads ═════════════════════════════════════════════════════════════

        public async Task<List<NotificationDto>> GetUnreadAsync(int userId, int take = 50)
        {
            return await _ctx.Notifications
                .AsNoTracking()
                .Where(n => n.user_id == userId && !n.is_read)
                .OrderByDescending(n => n.created_at)
                .Take(take)
                .Select(n => new NotificationDto
                {
                    notif_id = n.notif_id,
                    type = n.notif_type,
                    title = n.subject,
                    body = n.body,
                    is_read = n.is_read,
                    proj_id = n.proj_id,
                    task_id = n.task_id,
                    created_at = n.created_at
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _ctx.Notifications
                .CountAsync(n => n.user_id == userId && !n.is_read);
        }

        public async Task MarkReadAsync(int notifId, int userId)
        {
            var notif = await _ctx.Notifications
                .FirstOrDefaultAsync(n => n.notif_id == notifId && n.user_id == userId);

            if (notif is null) return;

            notif.is_read = true;
            notif.read_at = DateTime.Now;
            await _ctx.SaveChangesAsync();
        }

        public async Task MarkAllReadAsync(int userId)
        {
            await _ctx.Notifications
                .Where(n => n.user_id == userId && !n.is_read)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.is_read, true)
                    .SetProperty(n => n.read_at, DateTime.Now));
        }

        // ══ Business event triggers ═══════════════════════════════════════════

        // N1: Task completed → notify departments with pending tasks in the same stage
        public async Task OnTaskCompletedAsync(int taskId, int projId, string? stageId)
        {
            if (string.IsNullOrEmpty(stageId)) return;

            var completedTask = await _ctx.Tasks
                .AsNoTracking()
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (completedTask == null) return;

            var targetDeptIds = await _ctx.Tasks
                .AsNoTracking()
                .Where(t => t.proj_id == projId
                         && t.stage_id == stageId
                         && t.status == TasksStatus.NotStarted
                         && t.task_id != taskId
                         && t.dept_id.HasValue)
                .Select(t => t.dept_id!.Value)
                .Distinct()
                .ToListAsync();

            if (targetDeptIds.Count == 0) return;

            var project = await _ctx.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.proj_id == projId);

            var teamUserIds = await _ctx.ProjectTeams
                .AsNoTracking()
                .Where(pt => pt.proj_id == projId)
                .Join(_ctx.Users, pt => pt.user_id, u => u.user_id, (pt, u) => u)
                .Where(u => u.is_active && u.dept_id.HasValue && targetDeptIds.Contains(u.dept_id.Value))
                .Select(u => u.user_id)
                .ToListAsync();

            await NotifyManyAsync(
                teamUserIds,
                "handover",
                $"Action required: {project?.proj_no}",
                $"Task '{completedTask.title}' ({completedTask.Department?.dept_name}) is complete. " +
                $"Your department's tasks in stage {stageId} are now unblocked.",
                projId, taskId);
        }

        // N2: All tasks in a stage completed
        public async Task OnStageCompletedAsync(int projId, string stageId)
        {
            var project = await _ctx.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.proj_id == projId);
            if (project == null) return;

            var teamUserIds = await _ctx.ProjectTeams
                .AsNoTracking()
                .Where(pt => pt.proj_id == projId)
                .Select(pt => pt.user_id)
                .Distinct()
                .ToListAsync();

            var recipients = teamUserIds.Append(project.created_by);

            await NotifyManyAsync(
                recipients,
                "stage_complete",
                $"Stage {stageId} completed — {project.proj_no}",
                $"All tasks in stage {stageId} for project '{project.proj_name}' are marked complete.",
                projId);
        }

        // N5: Project launched
        public async Task OnProjectLaunchedAsync(int projId)
        {
            var project = await _ctx.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.proj_id == projId);
            if (project == null) return;

            var teamUserIds = await _ctx.ProjectTeams
                .AsNoTracking()
                .Where(pt => pt.proj_id == projId)
                .Select(pt => pt.user_id)
                .Distinct()
                .ToListAsync();

            await NotifyManyAsync(
                teamUserIds,
                "project_launch",
                $"Project launched: {project.proj_no}",
                $"You have been assigned to '{project.proj_name}'. " +
                $"The project is now In Progress — check your tasks.",
                projId);
        }

        // N6: FAI completed → notify Sales
        public async Task OnFaiCompletedAsync(int projId, int taskId)
        {
            var project = await _ctx.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.proj_id == projId);
            if (project == null) return;

            var salesDept = await _ctx.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.dept_code == DeptCodes.Sales);

            if (salesDept == null) return;

            var salesUserIds = await _ctx.ProjectTeams
                .AsNoTracking()
                .Where(pt => pt.proj_id == projId)
                .Join(_ctx.Users, pt => pt.user_id, u => u.user_id, (pt, u) => u)
                .Where(u => u.dept_id == salesDept.dept_id && u.is_active)
                .Select(u => u.user_id)
                .ToListAsync();

            await NotifyManyAsync(
                salesUserIds,
                "fai_complete",
                $"FAI complete — ready for customer: {project.proj_no}",
                $"QA has completed the First Article Inspection for '{project.proj_name}'. " +
                $"Samples are ready for customer submission.",
                projId, taskId);

            if (!salesUserIds.Contains(project.created_by))
            {
                await NotifyAsync(
                    project.created_by,
                    "fai_complete",
                    $"FAI complete — {project.proj_no}",
                    "First Article Inspection is done. Sales should now submit samples to the customer.",
                    projId, taskId);
            }
        }

        // N7: Task dates revised
        public async Task OnTaskDatesRevisedAsync(int taskId, int projId,
            DateOnly? oldStart, DateOnly? oldEnd,
            DateOnly? newStart, DateOnly? newEnd)
        {
            var task = await _ctx.Tasks.AsNoTracking()
                .FirstOrDefaultAsync(t => t.task_id == taskId);
            var project = await _ctx.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.proj_id == projId);
            if (task == null || project == null) return;

            var msg = $"Task '{task.title}' in '{project.proj_name}' has been rescheduled. " +
                      $"Was: {oldStart:dd MMM} → {oldEnd:dd MMM}. " +
                      $"Now: {newStart:dd MMM} → {newEnd:dd MMM}.";

            var recipients = new List<int> { project.created_by };
            if (task.assigned_to.HasValue)
                recipients.Add(task.assigned_to.Value);

            await NotifyManyAsync(
                recipients,
                "date_revised",
                $"Task rescheduled: {task.task_code ?? task.title}",
                msg, projId, taskId);
        }

        // N8: File upload milestone
        public async Task OnFileUploadedAsync(int projId, int uploadedByUserId)
        {
            var milestones = new[] { 10, 25, 50 };

            var count = await _ctx.Files
                .CountAsync(f => f.proj_id == projId && f.is_latest);

            if (!milestones.Contains(count)) return;

            var project = await _ctx.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.proj_id == projId);
            if (project == null) return;

            if (project.created_by != uploadedByUserId)
            {
                await NotifyAsync(
                    project.created_by,
                    "file_milestone",
                    $"File milestone: {count} files in {project.proj_no}",
                    $"Project '{project.proj_name}' now has {count} documents uploaded.",
                    projId);
            }
        }

        // N9: Task assigned
        public async Task OnTaskAssignedAsync(int taskId, int projId, int assignedToUserId)
        {
            var task = await _ctx.Tasks.AsNoTracking()
                .FirstOrDefaultAsync(t => t.task_id == taskId);
            var project = await _ctx.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.proj_id == projId);
            if (task == null || project == null) return;

            await NotifyAsync(
                assignedToUserId,
                "task_assigned",
                $"Task assigned to you: {task.task_code ?? task.title}",
                $"You have been assigned '{task.title}' in project '{project.proj_name}'. " +
                $"Planned: {task.planned_start_date:dd MMM} → {task.planned_end_date:dd MMM}.",
                projId, taskId);
        }

        // N11: Project status changed
        public async Task OnProjectStatusChangedAsync(int projId, string status)
        {
            var project = await _ctx.Projects
                .AsNoTracking()
                .Include(p => p.ProjectTeams)
                .FirstOrDefaultAsync(p => p.proj_id == projId);

            if (project == null) return;

            var (notifType, title, bodyTemplate) = status switch
            {
                ProjectsStatus.Planning => ("project_planning",
                    "Project moved to Planning",
                    "'{0}' is now in Planning phase. Awaiting project launch and team assignment."),

                ProjectsStatus.InProgress => ("project_active",
                    "Project launched and In Progress",
                    "'{0}' is now In Progress. All team members should review their assigned tasks."),

                ProjectsStatus.OnHold => ("project_on_hold",
                    "Project placed On Hold",
                    "'{0}' has been placed On Hold. Work should pause until further notice."),

                ProjectsStatus.Completed => ("project_complete",
                    "Project Completed",
                    "'{0}' has been completed successfully. All tasks are done and deliverables are signed off."),

                ProjectsStatus.Cancelled => ("project_cancelled",
                    "Project Cancelled",
                    "'{0}' has been cancelled. If you have outstanding work, please stop and report to your manager."),

                _ => ("project_status_changed",
                    $"Project status: {status}",
                    "'{0}' status has been updated to: " + status)
            };

            var teamUserIds = project.ProjectTeams
                .Select(pt => pt.user_id)
                .Distinct()
                .ToList();

            await NotifyManyAsync(
                teamUserIds,
                notifType,
                title,
                string.Format(bodyTemplate, project.proj_name),
                projId);
        }
    }
}