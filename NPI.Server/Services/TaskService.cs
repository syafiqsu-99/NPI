using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;
using System.Text.RegularExpressions;

namespace NPI.Server.Services
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetAllTasksAsync();
        Task<TaskResponseDto?> GetTaskByIdAsync(int taskId);
        Task<(bool success, string message, int taskId)> CreateTaskAsync(CreateTaskDto dto, int userId);
        Task<(bool success, string message)> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int userId, string userRole, int userDeptId);
        Task<(bool success, string message)> DeleteTaskAsync(int taskId, int userId, string userRole, int userDeptId);
        Task<(bool success, string message)> UpdateTaskStatusAsync(int taskId, string status, int userId, string userRole, int userDeptId);
        Task<(bool success, string message)> UpdateTaskProgressAsync(int taskId, float per_complete, int userId, string userRole, int userDeptId);
        Task<(bool success, string message)> UpdatePlannedDatesAsync(int taskId, DateOnly? new_start_date, DateOnly? new_end_date, string? note, int userId, string userRole, int userDeptId);
        Task<(bool authorized, string message)> ValidateTaskWriteAccessAsync(int taskId, int userId, string userRole, int userDeptId);
        Task<List<TaskResponseDto>> GetTasksByProjectAsync(int projectId);
        Task<List<TaskResponseDto>> GetTasksByUserAsync(int userId);
        Task<List<TaskResponseDto>> GetTasksByDepartmentAsync(int deptId);
        Task<string?> GetTaskFolderPathAsync(int taskId);
        string GetTaskFolderPath(string projName, string? deptName);
        Task<List<TaskResponseDto>> GetTasksByProjectTeamsAsync(int userId, string userRole);
        Task<List<TaskCommentResponseDto>> GetTaskCommentsAsync(int taskId);
        Task<List<object>> GetMentionableUsersAsync(int taskId);
        Task<(bool success, string message, TaskCommentResponseDto? comment)> AddTaskCommentAsync(int taskId, string body, int userId);
        Task<(bool success, string message)> DeleteTaskCommentAsync(int commentId, int userId, string userRole);
    }

    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly string _basePath;

        public TaskService(ApplicationDbContext context, IConfiguration configuration, INotificationService notificationService)
        {
            _context = context;
            _configuration = configuration;
            _basePath = configuration["FileStorage:BasePath"]
                        ?? throw new InvalidOperationException(
                            "FileStorage:BasePath not configured.");
            _notificationService = notificationService;
        }

        public async Task<(bool authorized, string message)> ValidateTaskWriteAccessAsync(int taskId, int userId, string userRole, int userDeptId)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (task == null)
                return (false, "Task not found");

            if (RbacHelper.IsAdminOrManager(userRole))
            {
                return (true, string.Empty);
            }

            var projectTeamRole = await _context.ProjectTeams
                .Where(pt => pt.proj_id == task.proj_id && pt.user_id == userId)
                .FirstOrDefaultAsync();

            if (projectTeamRole == null)
            {
                return (false, "Unauthorized: You are not a member of this project team.");
            }

            if (string.Equals(projectTeamRole.role, "Viewer", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Unauthorized: You have 'Viewer' access on this project and cannot modify tasks.");
            }

            if (string.Equals(projectTeamRole.role, "Team Lead", StringComparison.OrdinalIgnoreCase))
                return (true, string.Empty);

            if (task.dept_id != userDeptId)
                return (false, "Unauthorized: You can only modify tasks assigned to your department.");

            return (true, string.Empty);
        }

        private static string SanitizeFolderName(string name)
        {
            var result = name.Replace(" ", "_").Replace("/", "_");
            foreach (var c in Path.GetInvalidFileNameChars())
                result = result.Replace(c, '_');
            return result;
        }

        public async Task<string?> GetTaskFolderPathAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            if (task?.Project == null)
                return null;

            var projectFolder = SanitizeFolderName(task.Project.proj_name);
            var deptFolder = task.Department != null
                                    ? SanitizeFolderName(task.Department.dept_name)
                                    : "General";

            return Path.Combine(_basePath, "projects", projectFolder, deptFolder);
        }

        public string GetTaskFolderPath(string projName, string? deptName)
        {
            var projectFolder = SanitizeFolderName(projName);
            var deptFolder = !string.IsNullOrWhiteSpace(deptName)
                                    ? SanitizeFolderName(deptName)
                                    : "General";
            return Path.Combine(_basePath, "projects", projectFolder, deptFolder);
        }

        public async Task<List<TaskResponseDto>> GetAllTasksAsync()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AssignedByUser)
                .Include(t => t.TaskRevisions)
                .OrderBy(t => t.planned_start_date)
                .ToListAsync();

            return tasks.Select(MapToResponseDto).ToList();
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AssignedByUser)
                .Include(t => t.TaskRevisions)
                .FirstOrDefaultAsync(t => t.task_id == taskId);

            return task == null ? null : MapToResponseDto(task);
        }

        public async Task<List<TaskResponseDto>> GetTasksByProjectAsync(int projId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AssignedByUser)
                .Include(t => t.TaskRevisions)
                .Where(t => t.proj_id == projId)
                .OrderBy(t => t.planned_start_date)
                .ToListAsync();

            return tasks.Select(MapToResponseDto).ToList();
        }

        public async Task<List<TaskResponseDto>> GetTasksByProjectTeamsAsync(int userId, string userRole)
        {
            IQueryable<Tasks> query = _context.Tasks
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AssignedByUser)
                .Include(t => t.TaskRevisions);

            bool hasGlobalAccess = RbacHelper.IsAdminOrManager(userRole); ;

            if (!hasGlobalAccess)
            {
                var projectIds = _context.ProjectTeams
                    .Where(pt => pt.user_id == userId)
                    .Select(pt => pt.proj_id)
                    .Distinct();

                query = query.Where(t => projectIds.Contains(t.proj_id));
            }

            var tasks = await query.OrderBy(t => t.planned_start_date).ToListAsync();

            return tasks.Select(MapToResponseDto).ToList();
        }

        public async Task<(bool success, string message, int taskId)> CreateTaskAsync(CreateTaskDto dto, int userId)
        {
            try
            {
                var task = new Tasks
                {
                    proj_id = dto.proj_id,
                    parent_task_id = dto.parent_task_id,
                    title = dto.title,
                    description = dto.description,
                    planned_start_date = dto.planned_start_date,
                    planned_end_date = dto.planned_end_date,
                    actual_start_date = dto.actual_start_date,
                    actual_end_date = dto.actual_end_date,
                    duration = dto.duration,
                    per_complete = dto.per_complete ?? 0,
                    dept_id = dto.dept_id,
                    assigned_to = dto.assigned_to,
                    assigned_by = userId,
                    priority = dto.priority ?? "Medium",
                    status = dto.status ?? "Not Started",
                    created_at = DateTime.Now
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                if (dto.assigned_to.HasValue)
                    await _notificationService.OnTaskAssignedAsync(task.task_id, task.proj_id, dto.assigned_to.Value);

                var folderPath = await GetTaskFolderPathAsync(task.task_id);
                if (folderPath != null)
                    Directory.CreateDirectory(folderPath);

                return (true, "Task created successfully", task.task_id);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating task: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int userId, string userRole, int userDeptId)
        {
            var (authorized, authMessage) = await ValidateTaskWriteAccessAsync(taskId, userId, userRole, userDeptId);
            if (!authorized)
                return (false, authMessage);

            try
            {
                var task = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.Department)
                    .FirstOrDefaultAsync(t => t.task_id == taskId);

                if (task == null)
                    return (false, "Task not found");

                task.parent_task_id = dto.parent_task_id;
                task.title = dto.title;
                task.description = dto.description;
                task.planned_start_date = dto.planned_start_date;
                task.planned_end_date = dto.planned_end_date;
                task.actual_start_date = dto.actual_start_date;
                task.actual_end_date = dto.actual_end_date;
                task.duration = dto.duration;
                task.per_complete = dto.per_complete ?? task.per_complete;
                task.dept_id = dto.dept_id;
                task.assigned_to = dto.assigned_to;
                task.priority = dto.priority ?? task.priority;
                task.status = dto.status ?? task.status;
                task.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return (true, "Task updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating task: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteTaskAsync(
            int taskId, int userId, string userRole, int userDeptId)
        {
            try
            {
                var task = await _context.Tasks
                    .Include(t => t.SubTasks)
                    .Include(t => t.Project)
                    .Include(t => t.Department)
                    .FirstOrDefaultAsync(t => t.task_id == taskId);

                if (task == null)
                    return (false, "Task not found");

                if (!RbacHelper.IsAdminOrManager(userRole))
                {
                    var isTeamLead = await _context.ProjectTeams.AnyAsync(pt =>
                        pt.proj_id == task.proj_id &&
                        pt.user_id == userId &&
                        pt.role == ProjectRoleNames.TeamLead);

                    if (!isTeamLead)
                        return (false, "Unauthorized: Only Admins, Managers, or the project's Team Lead can delete tasks.");
                }

                if (task.SubTasks != null && task.SubTasks.Any())
                    return (false, "Cannot delete task with subtasks. Delete subtasks first.");

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return (true, "Task deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting task: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdatePlannedDatesAsync(int taskId, DateOnly? new_start_date, DateOnly? new_end_date, string? note, int userId, string userRole, int userDeptId)
        {
            var (authorized, authMessage) = await ValidateTaskWriteAccessAsync(taskId, userId, userRole, userDeptId);
            if (!authorized)
                return (false, authMessage);

            var task = await _context.Tasks.FindAsync(taskId);

            if (task == null)
                return (false, "Task not found");

            var revision = new TaskRevisions
            {
                task_id = taskId,
                old_start_date = task.planned_start_date ?? new_start_date,
                old_end_date = task.planned_end_date ?? new_end_date,
                new_start_date = new_start_date,
                new_end_date = new_end_date,
                note = note,
                revised_on = DateTime.Now
            };

            _context.TaskRevisions.Add(revision);

            task.planned_start_date = new_start_date;
            task.planned_end_date = new_end_date;
            task.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();

            await _notificationService.OnTaskDatesRevisedAsync(
                taskId, task.proj_id,
                task.planned_start_date, task.planned_end_date,
                new_start_date, new_end_date);

            return (true, "Planned dates updated with revision history");
        }

        public async Task<(bool success, string message)> UpdateTaskStatusAsync(int taskId, string status, int userId, string userRole, int userDeptId)
        {
            try
            {
                var (authorized, authMessage) = await ValidateTaskWriteAccessAsync(taskId, userId, userRole, userDeptId);
                if (!authorized)
                    return (false, authMessage);

                var task = await _context.Tasks.FindAsync(taskId);
                if (task is null)
                    return (false, "Task not found");

                var validStatuses = new[] { "Not Started", "In Progress", "On Hold", "Completed", "Cancelled" };
                if (!validStatuses.Contains(status))
                    return (false, "Invalid status value");

                task.status = status;
                task.updated_at = DateTime.Now;

                if (status == "Completed" && task.per_complete < 100)
                    task.per_complete = 100;
                else if (status == "In Progress" && task.per_complete == 0)
                    task.per_complete = 10;
                else if (status == "Not Started")
                    task.per_complete = 0;

                if (status == "In Progress" && task.actual_start_date is null)
                    task.actual_start_date = DateOnly.FromDateTime(DateTime.Now);

                if (status == "Completed" && task.actual_end_date is null)
                {
                    task.actual_end_date = DateOnly.FromDateTime(DateTime.Now);
                    task.completed_at = DateTime.Now;
                }

                var auditRevision = new TaskRevisions
                {
                    revision_id = null,
                    task_id = taskId,
                    title = task.title,
                    old_start_date = task.planned_start_date,
                    old_end_date = task.planned_end_date,
                    new_start_date = task.planned_start_date,
                    new_end_date = task.planned_end_date,
                    note = $"Status changed to '{status}' by user {userId}",
                    revised_on = DateTime.Now,
                    status = status,
                    dept_id = task.dept_id
                };

                _context.TaskRevisions.Add(auditRevision);
                await _context.SaveChangesAsync();

                if (status == "Completed")
                {
                    await _notificationService.OnTaskCompletedAsync(taskId, task.proj_id, task.stage_id);

                    if (!string.IsNullOrEmpty(task.stage_id))
                    {
                        bool stageComplete = !await _context.Tasks.AnyAsync(t =>
                            t.proj_id == task.proj_id &&
                            t.stage_id == task.stage_id &&
                            t.status != "Completed" &&
                            t.task_id != taskId);

                        if (stageComplete)
                            await _notificationService.OnStageCompletedAsync(task.proj_id, task.stage_id);
                    }
                }

                return (true, "Task status updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating task status: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateTaskProgressAsync(int taskId, float progress, int userId, string userRole, int userDeptId)
        {
            var (authorized, authMessage) = await ValidateTaskWriteAccessAsync(taskId, userId, userRole, userDeptId);
            if (!authorized)
                return (false, authMessage);

            try
            {
                var task = await _context.Tasks.FindAsync(taskId);

                if (task == null)
                {
                    return (false, "Task not found");
                }

                if (progress < 0 || progress > 100)
                {
                    return (false, "Progress must be between 0 and 100");
                }

                task.per_complete = progress;
                task.updated_at = DateTime.Now;

                if (progress == 100 && task.status != "Completed")
                {
                    task.status = "Completed";
                    task.actual_end_date = DateOnly.FromDateTime(DateTime.Now);
                    task.completed_at = DateTime.Now;
                }
                else if (progress > 0 && progress < 100 && task.status == "Not Started")
                {
                    task.status = "In Progress";
                    task.actual_start_date ??= DateOnly.FromDateTime(DateTime.Now);
                }
                else if (progress == 0 && task.status != "Not Started")
                {
                    task.status = "Not Started";
                    task.actual_start_date = null;
                }

                await _context.SaveChangesAsync();

                return (true, "Task progress updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating task progress: {ex.Message}");
            }
        }

        public async Task<List<TaskResponseDto>> GetTasksByUserAsync(int userId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.assigned_to == userId)
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AssignedByUser)
                .Include(t => t.TaskRevisions)
                .OrderBy(t => t.planned_start_date)
                .ToListAsync();

            return tasks.Select(MapToResponseDto).ToList();
        }

        public async Task<List<TaskResponseDto>> GetTasksByDepartmentAsync(int deptId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.dept_id == deptId)
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AssignedByUser)
                .Include(t => t.TaskRevisions)
                .OrderBy(t => t.planned_start_date)
                .ToListAsync();

            return tasks.Select(MapToResponseDto).ToList();
        }

        private static TaskResponseDto MapToResponseDto(Tasks task) => new()
        {
            task_id = task.task_id,
            proj_id = task.proj_id,
            proj_no = task.Project?.proj_no,
            proj_name = task.Project?.proj_name,
            proj_status = task.Project?.status,
            proj_priority = task.Project?.priority,
            parent_task_id = task.parent_task_id,
            order = 0,
            stage_id = task.stage_id,
            task_code = task.task_code,
            title = task.title,
            description = task.description,
            dept_id = task.dept_id,
            dept_name = task.Department?.dept_name,
            assigned_to = task.assigned_to,
            assigned_to_name = task.AssignedToUser?.full_name,
            assigned_by = task.assigned_by,
            assigned_by_name = task.AssignedByUser?.full_name,
            planned_start_date = task.planned_start_date,
            planned_end_date = task.planned_end_date,
            actual_start_date = task.actual_start_date,
            actual_end_date = task.actual_end_date,
            duration = task.duration,
            status = task.status,
            priority = task.priority,
            per_complete = task.per_complete,
            created_at = task.created_at,
            updated_at = task.updated_at,
            completed_at = task.completed_at,
            planned_revisions = task.TaskRevisions?
                .OrderBy(r => r.revised_on)
                .Select((r, index) => new TaskRevisionDto
                {
                    revision_no = index + 1,
                    old_start_date = r.old_start_date,
                    old_end_date = r.old_end_date,
                    new_start_date = r.new_start_date,
                    new_end_date = r.new_end_date,
                    revision_note = r.note,
                    revised_on = r.revised_on
                }).ToList() ?? new List<TaskRevisionDto>()
        };

        public async Task<List<TaskCommentResponseDto>> GetTaskCommentsAsync(int taskId)
        {
            return await _context.TaskComments
                .Where(c => c.task_id == taskId && !c.is_deleted)
                .OrderBy(c => c.created_at)
                .Select(c => new TaskCommentResponseDto
                {
                    comment_id = c.comment_id,
                    task_id = c.task_id,
                    user_id = c.user_id,
                    username = c.User!.username,
                    full_name = c.User!.full_name,
                    body = c.body,
                    created_at = c.created_at
                })
                .ToListAsync();
        }

        public async Task<List<object>> GetMentionableUsersAsync(int taskId)
        {
            var projId = await _context.Tasks
                .Where(t => t.task_id == taskId)
                .Select(t => t.proj_id)
                .FirstOrDefaultAsync();

            var teamUsers = await _context.ProjectTeams
                .Where(pt => pt.proj_id == projId)
                .Select(pt => new { pt.User!.username, pt.User!.full_name })
                .Distinct()
                .OrderBy(u => u.username)
                .ToListAsync();

            var result = new List<object>
            {
                new { username = "all", full_name = "Everyone on this project" }
            };
            result.AddRange(teamUsers);
            return result;
        }

        public async Task<(bool success, string message, TaskCommentResponseDto? comment)>
            AddTaskCommentAsync(int taskId, string body, int userId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task is null)
                return (false, "Task not found", null);

            var trimmed = body?.Trim() ?? string.Empty;
            if (trimmed.Length == 0)
                return (false, "Comment cannot be empty", null);

            var comment = new TaskComments
            {
                task_id = taskId,
                user_id = userId,
                body = trimmed,
                created_at = DateTime.Now
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            await NotifyMentionsAsync(trimmed, taskId, task.proj_id, userId);

            var author = await _context.Users.FindAsync(userId);
            return (true, "Comment added", new TaskCommentResponseDto
            {
                comment_id = comment.comment_id,
                task_id = taskId,
                user_id = userId,
                username = author?.username,
                full_name = author?.full_name,
                body = comment.body,
                created_at = comment.created_at
            });
        }

        public async Task<(bool success, string message)>
            DeleteTaskCommentAsync(int commentId, int userId, string userRole)
        {
            var comment = await _context.TaskComments.FindAsync(commentId);
            if (comment is null || comment.is_deleted)
                return (false, "Comment not found");

            if (comment.user_id != userId && !RbacHelper.IsAdminOrManager(userRole))
                return (false, "You are not authorised to delete this comment");

            comment.is_deleted = true;
            comment.deleted_by = userId;
            comment.deleted_at = DateTime.Now;
            await _context.SaveChangesAsync();

            return (true, "Comment deleted");
        }

        private async Task NotifyMentionsAsync(string body, int taskId, int projId, int authorId)
        {
            var handles = Regex.Matches(body, @"@([A-Za-z0-9._-]+)")
                .Select(m => m.Groups[1].Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (handles.Count == 0) return;

            var author = await _context.Users.FindAsync(authorId);
            var mentionsAll = handles.Any(h => h.Equals("all", StringComparison.OrdinalIgnoreCase));

            List<int> mentionedIds;

            if (mentionsAll)
            {
                mentionedIds = await _context.ProjectTeams
                    .Where(pt => pt.proj_id == projId && pt.user_id != authorId)
                    .Select(pt => pt.user_id)
                    .Distinct()
                    .ToListAsync();
            }
            else
            {
                mentionedIds = await _context.Users
                    .Where(u => u.is_active
                        && u.user_id != authorId
                        && handles.Contains(u.username))
                    .Select(u => u.user_id)
                    .ToListAsync();
            }

            if (mentionedIds.Count == 0) return;

            var title = mentionsAll
                ? "The whole team was mentioned in a task comment"
                : "You were mentioned in a task comment";

            await _notificationService.NotifyManyAsync(
                mentionedIds,
                NotificationTypes.TaskComment,
                title,
                $"{author?.username ?? "Someone"} mentioned you.",
                projId,
                taskId);
        }
    }
}