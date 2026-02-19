using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Models;
using System.Threading.Tasks;

namespace NPI.Server.Services
{
    public class GanttService : IGanttService
    {
        private readonly ApplicationDbContext _context;

        public GanttService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GanttDataDto> GetGanttDataAsync(int projectId, int? revisionId = null)
        {
            var project = await _context.Projects
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.proj_id == projectId);

            if (project == null)
                return null;

            List<GanttTaskDto> tasks;
            int? currentRevisionId = null;
            int? revisionNumber = null;

            if (revisionId.HasValue)
            {
                var revision = await _context.ProjectRevisions
                    .FirstOrDefaultAsync(r => r.revision_id == revisionId.Value);

                if (revision != null)
                {
                    currentRevisionId = revision.revision_id;
                    revisionNumber = revision.revision_number;

                    var taskRevisions = await _context.TaskRevisions
                        .Include(tr => tr.Task)
                            .ThenInclude(t => t.Department)
                        .Where(tr => tr.revision_id == revisionId.Value)
                        .ToListAsync();

                    tasks = taskRevisions.Select(tr => new GanttTaskDto
                    {
                        task_id = tr.task_id,
                        title = tr.title ?? tr.Task?.title,
                        dept_name = tr.Task?.Department?.dept_name,
                        dept_color = GetDepartmentColor(tr.Task?.Department?.dept_name),
                        planned_start_date = tr.new_start_date,
                        planned_end_date = tr.new_end_date,
                        duration = tr.duration,
                        per_complete = 0,
                        status = tr.status ?? "Not Started"
                    }).ToList();
                }
                else
                {
                    tasks = new List<GanttTaskDto>();
                }
            }
            else
            {
                var currentTasks = await _context.Tasks
                    .Include(t => t.Department)
                    .Where(t => t.proj_id == projectId)
                    .OrderBy(t => t.planned_start_date)
                    .ToListAsync();

                tasks = currentTasks.Select(t => new GanttTaskDto
                {
                    task_id = t.task_id,
                    title = t.title,
                    dept_name = t.Department?.dept_name,
                    dept_color = GetDepartmentColor(t.Department?.dept_name),
                    planned_start_date = t.planned_start_date,
                    planned_end_date = t.planned_end_date,
                    duration = t.duration,
                    per_complete = t.per_complete,
                    status = t.status,
                    parent_task_id = t.parent_task_id
                }).ToList();
            }

            return new GanttDataDto
            {
                project = new ProjectResponseDto
                {
                    proj_id = project.proj_id,
                    proj_no = project.proj_no,
                    proj_name = project.proj_name,
                    customer_name = project.Customer?.comp_name,
                    project_start_date = project.project_start_date,
                    target_completion_date = project.target_completion_date,
                    priority = project.priority,
                    status = project.status
                },
                current_revision_id = currentRevisionId,
                revision_number = revisionNumber,
                tasks = tasks
            };
        }

        public async Task<List<ProjectRevisionDto>> GetProjectRevisionsAsync(int projectId)
        {
            var revisions = await _context.ProjectRevisions
                .Include(r => r.RevisedByUser)
                .Where(r => r.proj_id == projectId)
                .OrderByDescending(r => r.revision_number)
                .ToListAsync();

            return revisions.Select(r => new ProjectRevisionDto
            {
                revision_id = r.revision_id,
                revision_number = r.revision_number,
                revision_date = r.revision_date,
                revised_by_name = r.RevisedByUser?.username,
                revision_notes = r.revision_notes,
                previous_target_date = r.previous_target_date,
                new_target_date = r.new_target_date,
                is_active = r.is_active
            }).ToList();
        }

