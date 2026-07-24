using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class TaskComments
    {
        [Key]
        public int comment_id { get; set; }

        public int task_id { get; set; }
        public int user_id { get; set; }

        [Required]
        [StringLength(2000)]
        public string body { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.Now;

        public bool is_deleted { get; set; } = false;
        public int? deleted_by { get; set; }
        public DateTime? deleted_at { get; set; }

        [ForeignKey("task_id")]
        public virtual Tasks? Task { get; set; }

        [ForeignKey("user_id")]
        public virtual Users? User { get; set; }
    }
}
