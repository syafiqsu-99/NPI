using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class Milestones
    {
        [Key]
        public int milestone_id { get; set; }

        public int proj_id { get; set; }

        [Required]
        [StringLength(100)]
        public string milestone_name { get; set; }

        public DateOnly? planned_date { get; set; }
        public DateOnly? actual_date { get; set; }

        [StringLength(50)]
        public string? status { get; set; }

        public int? responsible_dept_id { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("responsible_dept_id")]
        public virtual Departments? ResponsibleDepartment { get; set; }
    }
}
