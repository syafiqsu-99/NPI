using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
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

        public async Task<(bool success, string message, int? proj_id)> CreateProjectFromEnquiryAsync(
            int enquiryId, int userId)
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
                        start_date = taskStartDate,
                        end_date = taskEndDate,
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

        public async Task<List<ProjectResponseDto>> GetAllProjectsAsync()
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();

            return projects.Select(p => new ProjectResponseDto
            {
                proj_id = p.proj_id,
                proj_no = p.proj_no,
                proj_name = p.proj_name,
                cust_id = p.cust_id,
                customer_name = p.Customer?.comp_name,
                project_start_date = p.project_start_date,
                target_completion_date = p.target_completion_date,
                priority = p.priority,
                status = p.status,
                description = p.description,
                storage_path = p.storage_path
            }).ToList();
        }

        public async Task<ProjectResponseDto?> GetProjectByIdAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Customer)
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
                project_start_date = project.project_start_date,
                target_completion_date = project.target_completion_date,
                priority = project.priority,
                status = project.status,
                description = project.description,
                storage_path = project.storage_path,

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

        public async Task<List<TaskResponseDto>> GetProjectTasksAsync(int projectId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Department)
                .Where(t => t.proj_id == projectId)
                .OrderBy(t => t.start_date)
                .ToListAsync();

            return tasks.Select((t, index) => new TaskResponseDto
            {
                task_id = t.task_id,
                parent_task_id = t.parent_task_id,
                order = index + 1,
                title = t.title,
                dept_name = t.Department?.dept_name,
                start_date = t.start_date,
                end_date = t.end_date,
                duration = t.duration,
                status = t.status
            }).ToList();
        }

        public async Task<(bool success, string message)> LaunchProjectAsync(int projectId, LaunchProjectDto dto, int userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.proj_id == projectId);

                if (project == null)
                    return (false, "Project not found");

                /* =========================
                 * PROJECT TEAM (UPSERT)
                 * ========================= */
                var existingTeam = await _context.ProjectTeam
                    .Where(t => t.proj_id == projectId)
                    .ToListAsync();

                _context.ProjectTeam.RemoveRange(existingTeam);

                foreach (var m in dto.team_members)
                {
                    _context.ProjectTeam.Add(new ProjectTeam
                    {
                        proj_id = projectId,
                        user_id = m.user_id,
                        role = m.role,
                        assigned_at = DateTime.Now
                    });
                }

                /* =========================
                 * TASKS (FULL SYNC)
                 * ========================= */
                var existingTasks = await _context.Tasks
                    .Where(t => t.proj_id == projectId)
                    .ToListAsync();

                var incomingIds = dto.tasks
                    .Where(t => t.task_id.HasValue)
                    .Select(t => t.task_id!.Value)
                    .ToHashSet();

                // Delete removed tasks
                var toDelete = existingTasks
                    .Where(t => !incomingIds.Contains(t.task_id))
                    .ToList();

                _context.Tasks.RemoveRange(toDelete);

                foreach (var t in dto.tasks)
                {
                    var deptId = await _context.Departments
                        .Where(d => d.dept_name == t.dept_name)
                        .Select(d => d.dept_id)
                        .FirstOrDefaultAsync();

                    if (t.task_id.HasValue)
                    {
                        var task = existingTasks.FirstOrDefault(x => x.task_id == t.task_id.Value);
                        if (task != null)
                        {
                            task.title = t.title;
                            task.start_date = t.start_date;
                            task.end_date = t.end_date;
                            task.duration = t.duration;
                            task.dept_id = deptId;
                            task.updated_at = DateTime.Now;
                        }
                    }
                    else
                    {
                        _context.Tasks.Add(new Tasks
                        {
                            proj_id = projectId,
                            title = t.title,
                            start_date = t.start_date,
                            end_date = t.end_date,
                            duration = t.duration,
                            dept_id = deptId,
                            status = "Not Started",
                            priority = "Medium",
                            created_at = DateTime.Now
                        });
                    }
                }

                /* =========================
                 * MILESTONES
                 * ========================= */
                if (dto.milestones != null)
                {
                    var existingMilestones = await _context.Milestones
                        .Where(m => m.proj_id == projectId)
                        .ToListAsync();

                    _context.Milestones.RemoveRange(existingMilestones);

                    foreach (var m in dto.milestones)
                    {
                        _context.Milestones.Add(new Milestones
                        {
                            proj_id = projectId,
                            milestone_name = m.milestone_name,
                            planned_date = m.planned_date,
                            created_at = DateTime.Now
                        });
                    }
                }

                /* =========================
                 * PROJECT UPDATE
                 * ========================= */
                project.status = "In Progress";
                project.updated_at = DateTime.Now;
                project.updated_by = userId;

                if (dto.tasks != null && dto.tasks.Any())
                {
                    var maxEndDate = dto.tasks
                        .Where(t => t.end_date.HasValue)
                        .Max(t => t.end_date);

                    if (maxEndDate.HasValue)
                    {
                        project.target_completion_date = maxEndDate.Value;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return (true, "Project launched successfully");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Launch failed: {ex.Message}");
            }
        }

    }
}
