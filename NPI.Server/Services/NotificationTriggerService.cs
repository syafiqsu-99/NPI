using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;

namespace NPI.Server.Services
{
    /// <summary>
    /// Centralises all business-event notification triggers.
    /// Call methods from TaskService, ProjectService, FileService as events occur.
    /// Background jobs (N3, N4, N10) are driven by NotificationBackgroundJob.
    /// </summary>
    public class NotificationTriggerService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notifications;

        public NotificationTriggerService(
            ApplicationDbContext context,
            INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        // ── N1: Task completed → notify next dept in workflow chain ────────────
        public async Task OnTaskCompletedAsync(int taskId, int projId, string? stageId)
        {
            if (string.IsNullOrEmpty(stageId)) return;

            var completedTask = await _context.Tasks
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (completedTask == null) return;

            // Find tasks in the same stage that are still pending
            var pendingInStage = await _context.Tasks
                .Include(t => t.Department)
                .Where(t => t.proj_id == projId
                         && t.stage_id == stageId
                         && t.status == "Not Started"
                         && t.task_id != taskId)
                .ToListAsync();

            if (!pendingInStage.Any()) return;

            // Collect unique departments from pending tasks
            var targetDeptIds = pendingInStage
                .Where(t => t.dept_id.HasValue)
                .Select(t => t.dept_id!.Value)
                .Distinct()
                .ToList();

            var project = await _context.Projects.FindAsync(projId);

            foreach (var deptId in targetDeptIds)
            {
                // Notify department members who are on this project's team
                var teamUserIds = await _context.ProjectTeams
                    .Where(pt => pt.proj_id == projId)
                    .Join(_context.Users,
                        pt => pt.user_id,
                        u => u.user_id,
                        (pt, u) => u)
                    .Where(u => u.dept_id == deptId && u.is_active)
                    .Select(u => u.user_id)
                    .ToListAsync();

                foreach (var uid in teamUserIds)
                {
                    await _notifications.NotifyAsync(
                        uid,
                        "handover",
                        $"Action required: {project?.proj_no}",
                        $"Task '{completedTask.title}' ({completedTask.Department?.dept_name}) is complete. " +
                        $"Your department's tasks in stage {stageId} are now unblocked.",
                        projId, taskId);
                }
            }
        }

        // ── N2: All tasks in a stage completed ──────────────────────────────────
        public async Task OnStageCompletedAsync(int projId, string stageId)
        {
            var project = await _context.Projects.FindAsync(projId);
            if (project == null) return;

            // Notify the project creator
            await _notifications.NotifyAsync(
                project.created_by,
                "stage_complete",
                $"Stage {stageId} completed — {project.proj_no}",
                $"All tasks in stage {stageId} for project '{project.proj_name}' are marked complete.",
                projId);

            // Notify all team members
            var teamUserIds = await _context.ProjectTeams
                .Where(pt => pt.proj_id == projId)
                .Select(pt => pt.user_id)
                .Distinct()
                .ToListAsync();

            foreach (var uid in teamUserIds.Where(id => id != project.created_by))
            {
                await _notifications.NotifyAsync(
                    uid,
                    "stage_complete",
                    $"Stage {stageId} completed — {project.proj_no}",
                    $"Stage {stageId} of '{project.proj_name}' is fully complete.",
                    projId);
            }
        }

        // ── N5: Project launched → notify all team members ──────────────────────
        public async Task OnProjectLaunchedAsync(int projId)
        {
            var project = await _context.Projects.FindAsync(projId);
            if (project == null) return;

            var teamUserIds = await _context.ProjectTeams
                .Where(pt => pt.proj_id == projId)
                .Select(pt => pt.user_id)
                .Distinct()
                .ToListAsync();

            foreach (var uid in teamUserIds)
            {
                await _notifications.NotifyAsync(
                    uid,
                    "project_launch",
                    $"Project launched: {project.proj_no}",
                    $"You have been assigned to '{project.proj_name}'. " +
                    $"The project is now In Progress — check your tasks.",
                    projId);
            }
        }

        // ── N6: FAI (task 5.8) completed → notify Sales ─────────────────────────
        public async Task OnFaiCompletedAsync(int projId, int taskId)
        {
            var project = await _context.Projects.FindAsync(projId);
            if (project == null) return;

            // Find Sales dept members on this project
            var salesDept = await _context.Departments
                .FirstOrDefaultAsync(d => d.dept_name == "Sales");

            if (salesDept == null) return;

            var salesUserIds = await _context.ProjectTeams
                .Where(pt => pt.proj_id == projId)
                .Join(_context.Users,
                    pt => pt.user_id,
                    u => u.user_id,
                    (pt, u) => u)
                .Where(u => u.dept_id == salesDept.dept_id && u.is_active)
                .Select(u => u.user_id)
                .ToListAsync();

            foreach (var uid in salesUserIds)
            {
                await _notifications.NotifyAsync(
                    uid,
                    "fai_complete",
                    $"FAI complete — ready for customer: {project.proj_no}",
                    $"QA has completed the First Article Inspection for '{project.proj_name}'. " +
                    $"Samples are ready for customer submission (task 5.9).",
                    projId, taskId);
            }

            // Also notify project creator
            if (!salesUserIds.Contains(project.created_by))
            {
                await _notifications.NotifyAsync(
                    project.created_by,
                    "fai_complete",
                    $"FAI complete — {project.proj_no}",
                    $"First Article Inspection is done. Sales should now submit samples to the customer.",
                    projId, taskId);
            }
        }

        // ── N7: Task date revised ───────────────────────────────────────────────
        public async Task OnTaskDatesRevisedAsync(int taskId, int projId,
            DateOnly? oldStart, DateOnly? oldEnd,
            DateOnly? newStart, DateOnly? newEnd)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            var project = await _context.Projects.FindAsync(projId);
            if (task == null || project == null) return;

            var msg = $"Task '{task.title}' in '{project.proj_name}' has been rescheduled. " +
                      $"Was: {oldStart:dd MMM} → {oldEnd:dd MMM}. " +
                      $"Now: {newStart:dd MMM} → {newEnd:dd MMM}.";

            // Notify assignee
            if (task.assigned_to.HasValue)
            {
                await _notifications.NotifyAsync(
                    task.assigned_to.Value,
                    "date_revised",
                    $"Task rescheduled: {task.task_code ?? task.title}",
                    msg, projId, taskId);
            }

            // Notify project creator if different from assignee
            if (project.created_by != task.assigned_to)
            {
                await _notifications.NotifyAsync(
                    project.created_by,
                    "date_revised",
                    $"Task rescheduled: {task.task_code ?? task.title}",
                    msg, projId, taskId);
            }
        }

