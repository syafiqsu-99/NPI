using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NPI.Server.Models
{
    public class Projects
    {
        [Key]
        public int proj_id { get; set; }

        [Required]
        [StringLength(50)]
        public string proj_no { get; set; }

        [Required]
        [StringLength(200)]
        public string proj_name { get; set; }

        public int cust_id { get; set; }

        public DateOnly? enquiry_date { get; set; }
        public DateOnly? project_start_date { get; set; }
        public DateOnly? target_completion_date { get; set; }
        public DateOnly? actual_completion_date { get; set; }

        [StringLength(20)]
        public string? priority { get; set; }

        [StringLength(50)]
        public string? status { get; set; }

        public string? description { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
        public int created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public int? updated_by { get; set; }

        [StringLength(500)]
        public string? storage_path { get; set; }

        [ForeignKey("cust_id")]
        public virtual Customers? Customer { get; set; }

        [ForeignKey("created_by")]
        public virtual Users? CreatedByUser { get; set; }

        [ForeignKey("updated_by")]
        public virtual Users? UpdatedByUser { get; set; }

        public virtual ICollection<ProjectTeam>? ProjectTeams { get; set; }
        public virtual ICollection<Milestones>? Milestones { get; set; }
        public virtual ICollection<Tasks>? Tasks { get; set; }
        public virtual ICollection<Files>? Files { get; set; }
        public virtual ICollection<Approvals>? Approvals { get; set; }
        public virtual ICollection<Notifications>? Notifications { get; set; }
        public virtual ICollection<Comments>? Comments { get; set; }
        public virtual ICollection<AuditLogs>? AuditLogs { get; set; }
        public virtual ICollection<ProjectStatusHistory>? ProjectStatusHistories { get; set; }
    }
}
