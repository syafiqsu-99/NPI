using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    public class StageCompletionLog
    {
        [Key]
        public int log_id { get; set; }

        public int proj_id { get; set; }

        [Required]
        [StringLength(10)]
        public string stage_id { get; set; }

        public int? completed_by { get; set; }

        public DateTime completed_at { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? notes { get; set; }

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("completed_by")]
        public virtual Users? CompletedByUser { get; set; }
    }
}