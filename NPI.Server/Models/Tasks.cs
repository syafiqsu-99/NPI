using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NPI.Server.Models
{
    public class Tasks
    {
        [Key]
        public int task_id { get; set; }

        public int proj_id { get; set; }
        public int? parent_task_id { get; set; }

        [Required]
        [StringLength(200)]
        public string title { get; set; }

        public string? description { get; set; }

        public DateOnly? start_date { get; set; }
        public DateOnly? end_date { get; set; }
        public float? duration { get; set; }
        public float? per_complete { get; set; }

        public int? dept_id { get; set; }
        public int? assigned_to { get; set; }
        public int? assigned_by { get; set; }

        [StringLength(20)]
        public string? priority { get; set; }

        [StringLength(50)]
        public string? status { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }
        public DateTime? completed_at { get; set; }

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("parent_task_id")]
        public virtual Tasks? ParentTask { get; set; }

        [ForeignKey("dept_id")]
        public virtual Departments? Department { get; set; }

        [ForeignKey("assigned_to")]
        public virtual Users? AssignedToUser { get; set; }

        [ForeignKey("assigned_by")]
        public virtual Users? AssignedByUser { get; set; }

        public virtual ICollection<Tasks>? SubTasks { get; set; }
        public virtual ICollection<Files>? Files { get; set; }
        public virtual ICollection<Approvals>? Approvals { get; set; }
        public virtual ICollection<Comments>? Comments { get; set; }
    }
}
