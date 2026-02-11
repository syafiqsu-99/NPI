using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class TaskRevisions
    {
        [Key]
        public int task_revision_id { get; set; }

        public int revision_id { get; set; }

        public int task_id { get; set; }

        [Required]
        [StringLength(200)]
        public string title { get; set; }

        public DateOnly? start_date { get; set; }

        public DateOnly? end_date { get; set; }

        public float? duration { get; set; }

        public int? dept_id { get; set; }

        [StringLength(50)]
        public string? status { get; set; }

        [ForeignKey("revision_id")]
        public virtual ProjectRevisions? Revision { get; set; }

        [ForeignKey("task_id")]
        public virtual Tasks? Task { get; set; }

        [ForeignKey("dept_id")]
        public virtual Departments? Department { get; set; }
    }
}