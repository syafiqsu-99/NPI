using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Project ID is required")]
        public int proj_id { get; set; }

        public int? parent_task_id { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? description { get; set; }

        public DateOnly? planned_start_date { get; set; }

        public DateOnly? planned_end_date { get; set; }

        public DateOnly? actual_start_date { get; set; }

        public DateOnly? actual_end_date { get; set; }

        [Range(0, float.MaxValue, ErrorMessage = "Duration must be non-negative")]
        public float? duration { get; set; }

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public float? per_complete { get; set; }

        public int? dept_id { get; set; }

        public int? assigned_to { get; set; }

        [StringLength(20)]
        public string? priority { get; set; }

        [StringLength(20)]
        public string? status { get; set; }
    }
    public class UpdateTaskDto
    {
        public int? parent_task_id { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? description { get; set; }

        public DateOnly? planned_start_date { get; set; }

        public DateOnly? planned_end_date { get; set; }

        public DateOnly? actual_start_date { get; set; }

        public DateOnly? actual_end_date { get; set; }

        [Range(0, float.MaxValue, ErrorMessage = "Duration must be non-negative")]
        public float? duration { get; set; }

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public float? per_complete { get; set; }

        public int? dept_id { get; set; }

        public int? assigned_to { get; set; }

        [StringLength(20)]
        public string? priority { get; set; }

        [StringLength(20)]
        public string? status { get; set; }
    }

    public class UpdateTaskStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Not Started|In Progress|On Hold|Completed|Cancelled)$",
            ErrorMessage = "Invalid status value")]
        public string status { get; set; }
    }

    public class UpdateTaskProgressDto
    {
        [Required(ErrorMessage = "Progress percentage is required")]
        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public float per_complete { get; set; }
    }

    public class UpdatePlannedDatesDto
    {
        [Required(ErrorMessage = "New start date is required")]
        public DateOnly new_start_date { get; set; }

        [Required(ErrorMessage = "New end date is required")]
        public DateOnly new_end_date { get; set; }

        [Required(ErrorMessage = "Note is required")]
        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string note { get; set; }
    }

    public class TaskResponseDto
    {
        public int task_id { get; set; }
        public int proj_id { get; set; }
        public string? proj_no { get; set; }
        public string? proj_name { get; set; }
        public string? proj_status { get; set; }
        public string? proj_priority { get; set; }
        public int? parent_task_id { get; set; }
        public int order { get; set; }
        public string? stage_id { get; set; }
        public string? task_code { get; set; }
        public string title { get; set; }
        public string? description { get; set; }
        public int? dept_id { get; set; }
        public string? dept_name { get; set; }
        public int? assigned_to { get; set; }
        public string? assigned_to_name { get; set; }
        public Boolean is_milestone { get; set; }
        public int? assigned_by { get; set; }
        public string? assigned_by_name { get; set; }
        public DateOnly? planned_start_date { get; set; }
        public DateOnly? planned_end_date { get; set; }
        public DateOnly? actual_start_date { get; set; }
        public DateOnly? actual_end_date { get; set; }
        public float? duration { get; set; }
        public string? status { get; set; }
        public string? priority { get; set; }
        public float? per_complete { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? completed_at { get; set; }
        public List<TaskRevisionDto> planned_revisions { get; set; } = new();
    }

    public class TaskRevisionDto
    {
        public int revision_no { get; set; }
        public DateOnly? old_start_date { get; set; }
        public DateOnly? old_end_date { get; set; }
        public DateOnly? new_start_date { get; set; }
        public DateOnly? new_end_date { get; set; }
        public string? note { get; set; }
        public DateTime revised_on { get; set; }
    }
}
