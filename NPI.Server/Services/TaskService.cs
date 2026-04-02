using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _basePath;

        public TaskService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _basePath = configuration["FileStorage:BasePath"] ?? @"D:\NPI_Projects";
        }

        private static string SanitizeFolderName(string name)
        {
            var result = name.Replace(" ", "_").Replace("/", "_");
            foreach (var c in Path.GetInvalidFileNameChars())
                result = result.Replace(c, '_');
            return result;
        }

        // ── Folder path: basePath/projects/proj_name/dept_name/ ──────────────

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

        // Synchronous overload used internally (no extra DB call needed when
        // caller already has the names in memory).
        public string GetTaskFolderPath(string projName, string? deptName)
        {
            var projectFolder = SanitizeFolderName(projName);
            var deptFolder = !string.IsNullOrWhiteSpace(deptName)
                                    ? SanitizeFolderName(deptName)
                                    : "General";
            return Path.Combine(_basePath, "projects", projectFolder, deptFolder);
        }

        // ── Queries ───────────────────────────────────────────────────────────

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

        /// <summary>
        /// Returns all tasks that belong to projects the given user is a member of
        /// (via ProjectTeams). Admins receive every task in the system.
        /// </summary>
        public async Task<List<TaskResponseDto>> GetTasksByProjectTeamsAsync(int userId, string userRole)
        {
            IQueryable<Tasks> query = _context.Tasks
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.AssignedByUser)
                .Include(t => t.TaskRevisions);

            if (!string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Get project IDs the user belongs to
                var projectIds = await _context.ProjectTeams
                    .Where(pt => pt.user_id == userId)
                    .Select(pt => pt.proj_id)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(t => projectIds.Contains(t.proj_id));
            }

            var tasks = await query.OrderBy(t => t.planned_start_date).ToListAsync();
            return tasks.Select(MapToResponseDto).ToList();
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

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

        public async Task<(bool success, string message)> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int userId)
        {
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

        public async Task<(bool success, string message)> DeleteTaskAsync(int taskId)
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

                if (task.SubTasks != null && task.SubTasks.Any())
                    return (false, "Cannot delete task with subtasks. Delete subtasks first.");

                // Resolve folder BEFORE removing from DB
                var folderPath = task.Project != null
                    ? GetTaskFolderPath(task.Project.proj_name, task.Department?.dept_name)
                    : null;

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                // Folders are now shared by department — do NOT auto-delete them,
                // as other tasks in the same dept may have files there.
                // Just report success.
                return (true, "Task deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting task: {ex.Message}");
            }
        }

        // ── Status / progress / dates ─────────────────────────────────────────

        public async Task<(bool success, string message)> UpdatePlannedDatesAsync(
            int taskId, DateOnly newStart, DateOnly newEnd, string note)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return (false, "Task not found");

            var revision = new TaskRevisions
            {
                task_id = taskId,
                old_start_date = task.planned_start_date ?? newStart,
                old_end_date = task.planned_end_date ?? newEnd,
                new_start_date = newStart,
                new_end_date = newEnd,
                note = note,
                revised_on = DateTime.Now
            };

            _context.TaskRevisions.Add(revision);

            task.planned_start_date = newStart;
            task.planned_end_date = newEnd;
            task.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, "Planned dates updated with revision history");
        }

        public async Task<(bool success, string message)> UpdateTaskStatusAsync(int taskId, string status)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
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

                if (status == "In Progress" && task.actual_start_date == null)
                    task.actual_start_date = DateOnly.FromDateTime(DateTime.Now);

                if (status == "Completed" && task.actual_end_date == null)
                {
                    task.actual_end_date = DateOnly.FromDateTime(DateTime.Now);
                    task.completed_at = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return (true, "Task status updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating task status: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateTaskProgressAsync(int taskId, float progress)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                    return (false, "Task not found");

                if (progress < 0 || progress > 100)
                    return (false, "Progress must be between 0 and 100");

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

        // ── Legacy user / dept filters (kept for other callers) ───────────────

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

        // ── Mapper ────────────────────────────────────────────────────────────

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
                    note = r.note,
                    revised_on = r.revised_on
                }).ToList() ?? new List<TaskRevisionDto>()
        };
    }
}