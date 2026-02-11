using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    public class ProjectRevisions
    {
        [Key]
        public int revision_id { get; set; }

        public int proj_id { get; set; }

        public int revision_number { get; set; }

        public DateTime revision_date { get; set; } = DateTime.Now;

        public int revised_by { get; set; }

        [StringLength(500)]
        public string? revision_notes { get; set; }

        public DateOnly? previous_target_date { get; set; }

        public DateOnly? new_target_date { get; set; }

        public bool is_active { get; set; } = true;

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("revised_by")]
        public virtual Users? RevisedByUser { get; set; }

        public virtual ICollection<TaskRevisions>? TaskRevisions { get; set; }
    }
}