        // ── N8: File upload milestone ───────────────────────────────────────────
        public async Task OnFileUploadedAsync(int projId, int uploadedByUserId)
        {
            var milestones = new[] { 10, 25, 50 };

            var count = await _context.Files
                .Where(f => f.proj_id == projId && f.is_latest)
                .CountAsync();

            if (!milestones.Contains(count)) return;

            var project = await _context.Projects.FindAsync(projId);
            if (project == null) return;

            // Notify project creator (don't notify the uploader — they know)
            if (project.created_by != uploadedByUserId)
            {
                await _notifications.NotifyAsync(
                    project.created_by,
                    "file_milestone",
                    $"File milestone: {count} files in {project.proj_no}",
                    $"Project '{project.proj_name}' now has {count} documents uploaded.",
                    projId);
            }
        }

        // ── N9: Task assigned ───────────────────────────────────────────────────
        public async Task OnTaskAssignedAsync(int taskId, int projId, int assignedToUserId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            var project = await _context.Projects.FindAsync(projId);
            if (task == null || project == null) return;

            await _notifications.NotifyAsync(
                assignedToUserId,
                "task_assigned",
                $"Task assigned to you: {task.task_code ?? task.title}",
                $"You have been assigned '{task.title}' in project '{project.proj_name}'. " +
                $"Planned: {task.planned_start_date:dd MMM} → {task.planned_end_date:dd MMM}.",
                projId, taskId);
        }



        // ── N10: Project Status Changed ───────────────────────────────────────────────────
        public async Task OnProjectStatusChangedAsync(int projId, string status)
        {
        }
    }
}