        public async Task<(bool success, string message, int? revisionId)> CreateRevisionAsync(
            int projectId, CreateRevisionDto dto, int userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return (false, "Project not found", null);

                var lastRevision = await _context.ProjectRevisions
                    .Where(r => r.proj_id == projectId)
                    .OrderByDescending(r => r.revision_number)
                    .FirstOrDefaultAsync();

                var newRevisionNumber = (lastRevision?.revision_number ?? 0) + 1;

                var oldTargetDate = project.target_completion_date;

                var previousRevisions = await _context.ProjectRevisions
                    .Where(r => r.proj_id == projectId && r.is_active)
                    .ToListAsync();

                foreach (var prevRevision in previousRevisions)
                {
                    prevRevision.is_active = false;
                }

                var revision = new ProjectRevisions
                {
                    proj_id = projectId,
                    revision_number = newRevisionNumber,
                    revision_date = DateTime.Now,
                    revised_by = userId,
                    revision_notes = dto.revision_notes,
                    previous_target_date = oldTargetDate,
                    is_active = true
                };

                _context.ProjectRevisions.Add(revision);
                await _context.SaveChangesAsync();

                var currentTasks = await _context.Tasks
                    .Where(t => t.proj_id == projectId)
                    .ToListAsync();

                foreach (var task in currentTasks)
                {
                    var taskRevision = new TaskRevisions
                    {
                        revision_id = revision.revision_id,
                        task_id = task.task_id,
                        title = task.title,
                        old_start_date = task.planned_start_date,
                        old_end_date = task.planned_end_date,
                        new_start_date = task.planned_start_date,
                        new_end_date = task.planned_end_date,
                        duration = task.duration,
                        dept_id = task.dept_id,
                        status = task.status,
                        revised_on = DateTime.Now
                    };

                    _context.TaskRevisions.Add(taskRevision);
                }

                await _context.SaveChangesAsync();

                foreach (var taskUpdate in dto.tasks)
                {
                    if (taskUpdate.task_id.HasValue)
                    {
                        var task = currentTasks.FirstOrDefault(t => t.task_id == taskUpdate.task_id.Value);
                        if (task != null)
                        {
                            task.title = taskUpdate.title;
                            task.planned_start_date = taskUpdate.start_date;
                            task.planned_end_date = taskUpdate.end_date;
                            task.duration = taskUpdate.duration;

                            if (!string.IsNullOrEmpty(taskUpdate.dept_name))
                            {
                                var dept = await _context.Departments
                                    .FirstOrDefaultAsync(d => d.dept_name == taskUpdate.dept_name);
                                task.dept_id = dept?.dept_id;
                            }

                            task.updated_at = DateTime.Now;

                            var taskRevision = await _context.TaskRevisions
                                .FirstOrDefaultAsync(tr => tr.revision_id == revision.revision_id
                                    && tr.task_id == task.task_id);

                            if (taskRevision != null)
                            {
                                taskRevision.new_start_date = taskUpdate.start_date;
                                taskRevision.new_end_date = taskUpdate.end_date;
                                taskRevision.duration = taskUpdate.duration;
                                taskRevision.title = taskUpdate.title;
                            }
                        }
                    }
                }

                var newTargetDate = dto.tasks
                    .Where(t => t.end_date.HasValue)
                    .Max(t => (DateOnly?)t.end_date);

                if (newTargetDate.HasValue)
                {
                    project.target_completion_date = newTargetDate.Value;
                    revision.new_target_date = newTargetDate.Value;
                }

                project.updated_at = DateTime.Now;
                project.updated_by = userId;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return (true, "Revision created successfully", revision.revision_id);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Error creating revision: {ex.Message}", null);
            }
        }
        private string GetDepartmentColor(string? deptName)
        {
            if (string.IsNullOrEmpty(deptName)) return "#808080";

            var colors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Sales", "#2196F3" },
                { "Technical", "#4CAF50" },
                { "Purchaser", "#FF9800" },
                { "Purchasing", "#FF9800" },
                { "QA", "#9C27B0" },
                { "Production", "#F44336" },
                { "Others", "#607D8B" }
            };

            return colors.TryGetValue(deptName, out var color) ? color : "#808080";
        }
    }
}
