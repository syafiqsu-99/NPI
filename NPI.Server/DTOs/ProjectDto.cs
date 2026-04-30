using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class CreateProjectDto
    {
        [Required(ErrorMessage = "Project name is required")]
        [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
        public string proj_name { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? description { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int dept_id { get; set; }

        [RegularExpression("^(Low|Medium|High|Critical)$",
            ErrorMessage = "Priority must be Low, Medium, High, or Critical")]
        public string? priority { get; set; }

        [RegularExpression("^(Planning|Not Started|In Progress|On Hold|Completed|Cancelled)$",
            ErrorMessage = "Invalid status value")]
        public string? status { get; set; }

        public DateOnly? project_start_date { get; set; }

        public DateOnly? target_completion_date { get; set; }
    }

    public class UpdateProjectDto
    {
        [Required(ErrorMessage = "Project name is required")]
        [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
        public string proj_name { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? description { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int dept_id { get; set; }

        [RegularExpression("^(Low|Medium|High|Critical)$",
            ErrorMessage = "Priority must be Low, Medium, High, or Critical")]
        public string? priority { get; set; }

        [RegularExpression("^(Planning|Not Started|In Progress|On Hold|Completed|Cancelled)$",
            ErrorMessage = "Invalid status value")]
        public string? status { get; set; }

        public DateOnly? project_start_date { get; set; }

        public DateOnly? target_completion_date { get; set; }
    }

    public class UpdateProjectStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Planning|Not Started|In Progress|On Hold|Completed|Cancelled)$",
            ErrorMessage = "Invalid status value")]
        public string status { get; set; }
    }

    public class ProjectResponseDto
    {
        public int proj_id { get; set; }
        public string proj_no { get; set; }
        public string proj_name { get; set; }
        public int? cust_id { get; set; }
        public string? customer_name { get; set; }
        public int? dept_id { get; set; }
        public string? dept_name { get; set; }
        public DateOnly? project_start_date { get; set; }
        public DateOnly? target_completion_date { get; set; }
        public DateOnly? actual_completion_date { get; set; }
        public string? priority { get; set; }
        public string? status { get; set; }
        public string? description { get; set; }
        public string? storage_path { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public bool pilot_mould_required { get; set; }
        public bool machine_purchase_required { get; set; }
        public List<TeamMemberDto> team_members { get; set; } = new();
        public List<ProjectRevisionDto>? revisions { get; set; }
        public Dictionary<string, StageProgressDto>? stage_progress { get; set; }
    }

    public class TeamMemberDto
    {
        public int user_id { get; set; }
        public string? user_name { get; set; }
        public string? full_name { get; set; }
        public int dept_id { get; set; }
        public string? dept_name { get; set; }
        public string? role { get; set; }
        public DateTime assigned_at { get; set; }
        public string? email { get; set; }
    }

    public class LaunchProjectDto
    {
        [Required(ErrorMessage = "Team members are required")]
        [MinLength(1, ErrorMessage = "At least one team member is required")]
        public List<TeamMemberInputDto> team_members { get; set; }

        [Required(ErrorMessage = "Tasks are required")]
        [MinLength(1, ErrorMessage = "At least one task is required")]
        public List<TaskUpdateDto> tasks { get; set; }

        public List<MilestoneDto>? milestones { get; set; }

        [StringLength(50)]
        public string? priority { get; set; }
        public string? description { get; set; }
        public bool pilot_mould_required { get; set; } = false;
        public bool machine_purchase_required { get; set; } = false;
    }

    public class TeamMemberInputDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int user_id { get; set; }

        public int? dept_id { get; set; }

        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
        public string? role { get; set; }
        public string? dept_name { get; set; }
        public string? user_name { get; set; }
    }

    public class TaskUpdateDto
    {
        public int? task_id { get; set; }
        public string? stage_id { get; set; }
        public string? task_code { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string title { get; set; }

        public DateOnly? start_date { get; set; }

        public DateOnly? end_date { get; set; }

        [Range(0, float.MaxValue, ErrorMessage = "Duration must be non-negative")]
        public float? duration { get; set; }

        [StringLength(100)]
        public string? dept_name { get; set; }

        [StringLength(500, ErrorMessage = "Revision note cannot exceed 500 characters")]
        public string? revision_note { get; set; }
    }

    public class MilestoneDto
    {
        [Required(ErrorMessage = "Milestone name is required")]
        [StringLength(200, ErrorMessage = "Milestone name cannot exceed 200 characters")]
        public string milestone_name { get; set; }

        [Required(ErrorMessage = "Planned date is required")]
        public DateOnly planned_date { get; set; }

        [RegularExpression("^(Pending|In Progress|Completed|Delayed)$",
            ErrorMessage = "Invalid status value")]
        public string? status { get; set; }

        public int task_id { get; set; }
        public string? stage_id { get; set; }
        public string? task_code { get; set; }

        public int? responsible_dept_id { get; set; }
        public string? dept_name { get; set; }
    }

    public class StageProgressDto
    {
        public bool completed { get; set; }
        public bool in_progress { get; set; }
        public int task_count { get; set; }
        public int completed_count { get; set; }
    }

    public class CreateProjectFromEnquiryDto
    {
        public string? project_name { get; set; }
        public string? priority { get; set; }
        public string? description { get; set; }
        public DateOnly? expected_completion { get; set; }
    }
}
