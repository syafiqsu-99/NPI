using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    public class Milestones
    {
        [Key]
        public int milestone_id { get; set; }

        public int proj_id { get; set; }

        public int? task_id { get; set; }

        [Required]
        [StringLength(100)]
        public string milestone_name { get; set; }
        public string? description { get; set; }

        public DateOnly? planned_date { get; set; }
        public DateOnly? actual_date { get; set; }

        [StringLength(50)]
        public string? status { get; set; }

        public int? responsible_dept_id { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;

        [ForeignKey("task_id")]
        public virtual Tasks? Tasks { get; set; }

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("responsible_dept_id")]
        public virtual Departments? ResponsibleDepartment { get; set; }
    }
}
