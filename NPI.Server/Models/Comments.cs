using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class Comments
    {
        [Key]
        public int comment_id { get; set; }

        public int? proj_id { get; set; }
        public int? task_id { get; set; }
        public int user_id { get; set; }

        [Required]
        public string comment_text { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("task_id")]
        public virtual Tasks? Task { get; set; }

        [ForeignKey("user_id")]
        public virtual Users? User { get; set; }
    }
}
