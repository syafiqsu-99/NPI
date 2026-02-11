using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class Approvals
    {
        [Key]
        public int appr_id { get; set; }

        public int proj_id { get; set; }
        public int? file_id { get; set; }
        public int? task_id { get; set; }

        [StringLength(50)]
        public string? appr_type { get; set; }

        public int? appr_level { get; set; }
        public int? appr_order { get; set; }

        [StringLength(50)]
        public string? status { get; set; }

        public int? apprv_by { get; set; }
        public DateTime? approved_at { get; set; }
        public DateTime? rejected_at { get; set; }

        public string? remark { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("file_id")]
        public virtual Files? File { get; set; }

        [ForeignKey("task_id")]
        public virtual Tasks? Task { get; set; }

        [ForeignKey("apprv_by")]
        public virtual Users? ApprovedByUser { get; set; }
    }
}
