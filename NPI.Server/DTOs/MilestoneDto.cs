using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class CreateMilestoneDto
    {
        [Required(ErrorMessage = "Milestone name is required")]
        [StringLength(200, ErrorMessage = "Milestone name cannot exceed 200 characters")]
        public string milestone_name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? description { get; set; }

        [Required(ErrorMessage = "Planned date is required")]
        public DateOnly planned_date { get; set; }

        public int? dept_id { get; set; }
    }

    public class UpdateMilestoneDto
    {
        [StringLength(200, ErrorMessage = "Milestone name cannot exceed 200 characters")]
        public string? milestone_name { get; set; }

        public DateOnly? planned_date { get; set; }

        public DateOnly? actual_date { get; set; }

        public int? responsible_dept_id { get; set; }
    }

    public class MilestoneResponseDto
    {
        public int milestone_id { get; set; }
        public int proj_id { get; set; }
        public int task_id { get; set; }
        public string milestone_name { get; set; }
        public string? description { get; set; }
        public DateOnly? planned_date { get; set; }
        public DateOnly? actual_date { get; set; }
        public string? status { get; set; }
        public int? responsible_dept_id { get; set; }
        public string? dept_name { get; set; }
        public bool is_completed { get; set; }
        public bool is_delayed { get; set; }
        public DateTime created_at { get; set; }
    }
}
