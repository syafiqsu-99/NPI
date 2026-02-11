namespace NPI.Server.Services
{
    public interface IProjectService
    {
        Task<(bool success, string message, int? proj_id)> CreateProjectFromEnquiryAsync(int enquiryId, int userId);
        Task<List<ProjectResponseDto>> GetAllProjectsAsync();
        Task<ProjectResponseDto?> GetProjectByIdAsync(int projectId);
        Task<List<TaskResponseDto>> GetProjectTasksAsync(int projectId);
        Task<(bool success, string message)> LaunchProjectAsync(int projectId, LaunchProjectDto dto, int userId);
    }
}

public class ProjectResponseDto
{
    public int proj_id { get; set; }
    public string proj_no { get; set; }
    public string proj_name { get; set; }
    public int cust_id { get; set; }
    public string? customer_name { get; set; }
    public DateOnly? project_start_date { get; set; }
    public DateOnly? target_completion_date { get; set; }
    public string? priority { get; set; }
    public string? status { get; set; }
    public string? description { get; set; }
    public string? storage_path { get; set; }
    public List<TeamMemberDto> team_members { get; set; } = new();
}

public class TaskResponseDto
{
    public int task_id { get; set; }
    public int? parent_task_id { get; set; }
    public int order { get; set; }
    public string title { get; set; }
    public string? dept_name { get; set; }
    public DateOnly? start_date { get; set; }
    public DateOnly? end_date { get; set; }
    public float? duration { get; set; }
    public string? status { get; set; }
}


public class TeamMemberDto
{
    public int user_id { get; set; }
    public int dept_id { get; set; }
    public string role { get; set; }
    public string? user_name { get; set; }
    public string? dept_name { get; set; }
}

public class LaunchProjectDto
{
    public List<TeamMemberDto> team_members { get; set; }
    public List<TaskUpdateDto> tasks { get; set; }
    public List<MilestoneDto>? milestones { get; set; }
}

public class MilestoneDto
{
    public string milestone_name { get; set; }
    public DateOnly? planned_date { get; set; }
}

public class TaskUpdateDto
{
    public int? task_id { get; set; }
    public string title { get; set; }
    public DateOnly? start_date { get; set; }
    public DateOnly? end_date { get; set; }
    public float? duration { get; set; }
    public string? dept_name { get; set; }
}