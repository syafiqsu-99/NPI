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
        Task NotifyAsync(int userId, string type, string title, string body, int? projId = null, int? taskId = null, int? enquiryId = null);
        Task NotifyManyAsync(IEnumerable<int> userIds, string type, string title, string body, int? projId = null, int? taskId = null, int? enquiryId = null);
        Task NotifyDepartmentAsync(int deptId, string type, string title, string body, int? projId = null);
        Task NotifyProjectTeamAsync(int projId, string type, string title, string body, int? taskId = null, int? excludeUserId = null);
        Task<List<NotificationDto>> GetUnreadAsync(int userId, int take = 50);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkReadAsync(int notifId, int userId);
        Task MarkAllReadAsync(int userId);
        Task OnTaskCompletedAsync(int taskId, int projId, string? stageId);
        Task OnStageCompletedAsync(int projId, string stageId);
        Task OnProjectLaunchedAsync(int projId);
        Task OnTaskDatesRevisedAsync(int taskId, int projId, DateOnly? oldStart, DateOnly? oldEnd, DateOnly? newStart, DateOnly? newEnd);
        Task OnTaskAssignedAsync(int taskId, int projId, int assignedToUserId);
        Task OnProjectStatusChangedAsync(int projId, string status);
        Task OnEnquiryReviewedAsync(int enquiryId, int createdBy, string enquiryNo, string decision, string? remark);
        Task OnEnquirySubmittedAsync(int enquiryId, string enquiryNo, int submittedBy);
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

        public async Task NotifyAsync(int userId, string type, string title, string body,
                                      int? projId = null, int? taskId = null, int? enquiryId = null)
        {
            var notif = new Notifications
            {
                user_id = userId,
                proj_id = projId,
                task_id = taskId,
                enquiry_id = enquiryId,
                notif_type = type,
                subject = title,
                body = body,
                is_read = false,
                created_at = DateTime.Now
            };

            _ctx.Notifications.Add(notif);
            await _ctx.SaveChangesAsync();

            await _hub.Clients
                    .Group($"user_{userId}")
                    .SendAsync("NewNotification", new
                    {
                        notif.notif_id,
                        title,
                        type,
                        body,
                        proj_id = projId,
                        task_id = taskId,
                        enquiry_id = enquiryId,
                        created_at = notif.created_at
                    });
        }

        public async Task NotifyManyAsync(IEnumerable<int> userIds, string type, string title,
                                          string body, int? projId = null, int? taskId = null, int? enquiryId = null)
        {
            var distinctIds = userIds.Distinct().ToList();
            if (distinctIds.Count == 0) return;

            var now = DateTime.Now;

            var notifications = distinctIds.Select(uid => new Notifications
            {
                user_id = uid,
                proj_id = projId,
                task_id = taskId,
                enquiry_id = enquiryId,
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
                await _hub.Clients
                        .Group($"user_{notif.user_id}")
                        .SendAsync("NewNotification", new
                        {
                            notif.notif_id,
                            title,
                            type,
                            body,
                            proj_id = projId,
                            task_id = taskId,
                            enquiry_id = enquiryId,
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
                    enquiry_id = n.enquiry_id,
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
                NotificationTypes.Handover,
                $"Action required: {project?.proj_no}",
                $"Task '{completedTask.title}' ({completedTask.Department?.dept_name}) is complete. " +
                $"Your department's tasks in stage {stageId} are now unblocked.",
                projId, taskId);
        }

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
                NotificationTypes.StageComplete,
                $"Stage {stageId} completed — {project.proj_no}",
                $"All tasks in stage {stageId} for project '{project.proj_name}' are marked complete.",
                projId);
        }

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
                NotificationTypes.ProjectLaunch,
                $"Project launched: {project.proj_no}",
                $"You have been assigned to '{project.proj_name}'. " +
                $"The project is now In Progress — check your tasks.",
                projId);
        }

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
                NotificationTypes.DateRevised,
                $"Task rescheduled: {task.task_code ?? task.title}",
                msg, projId, taskId);
        }

        public async Task OnTaskAssignedAsync(int taskId, int projId, int assignedToUserId)
        {
            var task = await _ctx.Tasks.AsNoTracking()
                .FirstOrDefaultAsync(t => t.task_id == taskId);
            var project = await _ctx.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.proj_id == projId);
            if (task == null || project == null) return;

            await NotifyAsync(
                assignedToUserId,
                NotificationTypes.TaskAssigned,
                $"Task assigned to you: {task.task_code ?? task.title}",
                $"You have been assigned '{task.title}' in project '{project.proj_name}'. " +
                $"Planned: {task.planned_start_date:dd MMM} → {task.planned_end_date:dd MMM}.",
                projId, taskId);
        }

        public async Task OnProjectStatusChangedAsync(int projId, string status)
        {
            var project = await _ctx.Projects
                .AsNoTracking()
                .Include(p => p.ProjectTeams)
                .FirstOrDefaultAsync(p => p.proj_id == projId);

            if (project == null) return;

            var (notifType, title, bodyTemplate) = status switch
            {
                ProjectsStatus.Planning => (NotificationTypes.ProjectPlanning,
                    "Project moved to Planning",
                    "'{0}' is now in Planning phase. Awaiting project launch and team assignment."),

                ProjectsStatus.InProgress => (NotificationTypes.ProjectActive,
                    "Project launched and In Progress",
                    "'{0}' is now In Progress. All team members should review their assigned tasks."),

                ProjectsStatus.OnHold => (NotificationTypes.ProjectOnHold,
                    "Project placed On Hold",
                    "'{0}' has been placed On Hold. Work should pause until further notice."),

                ProjectsStatus.Completed => (NotificationTypes.ProjectComplete,
                    "Project Completed",
                    "'{0}' has been completed successfully. All tasks are done and deliverables are signed off."),

                ProjectsStatus.Cancelled => (NotificationTypes.ProjectCancelled,
                    "Project Cancelled",
                    "'{0}' has been cancelled. If you have outstanding work, please stop and report to your manager."),

                _ => (NotificationTypes.ProjectStatusChanged,
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
        public async Task OnEnquiryReviewedAsync(int enquiryId, int createdBy,
            string enquiryNo, string decision, string? remark)
        {
            var readable = decision == "NeedsRework" ? "needs rework" : "not feasible";

            await NotifyAsync(
                createdBy,
                NotificationTypes.EnquiryReview,
                $"Enquiry {enquiryNo}: {readable}",
                string.IsNullOrWhiteSpace(remark)
                    ? $"Your enquiry has been reviewed and marked {readable}."
                    : remark.Trim(),
                projId: null,
                taskId: null,
                enquiryId: enquiryId);
        }

        public async Task OnEnquirySubmittedAsync(int enquiryId, string enquiryNo, int submittedBy)
        {
            var reviewerIds = await _ctx.Users
                .AsNoTracking()
                .Where(u => u.is_active
                         && u.user_id != submittedBy
                         && (u.Role!.role_name == SystemRoles.Admin
                          || u.Role!.role_name == SystemRoles.Manager))
                .Select(u => u.user_id)
                .ToListAsync();

            if (reviewerIds.Count == 0) return;

            await NotifyManyAsync(
                reviewerIds,
                NotificationTypes.EnquirySubmitted,
                $"Enquiry submitted: {enquiryNo}",
                "A new enquiry has been submitted and is awaiting review.",
                projId: null,
                taskId: null,
                enquiryId: enquiryId);
        }
    }
}