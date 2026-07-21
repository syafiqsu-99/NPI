using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class TaskTemplates
    {
        [Key]
        public int template_id { get; set; }

        [Required, MaxLength(10)]
        public string stage_id { get; set; } = null!;

        [Required, MaxLength(20)]
        public string task_code { get; set; } = null!;

        [Required, MaxLength(200)]
        public string title { get; set; } = null!;

        [Required]
        public int dept_id { get; set; }

        public int default_duration { get; set; } = 5;

        public bool has_link { get; set; }

        [MaxLength(50)]
        public string? doc_format { get; set; }

        [MaxLength(50)]
        public string? role_gated { get; set; }

        public int display_order { get; set; }

        public bool is_active { get; set; } = true;

        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }

        [ForeignKey(nameof(dept_id))]
        public Departments? Department { get; set; }
    }
}
