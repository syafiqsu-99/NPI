using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class ProjectStatusHistory
    {
        [Key]
        public int history_id { get; set; }

        public int proj_id { get; set; }

        [StringLength(50)]
        public string? old_status { get; set; }

        [StringLength(50)]
        public string? new_status { get; set; }

        public int changed_by { get; set; }
        public DateTime changed_at { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string? remark { get; set; }

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("changed_by")]
        public virtual Users? ChangedByUser { get; set; }
    }
}
