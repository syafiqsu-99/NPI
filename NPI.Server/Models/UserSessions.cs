using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    public class UserSessions
    {
        [Key]
        [StringLength(100)]
        public string session_id { get; set; }

        public int user_id { get; set; }

        [Required]
        public string token_hash { get; set; }

        [StringLength(50)]
        public string? ip_address { get; set; }

        [StringLength(200)]
        public string? user_agent { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime expires_at { get; set; }
        public bool is_active { get; set; } = true;

        [ForeignKey("user_id")]
        public virtual Users? User { get; set; }
    }
}
