using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _basePath;

        public ProjectService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _basePath = configuration["FileStorage:BasePath"] ?? @"D:\NPI_Projects";
        }

        public async Task<List<ProjectResponseDto>> GetAllProjectsAsync()
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Departments)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();

            return projects.Select(p => new ProjectResponseDto
            {
                proj_id = p.proj_id,
                proj_no = p.proj_no,
                proj_name = p.proj_name,
                cust_id = p.cust_id,
                customer_name = p.Customer?.comp_name,
                dept_id = p.dept_id,
                dept_name = p.Departments != null ? string.Join(", ", p.Departments.Select(d => d.dept_name)) : null,
                project_start_date = p.project_start_date,
                target_completion_date = p.target_completion_date,
                actual_completion_date = p.actual_completion_date,
                priority = p.priority,
                status = p.status,
                description = p.description,
                storage_path = p.storage_path,
                created_at = p.created_at,
                updated_at = p.updated_at
            }).ToList();
        }

        public async Task<ProjectResponseDto?> GetProjectByIdAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Departments)
                .Include(p => p.ProjectTeams)
                    .ThenInclude(tm => tm.User)
                        .ThenInclude(u => u.Department)
                .FirstOrDefaultAsync(p => p.proj_id == projectId);

            if (project == null)
                return null;

            return new ProjectResponseDto
            {
                proj_id = project.proj_id,
                proj_no = project.proj_no,
                proj_name = project.proj_name,
                cust_id = project.cust_id,
                customer_name = project.Customer?.comp_name,
                dept_id = project.dept_id,
                dept_name = project.Departments != null ? string.Join(", ", project.Departments.Select(d => d.dept_name)) : null,
                project_start_date = project.project_start_date,
                target_completion_date = project.target_completion_date,
                actual_completion_date = project.actual_completion_date,
                priority = project.priority,
                status = project.status,
                description = project.description,
                storage_path = project.storage_path,
                created_at = project.created_at,
                updated_at = project.updated_at,
                team_members = project.ProjectTeams.Select(tm => new TeamMemberDto
                {
                    user_id = tm.user_id,
                    dept_id = tm.User?.dept_id ?? 0,
                    dept_name = tm.User?.Department?.dept_name,
                    role = tm.role ?? "Team Member",
                    user_name = tm.User?.username
                }).ToList()
            };
        }

        public async Task<(bool success, string message, int projId)> CreateProjectAsync(CreateProjectDto dto, int userId)
        {
            try
            {
                var year = DateTime.Now.Year;
                var lastProject = await _context.Projects
                    .Where(p => p.proj_no.StartsWith($"PRJ{year}"))
                    .OrderByDescending(p => p.proj_id)
                    .FirstOrDefaultAsync();

                string projectNo = $"PRJ{year}001";
                if (lastProject != null)
                {
                    var lastNumStr = lastProject.proj_no.Substring(7);
                    if (int.TryParse(lastNumStr, out int lastNumber))
                    {
                        projectNo = $"PRJ{year}{(lastNumber + 1):D3}";
                    }
                }

                var project = new Projects
                {
                    proj_no = projectNo,
                    proj_name = dto.proj_name,
                    description = dto.description,
                    dept_id = dto.dept_id,
                    priority = dto.priority ?? "Medium",
                    status = dto.status ?? "Planning",
                    project_start_date = dto.project_start_date ?? DateOnly.FromDateTime(DateTime.Now),
                    target_completion_date = dto.target_completion_date,
                    created_by = userId,
                    created_at = DateTime.Now
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // Create folder structure
                var projectPath = Path.Combine(_basePath, "projects", project.proj_id.ToString());
                Directory.CreateDirectory(projectPath);

                var departments = await _context.Departments.ToListAsync();
                foreach (var dept in departments)
                {
                    var deptFolderName = dept.dept_name.ToLower().Replace(" ", "_");
                    var deptPath = Path.Combine(projectPath, deptFolderName);
                    Directory.CreateDirectory(deptPath);
                }

                project.storage_path = projectPath;
                await _context.SaveChangesAsync();

                return (true, "Project created successfully", project.proj_id);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating project: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> UpdateProjectAsync(int projectId, UpdateProjectDto dto, int userId)
        {
            try
            {
                var project = await _context.Projects.FindAsync(projectId);

                if (project == null)
                    return (false, "Project not found");

                project.proj_name = dto.proj_name;
                project.description = dto.description;
                project.dept_id = dto.dept_id;
                project.priority = dto.priority ?? project.priority;
                project.status = dto.status ?? project.status;
                project.project_start_date = dto.project_start_date ?? project.project_start_date;
                project.target_completion_date = dto.target_completion_date ?? project.target_completion_date;
                project.updated_at = DateTime.Now;
                project.updated_by = userId;

                await _context.SaveChangesAsync();

                return (true, "Project updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating project: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteProjectAsync(int projectId)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Tasks)
                    .Include(p => p.ProjectTeams)
                    .Include(p => p.Milestones)
                    .FirstOrDefaultAsync(p => p.proj_id == projectId);

                if (project == null)
                    return (false, "Project not found");

                // Check if project has active tasks
                if (project.Tasks != null && project.Tasks.Any(t => t.status != "Cancelled" && t.status != "Completed"))
                {
                    return (false, "Cannot delete project with active tasks. Complete or cancel all tasks first.");
                }

                // Remove related entities
                if (project.Tasks != null)
                    _context.Tasks.RemoveRange(project.Tasks);

                if (project.ProjectTeams != null)
                    _context.ProjectTeams.RemoveRange(project.ProjectTeams);

                if (project.Milestones != null)
                    _context.Milestones.RemoveRange(project.Milestones);

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                return (true, "Project deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting project: {ex.Message}");
            }
        }

        public async Task<(bool success, string message, int? proj_id)> CreateProjectFromEnquiryAsync(int enquiryId, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var enquiry = await _context.Enquiries
                    .Include(e => e.Customer)
                    .Include(e => e.GeneralInfo)
                    .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId);

                if (enquiry == null)
                {
                    return (false, "Enquiry not found", null);
                }

                if (enquiry.status != "Submitted")
                {
                    return (false, "Only approved enquiries can be converted to projects", null);
                }

                if (enquiry.proj_id != null)
                {
                    return (false, "Project already exists for this enquiry", null);
                }

                // Generate project number
                var year = DateTime.Now.Year;
                var lastProject = await _context.Projects
                    .Where(p => p.proj_no.StartsWith($"PRJ{year}"))
                    .OrderByDescending(p => p.proj_id)
                    .FirstOrDefaultAsync();

                string projectNo = $"PRJ{year}001";
                if (lastProject != null)
                {
                    var lastNumStr = lastProject.proj_no.Substring(7);
                    if (int.TryParse(lastNumStr, out int lastNumber))
                    {
                        projectNo = $"PRJ{year}{(lastNumber + 1):D3}";
                    }
                }

                var projectName = enquiry.GeneralInfo?.company_name ?? enquiry.Customer?.comp_name ?? "New Project";
                projectName = $"{projectName} - {enquiry.npi_category}";

                var project = new Projects
                {
                    proj_no = projectNo,
                    proj_name = projectName,
                    cust_id = enquiry.cust_id,
                    enquiry_date = DateOnly.FromDateTime(enquiry.created_at),
                    project_start_date = DateOnly.FromDateTime(DateTime.Now),
                    target_completion_date = enquiry.GeneralInfo?.estimated_required_date,
                    priority = "Medium",
                    status = "Planning",
                    description = $"NPI Project for {enquiry.npi_category}",
                    created_by = userId,
                    created_at = DateTime.Now
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                var projectId = project.proj_id;

                enquiry.proj_id = projectId;

                var projectPath = Path.Combine(_basePath, "projects", projectId.ToString());

                // Get all departments from database
                var departments = await _context.Departments.ToListAsync();

                foreach (var dept in departments)
                {
                    var deptFolderName = dept.dept_name.ToLower().Replace(" ", "_");
                    var deptPath = Path.Combine(projectPath, deptFolderName);
                    Directory.CreateDirectory(deptPath);
                }

                project.storage_path = projectPath;

                var salesDept = departments.FirstOrDefault(d =>
                    d.dept_name.Contains("Sales", StringComparison.OrdinalIgnoreCase));
                var technicalDept = departments.FirstOrDefault(d =>
                    d.dept_name.Contains("Technical", StringComparison.OrdinalIgnoreCase));
                var purchaserDept = departments.FirstOrDefault(d =>
                    d.dept_name.Contains("Purchaser", StringComparison.OrdinalIgnoreCase) ||
                    d.dept_name.Contains("Purchasing", StringComparison.OrdinalIgnoreCase));

                var defaultTasks = new[]
                {
                    new { order = 1, title = "Project awarded/Contract signing", dept_id = salesDept?.dept_id, duration = 2.0f },
                    new { order = 2, title = "Drawing preparation", dept_id = technicalDept?.dept_id, duration = 7.0f },
                    new { order = 3, title = "Drawing submission to customer", dept_id = salesDept?.dept_id, duration = 1.0f },
                    new { order = 4, title = "DFM preparation", dept_id = technicalDept?.dept_id, duration = 5.0f },
                    new { order = 5, title = "DFM submission to customer", dept_id = technicalDept?.dept_id, duration = 1.0f },
                    new { order = 6, title = "Customer drawing approval", dept_id = salesDept?.dept_id, duration = 7.0f },
                    new { order = 7, title = "PO issuance from customer", dept_id = salesDept?.dept_id, duration = 3.0f },
                    new { order = 8, title = "PO issuance to supplier", dept_id = purchaserDept?.dept_id, duration = 2.0f }
                };

                var startDate = DateOnly.FromDateTime(DateTime.Now);
                DateOnly? previousEndDate = null;
                var createdTasks = new List<Tasks>();

                foreach (var taskTemplate in defaultTasks)
                {
                    var taskStartDate = previousEndDate?.AddDays(1) ?? startDate;
                    var taskEndDate = taskStartDate.AddDays((int)taskTemplate.duration);

                    var task = new Tasks
                    {
                        proj_id = projectId,
                        title = taskTemplate.title,
                        description = $"Default NPI task: {taskTemplate.title}",
                        planned_start_date = taskStartDate,
                        planned_end_date = taskEndDate,
                        duration = taskTemplate.duration,
                        per_complete = 0,
                        dept_id = taskTemplate.dept_id,
                        priority = "Medium",
                        status = "Not Started",
                        created_at = DateTime.Now
                    };

                    _context.Tasks.Add(task);
                    createdTasks.Add(task);
                    previousEndDate = taskEndDate;
                }

                await _context.SaveChangesAsync();

                for (int i = 1; i < createdTasks.Count; i++)
                {
                    createdTasks[i].parent_task_id = createdTasks[i - 1].task_id;
                }

                if (previousEndDate.HasValue)
                {
                    project.target_completion_date = previousEndDate.Value;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Project created successfully with folder structure and tasks", projectId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error creating project: {ex.Message}", null);
            }
        }

        public async Task<List<TaskResponseDto>> GetProjectTasksAsync(int projectId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Department)
                .Include(t => t.TaskRevisions)
                .Include(t => t.Milestone)
                .Where(t => t.proj_id == projectId)
                .OrderBy(t => t.planned_start_date)
                .ToListAsync();

            return tasks.Select((t, index) => new TaskResponseDto
            {
                task_id = t.task_id,
                proj_id = t.proj_id,
                parent_task_id = t.parent_task_id,
                order = index + 1,
                title = t.title,
                dept_id = t.dept_id,
                dept_name = t.Department?.dept_name,
                is_milestone = t.Milestone != null,
                planned_start_date = t.planned_start_date,
                planned_end_date = t.planned_end_date,
                actual_start_date = t.actual_start_date,
                actual_end_date = t.actual_end_date,
                duration = t.duration,
                status = t.status,
                priority = t.priority,
                per_complete = t.per_complete,
                planned_revisions = t.TaskRevisions?
                    .OrderBy(r => r.revised_on)
                    .Select((r, idx) => new TaskRevisionDto
                    {
                        revision_no = idx + 1,
                        old_start_date = r.old_start_date,
                        old_end_date = r.old_end_date,
                        new_start_date = r.new_start_date,
                        new_end_date = r.new_end_date,
                        note = r.note,
                        revised_on = r.revised_on
                    }).ToList() ?? new List<TaskRevisionDto>()
            }).ToList();
        }

        public async Task<List<MilestoneResponseDto>> GetProjectMilestonesAsync(int projectId)
        {
            var tasks = await _context.Milestones
                .Include(t => t.Tasks)
                .Include(t => t.ResponsibleDepartment)
                .Where(t => t.proj_id == projectId)
                .OrderBy(t => t.task_id)
                .ToListAsync();

            return tasks.Select((t, index) => new MilestoneResponseDto
            {
                proj_id = t.proj_id,
                task_id = t.task_id,
                milestone_name = t.milestone_name,
                planned_date = t.planned_date,
                actual_date = t.actual_date,
                status = t.status,
                responsible_dept_id = t.responsible_dept_id,
                dept_name = t.ResponsibleDepartment?.dept_name,
                created_at = t.created_at,
                description = t.description
            }).ToList();
        }

        public async Task<(bool success, string message)> LaunchProjectAsync(int projectId, LaunchProjectDto dto, int userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return (false, "Project not found");

                var isFirstLaunch = project.status == "Planning";

                if (dto.priority != null && !string.IsNullOrEmpty(dto.priority))
                {
                    project.priority = dto.priority;
                }

                var existingTeam = await _context.ProjectTeams
                    .Where(pt => pt.proj_id == projectId)
                    .ToListAsync();

                _context.ProjectTeams.RemoveRange(existingTeam);

                foreach (var member in dto.team_members)
                {
                    _context.ProjectTeams.Add(new ProjectTeams
                    {
                        proj_id = projectId,
                        user_id = member.user_id,
                        role = member.role ?? "Team Member",
                        created_at = DateTime.Now
                    });
                }

                var existingTasks = await _context.Tasks
                    .Where(t => t.proj_id == projectId)
                    .ToListAsync();

                var existingTaskDict = existingTasks.ToDictionary(t => t.task_id);

                // Track if any task dates changed
                bool hasTaskRevisions = false;
                var taskRevisionsToAdd = new List<(int taskId, string title, DateOnly? oldStart, DateOnly? oldEnd, DateOnly? newStart, DateOnly? newEnd, string note, float? duration, int? deptId)>();
                var newTasks = new List<Tasks>();

                // Process each task from DTO
                foreach (var taskDto in dto.tasks)
                {
                    if (taskDto.task_id.HasValue && existingTaskDict.ContainsKey(taskDto.task_id.Value))
                    {
                        // UPDATE EXISTING TASK
                        var existingTask = existingTaskDict[taskDto.task_id.Value];

                        // Check if dates have changed
                        bool datesChanged =
                            existingTask.planned_start_date != taskDto.start_date ||
                            existingTask.planned_end_date != taskDto.end_date;

                        // Track revision if dates changed (and not first launch)
                        if (datesChanged && !isFirstLaunch)
                        {
                            hasTaskRevisions = true;

                            // Get department ID
                            int? deptId = null;
                            if (!string.IsNullOrEmpty(taskDto.dept_name))
                            {
                                var dept = await _context.Departments
                                    .FirstOrDefaultAsync(d => d.dept_name == taskDto.dept_name);
                                deptId = dept?.dept_id;
                            }

                            taskRevisionsToAdd.Add((
                                taskId: existingTask.task_id,
                                title: existingTask.title,
                                oldStart: existingTask.planned_start_date,
                                oldEnd: existingTask.planned_end_date,
                                newStart: taskDto.start_date,
                                newEnd: taskDto.end_date,
                                note: taskDto.revision_note ?? "Updated via project setup",
                                duration: taskDto.duration,
                                deptId: deptId
                            ));
                        }

                        // Update task
                        existingTask.title = taskDto.title;
                        existingTask.planned_start_date = taskDto.start_date;
                        existingTask.planned_end_date = taskDto.end_date;
                        existingTask.duration = taskDto.duration;
                        existingTask.updated_at = DateTime.Now;

                        // Update department if dept_name provided
                        if (!string.IsNullOrEmpty(taskDto.dept_name))
                        {
                            var dept = await _context.Departments
                                .FirstOrDefaultAsync(d => d.dept_name == taskDto.dept_name);
                            if (dept != null)
                            {
                                existingTask.dept_id = dept.dept_id;
                            }
                        }
                    }
                    else
                    {
                        // CREATE NEW TASK
                        var dept = await _context.Departments
                            .FirstOrDefaultAsync(d => d.dept_name == taskDto.dept_name);

                        int? deptId = dept?.dept_id;

                        var newTask = new Tasks
                        {
                            proj_id = projectId,
                            title = taskDto.title,
                            planned_start_date = taskDto.start_date,
                            planned_end_date = taskDto.end_date,
                            duration = taskDto.duration,
                            dept_id = deptId,
                            status = "Not Started",
                            priority = "Medium",
                            per_complete = 0,
                            assigned_by = userId,
                            created_at = DateTime.Now
                        };

                        _context.Tasks.Add(newTask);
                        newTasks.Add(newTask);
                    }
                }
                await _context.SaveChangesAsync();

                /* =========================
                 * PROJECT REVISION TRACKING
                 * ========================= */
                if (hasTaskRevisions)
                {
                    // Get the latest revision number for this project
                    var latestRevisionNumber = await _context.ProjectRevisions
                        .Where(pr => pr.proj_id == projectId)
                        .OrderByDescending(pr => pr.revision_number)
                        .Select(pr => pr.revision_number)
                        .FirstOrDefaultAsync();

                    var newRevisionNumber = latestRevisionNumber + 1;

                    // Mark all previous revisions as inactive
                    var previousRevisions = await _context.ProjectRevisions
                        .Where(pr => pr.proj_id == projectId && pr.is_active)
                        .ToListAsync();

                    foreach (var rev in previousRevisions)
                    {
                        rev.is_active = false;
                    }

                    // Calculate new target completion date from tasks
                    DateOnly? newTargetDate = null;
                    if (dto.tasks != null && dto.tasks.Any())
                    {
                        var maxEndDate = dto.tasks
                            .Where(t => t.end_date.HasValue)
                            .Max(t => (DateOnly?)t.end_date);

                        if (maxEndDate.HasValue)
                        {
                            newTargetDate = maxEndDate.Value;
                        }
                    }

                    // Create new ProjectRevision
                    var projectRevision = new ProjectRevisions
                    {
                        proj_id = projectId,
                        revision_number = newRevisionNumber,
                        revision_date = DateTime.Now,
                        revised_by = userId,
                        revision_notes = $"Project updated with {taskRevisionsToAdd.Count} task date changes",
                        previous_target_date = project.target_completion_date,
                        new_target_date = newTargetDate,
                        is_active = true
                    };

                    _context.ProjectRevisions.Add(projectRevision);

                    // Save to get the revision_id
                    await _context.SaveChangesAsync();

                    // Now add all TaskRevisions linked to this ProjectRevision
                    foreach (var taskRev in taskRevisionsToAdd)
                    {
                        _context.TaskRevisions.Add(new TaskRevisions
                        {
                            revision_id = projectRevision.revision_id,
                            task_id = taskRev.taskId,
                            title = taskRev.title,
                            old_start_date = taskRev.oldStart,
                            old_end_date = taskRev.oldEnd,
                            new_start_date = taskRev.newStart,
                            new_end_date = taskRev.newEnd,
                            note = taskRev.note,
                            revised_on = DateTime.Now,
                            duration = taskRev.duration,
                            dept_id = taskRev.deptId,
                            status = "Revised"
                        });
                    }
                }

                /* =========================
                 * MILESTONES
                 * ========================= */
                if (dto.milestones != null && dto.milestones.Any())
                {
                    var existingMilestones = await _context.Milestones
                        .Where(m => m.proj_id == projectId)
                        .ToListAsync();

                    _context.Milestones.RemoveRange(existingMilestones);

                    var allTasks = existingTasks.Concat(newTasks).ToList();

                    foreach (var m in dto.milestones)
                    {
                        int taskId = m.task_id;

                        _context.Milestones.Add(new Milestones
                        {
                            proj_id = projectId,
                            task_id = taskId,
                            milestone_name = m.milestone_name,
                            planned_date = m.planned_date,
                            status = m.status ?? "Pending",
                            responsible_dept_id = m.responsible_dept_id,
                            created_at = DateTime.Now
                        });
                    }
                }

                /* =========================
                 * PROJECT UPDATE
                 * ========================= */
                if (isFirstLaunch)
                {
                    project.status = "In Progress";
                }

                project.updated_at = DateTime.Now;
                project.updated_by = userId;

                // Update target completion date
                if (dto.tasks != null && dto.tasks.Any())
                {
                    var maxEndDate = dto.tasks
                        .Where(t => t.end_date.HasValue)
                        .Max(t => (DateOnly?)t.end_date);

                    if (maxEndDate.HasValue)
                    {
                        project.target_completion_date = maxEndDate.Value;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return (true, isFirstLaunch ? "Project launched successfully" : "Project updated successfully");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Operation failed: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateProjectStatusAsync(int projectId, string status)
        {
            try
            {
                var project = await _context.Projects.FindAsync(projectId);

                if (project == null)
                    return (false, "Project not found");

                var validStatuses = new[] { "Planning", "Not Started", "In Progress", "On Hold", "Completed", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    return (false, "Invalid status value");
                }

                project.status = status;
                project.updated_at = DateTime.Now;

                if (status == "Completed" && project.actual_completion_date == null)
                {
                    project.actual_completion_date = DateOnly.FromDateTime(DateTime.Now);
                }

                await _context.SaveChangesAsync();

                return (true, "Project status updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating project status: {ex.Message}");
            }
        }

        public async Task<List<ProjectResponseDto>> GetProjectsByStatusAsync(string status)
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Departments)
                .Where(p => p.status == status)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();

            return projects.Select(p => new ProjectResponseDto
            {
                proj_id = p.proj_id,
                proj_no = p.proj_no,
                proj_name = p.proj_name,
                cust_id = p.cust_id,
                customer_name = p.Customer?.comp_name,
                dept_id = p.dept_id,
                dept_name = p.Departments != null ? string.Join(", ", p.Departments.Select(d => d.dept_name)) : null,
                project_start_date = p.project_start_date,
                target_completion_date = p.target_completion_date,
                actual_completion_date = p.actual_completion_date,
                priority = p.priority,
                status = p.status,
                description = p.description,
                storage_path = p.storage_path,
                created_at = p.created_at,
                updated_at = p.updated_at
            }).ToList();
        }

        public async Task<List<ProjectResponseDto>> GetProjectsByDepartmentAsync(int deptId)
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Departments)
                .Where(p => p.dept_id == deptId)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();

            return projects.Select(p => new ProjectResponseDto
            {
                proj_id = p.proj_id,
                proj_no = p.proj_no,
                proj_name = p.proj_name,
                cust_id = p.cust_id,
                customer_name = p.Customer?.comp_name,
                dept_id = p.dept_id,
                dept_name = p.Departments != null ? string.Join(", ", p.Departments.Select(d => d.dept_name)) : null,
                project_start_date = p.project_start_date,
                target_completion_date = p.target_completion_date,
                actual_completion_date = p.actual_completion_date,
                priority = p.priority,
                status = p.status,
                description = p.description,
                storage_path = p.storage_path,
                created_at = p.created_at,
                updated_at = p.updated_at
            }).ToList();
        }

        public async Task<List<ProjectResponseDto>> GetProjectsByCustomerAsync(int customerId)
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Departments)
                .Where(p => p.cust_id == customerId)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();

            return projects.Select(p => new ProjectResponseDto
            {
                proj_id = p.proj_id,
                proj_no = p.proj_no,
                proj_name = p.proj_name,
                cust_id = p.cust_id,
                customer_name = p.Customer?.comp_name,
                dept_id = p.dept_id,
                dept_name = p.Departments != null ? string.Join(", ", p.Departments.Select(d => d.dept_name)) : null,
                project_start_date = p.project_start_date,
                target_completion_date = p.target_completion_date,
                actual_completion_date = p.actual_completion_date,
                priority = p.priority,
                status = p.status,
                description = p.description,
                storage_path = p.storage_path,
                created_at = p.created_at,
                updated_at = p.updated_at
            }).ToList();
        }
    }
}
