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
        private readonly NotificationTriggerService _triggerService;
        private readonly string _basePath;

        public ProjectService(ApplicationDbContext context, IConfiguration configuration, NotificationTriggerService triggerService)
        {
            _context = context;
            _configuration = configuration;
            _basePath = configuration["FileStorage:BasePath"] ?? @"D:\NPI_Projects";
            _triggerService = triggerService;
        }

        private static string SanitizeFolderName(string name)
        {
            var result = name.Replace(" ", "_").Replace("/", "_");
            foreach (var c in Path.GetInvalidFileNameChars())
                result = result.Replace(c, '_');
            return result;
        }

        private string GetProjectPath(string projName)
            => Path.Combine(_basePath, "projects", SanitizeFolderName(projName));

        private static ProjectResponseDto MapToResponseDto(Projects p) => new()
        {
            proj_id = p.proj_id,
            proj_no = p.proj_no,
            proj_name = p.proj_name,
            cust_id = p.cust_id,
            customer_name = p.Customer?.comp_name,
            dept_id = p.dept_id,
            dept_name = p.Departments != null
                                           ? string.Join(", ", p.Departments.Select(d => d.dept_name))
                                           : null,
            project_start_date = p.project_start_date,
            target_completion_date = p.target_completion_date,
            actual_completion_date = p.actual_completion_date,
            priority = p.priority,
            status = p.status,
            description = p.description,
            storage_path = p.storage_path,
            created_at = p.created_at,
            updated_at = p.updated_at,
            pilot_mould_required = p.pilot_mould_required,
            machine_purchase_required = p.machine_purchase_required,
        };

        public async Task<List<ProjectResponseDto>> GetAllProjectsAsync()
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Departments)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();

            if (!projects.Any())
            {
                return new List<ProjectResponseDto>();
            }

            var projectIds = projects.Select(p => p.proj_id).ToList();

            var allTasks = await _context.Tasks
                .Where(t => projectIds.Contains(t.proj_id) && t.stage_id != null)
                .Select(t => new { t.proj_id, t.stage_id, t.status })
                .ToListAsync();

            var tasksByProject = allTasks
                .GroupBy(t => t.proj_id)
                .ToDictionary(g => g.Key, g => g.ToList());

            return projects.Select(p =>
            {
                var dto = MapToResponseDto(p);
                var projTasks = tasksByProject.GetValueOrDefault(p.proj_id, new());

                dto.stage_progress = projTasks
                    .GroupBy(t => t.stage_id!)
                    .ToDictionary(
                        g => g.Key,
                        g => new StageProgressDto
                        {
                            task_count = g.Count(),
                            completed_count = g.Count(t => t.status == "Completed"),
                            completed = g.All(t => t.status == "Completed"),
                            in_progress = g.Any(t => t.status == "In Progress" || t.status == "Completed")
                                                && !g.All(t => t.status == "Completed")
                        });

                return dto;
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
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.TaskRevisions)
                        .ThenInclude(tr => tr.Revision)
                .Include(p => p.ProjectRevisions)
                    .ThenInclude(pr => pr.RevisedByUser)
                .Include(p => p.ProjectRevisions)
                    .ThenInclude(pr => pr.TaskRevisions)
                .FirstOrDefaultAsync(p => p.proj_id == projectId);

            if (project == null) return null;

            var dto = MapToResponseDto(project);

            dto.team_members = project.ProjectTeams.Select(tm => new TeamMemberDto
            {
                user_id = tm.user_id,
                user_name = tm.User != null ? tm.User.username : null,
                full_name = tm.User != null ? tm.User.full_name : null,
                dept_id = tm.User != null && tm.User.dept_id.HasValue ? tm.User.dept_id.Value : 0,
                dept_name = tm.User != null && tm.User.Department != null ? tm.User.Department.dept_name : null,
                role = tm.role ?? "Team Member",
                assigned_at = tm.created_at,
                email = tm.User != null ? tm.User.email : null,
            }).ToList();

            if (project.ProjectRevisions != null && project.ProjectRevisions.Any())
            {
                dto.revisions = project.ProjectRevisions
                    .OrderByDescending(pr => pr.revision_date)
                    .Select(pr => new ProjectRevisionDto
                    {
                        revision_id = pr.revision_id,
                        revision_number = pr.revision_number,
                        revision_date = pr.revision_date,
                        revised_by = pr.revised_by,
                        revised_by_name = pr.RevisedByUser?.username,
                        revision_notes = pr.revision_notes,
                        previous_target_date = pr.previous_target_date,
                        new_target_date = pr.new_target_date,
                        is_active = pr.is_active,
                        task_revisions = pr.TaskRevisions?.Select(tr => new TaskRevisionDto
                        {
                            task_id = tr.task_id,
                            task_title = tr.title,
                            old_start_date = tr.old_start_date,
                            old_end_date = tr.old_end_date,
                            new_start_date = tr.new_start_date,
                            new_end_date = tr.new_end_date,
                            revision_note = tr.note,
                            revised_on = tr.revised_on
                        }).ToList() ?? new List<TaskRevisionDto>()
                    }).ToList();
            }

            var tasks = await _context.Tasks
                .Where(t => t.proj_id == projectId && t.stage_id != null)
                .ToListAsync();

            dto.stage_progress = tasks
                .GroupBy(t => t.stage_id!)
                .ToDictionary(
                    g => g.Key,
                    g => new StageProgressDto
                    {
                        task_count = g.Count(),
                        completed_count = g.Count(t => t.status == "Completed"),
                        completed = g.All(t => t.status == "Completed"),
                        in_progress = g.Any(t => t.status == "In Progress" || t.status == "Completed")
                                          && !g.All(t => t.status == "Completed")
                    });

            return dto;
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
                    if (int.TryParse(lastNumStr, out int lastNum))
                        projectNo = $"PRJ{year}{(lastNum + 1):D3}";
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

                var projectPath = GetProjectPath(project.proj_name);
                Directory.CreateDirectory(projectPath);
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
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return (false, "User not found.");

                string userRole = user.Role?.role_name ?? "Member";

                var (authorized, authMessage) = await ValidateProjectWriteAccessAsync(projectId, userId, userRole);
                if (!authorized) return (false, authMessage);

                var project = await _context.Projects.FindAsync(projectId);
                if (project == null) return (false, "Project not found");

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
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var project = await _context.Projects
                    .Include(p => p.Tasks)
                        .ThenInclude(t => t.Files)
                    .Include(p => p.Tasks)
                        .ThenInclude(t => t.SubTasks)
                            .ThenInclude(st => st.Files)
                    .Include(p => p.Tasks)
                        .ThenInclude(t => t.SubTasks)
                            .ThenInclude(st => st.Milestone)
                    .Include(p => p.ProjectTeams)
                    .FirstOrDefaultAsync(p => p.proj_id == projectId);

                if (project == null)
                    return (false, "Project not found");

                var relatedEnquiries = await _context.Enquiries
                    .Where(e => e.proj_id == projectId)
                    .ToListAsync();

                foreach (var enquiry in relatedEnquiries)
                {
                    enquiry.proj_id = null;
                    enquiry.updated_at = DateTime.Now;
                    enquiry.status = "Submitted";
                }

                await _context.SaveChangesAsync();

                var allTasks = project.Tasks
                    .Concat(project.Tasks.SelectMany(t => t.SubTasks ?? new List<Tasks>()))
                    .ToList();

                var allFiles = allTasks
                    .SelectMany(t => t.Files ?? new List<Files>())
                    .ToList();

                if (allFiles.Any())
                    _context.Files.RemoveRange(allFiles);

                await _context.SaveChangesAsync();

                var subTasks = project.Tasks
                    .SelectMany(t => t.SubTasks ?? new List<Tasks>())
                    .ToList();

                if (subTasks.Any())
                    _context.Tasks.RemoveRange(subTasks);

                await _context.SaveChangesAsync();

                _context.Tasks.RemoveRange(project.Tasks);
                _context.ProjectTeams.RemoveRange(project.ProjectTeams);

                await _context.SaveChangesAsync();

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, $"Project {project.proj_no} deleted successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error deleting project: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateProjectStatusAsync(int projectId, string status, int userId, string userRole)
        {
            try
            {
                // 1. Verify Authorization
                var (authorized, authMessage) = await ValidateProjectWriteAccessAsync(projectId, userId, userRole);
                if (!authorized) return (false, authMessage);

                // 2. Fetch Project
                var project = await _context.Projects
                    .Include(p => p.Tasks)
                    .Include(p => p.ProjectTeams)
                    .FirstOrDefaultAsync(p => p.proj_id == projectId);

                if (project == null)
                    return (false, "Project not found");

                var validStatuses = new[]
                    { "Planning", "Not Started", "In Progress", "On Hold", "Completed", "Cancelled" };

                if (!validStatuses.Contains(status))
                    return (false, "Invalid status value");

                // 3. Validation for Completion
                if (status == "Completed")
                {
                    var incompleteTasks = project.Tasks.Count(t => t.status != "Completed" && t.status != "Cancelled");
                    if (incompleteTasks > 0)
                    {
                        return (false, $"Cannot complete project: {incompleteTasks} task(s) still pending.");
                    }
                }

                // 4. Create Revision History
                var lastRevNumber = await _context.ProjectRevisions
                    .Where(pr => pr.proj_id == projectId)
                    .Select(pr => (int?)pr.revision_number)
                    .MaxAsync() ?? 0;

                var projectRevision = new ProjectRevisions
                {
                    proj_id = projectId,
                    revision_number = lastRevNumber + 1,
                    revision_date = DateTime.Now,
                    revised_by = userId,
                    revision_notes = $"Project status changed from '{project.status}' to '{status}'",
                    previous_target_date = project.target_completion_date,
                    new_target_date = project.target_completion_date,
                    is_active = true
                };

                _context.ProjectRevisions.Add(projectRevision);

                // 5. Update Project Status
                project.status = status;
                project.updated_at = DateTime.Now;
                project.updated_by = userId;

                if (status == "Completed" && project.actual_completion_date == null)
                    project.actual_completion_date = DateOnly.FromDateTime(DateTime.Now);

                // 6. SYNC ENQUIRY STATUS
                var linkedEnquiry = await _context.Enquiries.FirstOrDefaultAsync(e => e.proj_id == projectId);
                if (linkedEnquiry != null)
                {
                    // Map Project status to the corresponding Enquiry status
                    linkedEnquiry.status = status switch
                    {
                        "Planning" => "Started",
                        "Not Started" => "Started",
                        "In Progress" => "In Progress",
                        "On Hold" => "In Review",
                        "Completed" => "Completed",
                        "Cancelled" => "Rejected",
                        _ => status
                    };

                    linkedEnquiry.updated_at = DateTime.Now;
                    linkedEnquiry.updated_by = userId;
                }

                // 7. Save everything to the database
                await _context.SaveChangesAsync();

                // 8. Trigger Notifications
                await _triggerService.OnProjectStatusChangedAsync(projectId, status);

                return (true, $"Project status updated to '{status}' successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating project status: {ex.Message}");
            }
        }

        public async Task<(bool success, string message, int proj_id)> CreateProjectFromEnquiryAsync(int enquiryId, int userId, CreateProjectFromEnquiryDto? dto = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var enquiry = await _context.Enquiries
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId);

                if (enquiry == null)
                    return (false, "Enquiry not found", 0);

                if (enquiry.status != "Submitted" && enquiry.status != "Approved")
                    return (false, "Only submitted or approved enquiries can be converted to projects", 0);

                if (enquiry.proj_id != null)
                    return (false, "A project already exists for this enquiry", 0);

                string projName;
                if (!string.IsNullOrWhiteSpace(dto?.project_name))
                {
                    projName = dto.project_name.Trim();
                }
                else
                {
                    var company = enquiry.Customer?.comp_name
                                  ?? "New Project";
                    var category = enquiry.npi_category ?? "";
                    projName = string.IsNullOrWhiteSpace(category)
                        ? company
                        : $"{company} - {category}";
                }

                var priority = !string.IsNullOrWhiteSpace(dto?.priority)
                    ? dto!.priority
                    : "Medium";

                var description = !string.IsNullOrWhiteSpace(dto?.description)
                    ? dto!.description
                    : $"NPI Project for {enquiry.npi_category}";

                DateOnly? targetDate = null;
                if (dto?.expected_completion != null)
                {
                    targetDate = dto.expected_completion;
                }

                // ── 3. Generate project number ────────────────────────────────────
                var year = DateTime.Now.Year;
                var lastProject = await _context.Projects
                    .Where(p => p.proj_no.StartsWith($"PRJ{year}"))
                    .OrderByDescending(p => p.proj_id)
                    .FirstOrDefaultAsync();

                string projectNo = $"PRJ{year}001";
                if (lastProject != null)
                {
                    var lastNumStr = lastProject.proj_no.Substring(7);
                    if (int.TryParse(lastNumStr, out int lastNum))
                        projectNo = $"PRJ{year}{(lastNum + 1):D3}";
                }

                var project = new Projects
                {
                    proj_no = projectNo,
                    proj_name = projName,
                    cust_id = enquiry.cust_id,
                    enquiry_date = DateOnly.FromDateTime(enquiry.created_at),
                    project_start_date = DateOnly.FromDateTime(DateTime.Now),
                    target_completion_date = targetDate,
                    priority = priority,
                    status = "Planning",
                    description = description,
                    pilot_mould_required = false,
                    machine_purchase_required = false,
                    created_by = userId,
                    created_at = DateTime.Now
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                var projectId = project.proj_id;

                enquiry.proj_id = projectId;
                enquiry.status = "Started";
                enquiry.updated_at = DateTime.Now;
                enquiry.updated_by = userId;
                await _context.SaveChangesAsync();

                var departments = await _context.Departments.ToListAsync();

                Departments? LookupDept(string name) =>
                    departments.FirstOrDefault(d =>
                        d.dept_name.Contains(name, StringComparison.OrdinalIgnoreCase));

                var salesDept = LookupDept("Sales");
                var technicalDept = LookupDept("Technical");
                var purchaserDept = LookupDept("Purchaser") ?? LookupDept("Purchasing");
                var qaDept = LookupDept("QA");
                var productionDept = LookupDept("Production");

                Departments? ResolveDept(string pic) => pic switch
                {
                    "Sales" => salesDept,
                    "Technical" => technicalDept,
                    "Purchaser" => purchaserDept,
                    "QA" => qaDept,
                    "Production" => productionDept,
                    _ => null
                };

                var stage0Tasks = new[]
                {
                    new { code = "0.1", title = "Sales Enquiry Form",  pic = "Sales"     },
                    new { code = "0.2", title = "Customer Info",       pic = "Sales"     },
                    new { code = "0.3", title = "Project Awarded",     pic = "Technical" },
                };

                var stage1Tasks = new[]
                {
                    new { code = "1.1", title = "Project awarded / Contract signing",  pic = "Sales"     },
                    new { code = "1.2", title = "Drawing preparation",                 pic = "Technical" },
                    new { code = "1.3", title = "Drawing submission to customer",      pic = "Sales"     },
                    new { code = "1.4", title = "DFM preparation",                     pic = "Technical" },
                    new { code = "1.5", title = "DFM submission to customer",           pic = "Technical" },
                    new { code = "1.6", title = "Customer drawing approval",            pic = "Sales"     },
                    new { code = "1.7", title = "PO issuance from customer",            pic = "Sales"     },
                    new { code = "1.8", title = "PO issuance to supplier",              pic = "Purchaser" },
                };

                var projectPath = GetProjectPath(project.proj_name);
                Directory.CreateDirectory(projectPath);
                project.storage_path = projectPath;

                var today = DateOnly.FromDateTime(DateTime.Now);

                var seededDepts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var t in stage0Tasks)
                {
                    var dept = ResolveDept(t.pic);
                    EnsureDeptFolder(projectPath, dept?.dept_name, seededDepts);

                    _context.Tasks.Add(new Tasks
                    {
                        proj_id = projectId,
                        stage_id = "0.0",
                        task_code = t.code,
                        title = t.title,
                        dept_id = dept?.dept_id,
                        planned_start_date = today,
                        planned_end_date = today,
                        actual_start_date = today,
                        actual_end_date = today,
                        duration = 1,
                        status = "Completed",
                        per_complete = 100,
                        priority = "Medium",
                        completed_at = DateTime.Now,
                        created_at = DateTime.Now
                    });
                }

                foreach (var t in stage1Tasks)
                {
                    var dept = ResolveDept(t.pic);
                    EnsureDeptFolder(projectPath, dept?.dept_name, seededDepts);

                    _context.Tasks.Add(new Tasks
                    {
                        proj_id = projectId,
                        stage_id = "1.0",
                        task_code = t.code,
                        title = t.title,
                        dept_id = dept?.dept_id,
                        planned_start_date = today,
                        planned_end_date = today.AddDays(5),
                        duration = 5,
                        status = "Not Started",
                        per_complete = 0,
                        priority = "Medium",
                        created_at = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                if (targetDate == null)
                    project.target_completion_date = today.AddDays(40);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Project created with stage-aware tasks", projectId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error creating project: {ex.Message}", 0);
            }
        }

        private static void EnsureDeptFolder(string projectPath, string? deptName, HashSet<string> created)
        {
            var folderName = SanitizeFolderName(deptName ?? "Others");
            if (created.Add(folderName))
                Directory.CreateDirectory(Path.Combine(projectPath, folderName));
        }

        public async Task<(bool success, string message, List<string>? folderWarnings)> LaunchProjectAsync(int projectId, LaunchProjectDto dto, int userId)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return (false, "Project not found", null);

                var isFirstLaunch = project.status == "Planning";

                if (!string.IsNullOrEmpty(dto.priority))
                    project.priority = dto.priority;

                if (!string.IsNullOrEmpty(dto.description))
                    project.description = dto.description;

                project.pilot_mould_required = dto.pilot_mould_required;
                project.machine_purchase_required = dto.machine_purchase_required;

                // Replace team
                var existingTeam = await _context.ProjectTeams
                    .Where(pt => pt.proj_id == projectId)
                    .ToListAsync();
                _context.ProjectTeams.RemoveRange(existingTeam);

                foreach (var member in dto.team_members)
                {
                    string assignedRole = member.role ?? "Member";

                    // Write ONLY to ProjectTeams — never mutate Users.role_id
                    _context.ProjectTeams.Add(new ProjectTeams
                    {
                        proj_id = projectId,
                        user_id = member.user_id,
                        role = assignedRole,
                        created_at = DateTime.Now
                    });
                }

                var existingTasks = await _context.Tasks
                    .Where(t => t.proj_id == projectId)
                    .ToListAsync();

                var existingTaskDict = existingTasks.ToDictionary(t => t.task_id);
                var incomingTaskIds = new HashSet<int>(
                    dto.tasks.Where(t => t.task_id.HasValue).Select(t => t.task_id!.Value));

                // Track date changes for revision history
                var taskRevisionsToAdd = new List<(int taskId, string title, DateOnly? oldStart,
                    DateOnly? oldEnd, DateOnly? newStart, DateOnly? newEnd, string? note,
                    float? duration, int? deptId)>();

                foreach (var taskDto in dto.tasks)
                {
                    if (taskDto.task_id.HasValue &&
                        existingTaskDict.TryGetValue(taskDto.task_id.Value, out var existing))
                    {
                        // Track changes before updating
                        bool datesChanged = existing.planned_start_date != taskDto.start_date
                                         || existing.planned_end_date != taskDto.end_date;

                        if (datesChanged && !string.IsNullOrWhiteSpace(taskDto.revision_note))
                        {
                            var dept = !string.IsNullOrEmpty(taskDto.dept_name)
                                ? await _context.Departments
                                    .FirstOrDefaultAsync(d => d.dept_name == taskDto.dept_name)
                                : null;

                            taskRevisionsToAdd.Add((
                                existing.task_id, existing.title,
                                existing.planned_start_date, existing.planned_end_date,
                                taskDto.start_date, taskDto.end_date,
                                taskDto.revision_note, taskDto.duration, dept?.dept_id));
                        }

                        // Update existing task
                        existing.title = taskDto.title;
                        existing.planned_start_date = taskDto.start_date;
                        existing.planned_end_date = taskDto.end_date;
                        existing.duration = taskDto.duration;
                        existing.stage_id = taskDto.stage_id;
                        existing.task_code = taskDto.task_code;
                        existing.updated_at = DateTime.Now;

                        if (!string.IsNullOrEmpty(taskDto.dept_name))
                        {
                            var dept = await _context.Departments
                                .FirstOrDefaultAsync(d => d.dept_name == taskDto.dept_name);
                            if (dept != null)
                                existing.dept_id = dept.dept_id;
                        }
                    }
                    else
                    {
                        // Add new task
                        var dept = await _context.Departments
                            .FirstOrDefaultAsync(d => d.dept_name == taskDto.dept_name);

                        var isStage0 = isFirstLaunch && taskDto.stage_id == "0.0";

                        _context.Tasks.Add(new Tasks
                        {
                            proj_id = projectId,
                            title = taskDto.title,
                            planned_start_date = taskDto.start_date,
                            planned_end_date = taskDto.end_date,
                            duration = taskDto.duration,
                            dept_id = dept?.dept_id,
                            stage_id = taskDto.stage_id,
                            task_code = taskDto.task_code,
                            status = isStage0 ? "Completed" : "Not Started",
                            per_complete = isStage0 ? 100 : 0,
                            actual_start_date = isStage0 ? DateOnly.FromDateTime(DateTime.Now) : null,
                            actual_end_date = isStage0 ? DateOnly.FromDateTime(DateTime.Now) : null,
                            completed_at = isStage0 ? DateTime.Now : null,
                            priority = "Medium",
                            assigned_by = userId,
                            created_at = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // Create project revision if any task dates changed
                if (taskRevisionsToAdd.Count > 0)
                {
                    var lastRevNumber = await _context.ProjectRevisions
                        .Where(pr => pr.proj_id == projectId)
                        .Select(pr => (int?)pr.revision_number)
                        .MaxAsync() ?? 0;

                    var previousRevisions = await _context.ProjectRevisions
                        .Where(pr => pr.proj_id == projectId && pr.is_active)
                        .ToListAsync();

                    foreach (var rev in previousRevisions)
                        rev.is_active = false;

                    var newTargetDate = dto.tasks
                        .Where(t => t.end_date.HasValue)
                        .Max(t => (DateOnly?)t.end_date);

                    var projectRevision = new ProjectRevisions
                    {
                        proj_id = projectId,
                        revision_number = lastRevNumber + 1,
                        revision_date = DateTime.Now,
                        revised_by = userId,
                        revision_notes = $"Launch update — {taskRevisionsToAdd.Count} task(s) rescheduled",
                        previous_target_date = project.target_completion_date,
                        new_target_date = newTargetDate,
                        is_active = true
                    };

                    _context.ProjectRevisions.Add(projectRevision);
                    await _context.SaveChangesAsync(); // flush to get revision_id

                    foreach (var (tId, title, oldStart, oldEnd, newStart, newEnd, note, dur, deptId)
                        in taskRevisionsToAdd)
                    {
                        _context.TaskRevisions.Add(new TaskRevisions
                        {
                            revision_id = projectRevision.revision_id,
                            task_id = tId,
                            title = title,
                            old_start_date = oldStart,
                            old_end_date = oldEnd,
                            new_start_date = newStart,
                            new_end_date = newEnd,
                            note = note ?? string.Empty,
                            revised_on = DateTime.Now,
                            duration = dur,
                            dept_id = deptId,
                            status = "Revised"
                        });
                    }
                }

                // Remove tasks no longer in the incoming list (non-stage-0 only)
                var tasksToDelete = existingTasks
                    .Where(t => !incomingTaskIds.Contains(t.task_id) && t.stage_id != "0.0")
                    .ToList();

                if (tasksToDelete.Count > 0)
                    _context.Tasks.RemoveRange(tasksToDelete);

                // Folder structure
                var projectPath = GetProjectPath(project.proj_name);
                Directory.CreateDirectory(projectPath);
                project.storage_path = projectPath;

                var customerFolder = Path.Combine(projectPath, "Customer");
                Directory.CreateDirectory(customerFolder);

                var allProjectTasks = await _context.Tasks
                    .Include(t => t.Department)
                    .Where(t => t.proj_id == projectId)
                    .ToListAsync();

                var expectedDeptFolders = allProjectTasks
                    .Select(t => SanitizeFolderName(t.Department?.dept_name ?? "Others"))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var deptFolder in expectedDeptFolders)
                    Directory.CreateDirectory(Path.Combine(projectPath, deptFolder));

                var folderWarnings = new List<string>();

                if (Directory.Exists(projectPath))
                {
                    foreach (var existingDir in Directory.GetDirectories(projectPath))
                    {
                        var dirName = Path.GetFileName(existingDir);
                        if (!expectedDeptFolders.Contains(dirName))
                        {
                            bool hasContent =
                                Directory.GetFiles(existingDir, "*", SearchOption.AllDirectories).Any() ||
                                Directory.GetDirectories(existingDir).Any();

                            if (hasContent)
                                folderWarnings.Add(dirName);
                            else
                                Directory.Delete(existingDir, recursive: true);
                        }
                    }
                }

                if (isFirstLaunch)
                    project.status = "In Progress";

                project.updated_at = DateTime.Now;
                project.updated_by = userId;

                var maxEnd = dto.tasks
                    .Where(t => t.end_date.HasValue)
                    .Max(t => (DateOnly?)t.end_date);

                if (maxEnd.HasValue)
                    project.target_completion_date = maxEnd.Value;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // N5: Notify team of project launch (only on first launch)
                if (isFirstLaunch)
                    await _triggerService.OnProjectLaunchedAsync(projectId);

                return folderWarnings.Count > 0
                    ? (true, "Project saved. Some folders could not be removed — they still contain files.", folderWarnings)
                    : (true, isFirstLaunch ? "Project launched successfully" : "Project updated successfully", null);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Operation failed: {ex.Message}", null);
            }
        }

        public async Task<List<TaskResponseDto>> GetProjectTasksAsync(int projectId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Department)
                .Include(t => t.TaskRevisions)
                .Where(t => t.proj_id == projectId)
                .OrderBy(t => t.planned_start_date)
                .ToListAsync();

            return tasks.Select((t, index) => new TaskResponseDto
            {
                task_id = t.task_id,
                proj_id = t.proj_id,
                parent_task_id = t.parent_task_id,
                order = index + 1,
                stage_id = t.stage_id,
                task_code = t.task_code,
                title = t.title,
                dept_id = t.dept_id,
                dept_name = t.Department?.dept_name,
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
                        revision_note = r.note,
                        revised_on = r.revised_on
                    }).ToList() ?? new List<TaskRevisionDto>()
            }).ToList();
        }

        public async Task<List<MilestoneResponseDto>> GetProjectMilestonesAsync(int projectId)
        {
            return await Task.FromResult(new List<MilestoneResponseDto>());
        }

        public async Task<List<ProjectResponseDto>> GetProjectsByStatusAsync(string status)
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Departments)
                .Where(p => p.status == status)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();

            return projects.Select(MapToResponseDto).ToList();
        }

        public async Task<List<ProjectResponseDto>> GetProjectsByDepartmentAsync(int deptId)
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Departments)
                .Where(p => p.dept_id == deptId)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();

            return projects.Select(MapToResponseDto).ToList();
        }

        public async Task<List<ProjectResponseDto>> GetProjectsByCustomerAsync(int customerId)
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.Departments)
                .Where(p => p.cust_id == customerId)
                .OrderByDescending(p => p.created_at)
                .ToListAsync();

            return projects.Select(MapToResponseDto).ToList();
        }

        private async Task<(bool authorized, string message)> ValidateProjectWriteAccessAsync(int projectId, int userId, string userRole)
        {
            // 1. Admins and Managers have global access to all projects
            if (string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return (true, string.Empty);
            }

            // 2. Team Leads can only modify projects they are explicitly assigned to
            if (string.Equals(userRole, "Team Lead", StringComparison.OrdinalIgnoreCase))
            {
                var isAssigned = await _context.ProjectTeams
                    .AnyAsync(pt => pt.proj_id == projectId && pt.user_id == userId);

                if (isAssigned)
                    return (true, string.Empty);

                return (false, "Unauthorized: Team Leads can only modify projects they are assigned to.");
            }

            // 3. Members and Viewers are strictly blocked
            return (false, "Unauthorized: Only Admins, Managers, or assigned Team Leads can modify project details.");
        }
    }
}