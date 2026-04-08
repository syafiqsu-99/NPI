using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class ProjectRoles
    {
        [Key]
        public int proj_role_id { get; set; }

        [Required]
        public int proj_id { get; set; }

        [Required]
        public int user_id { get; set; }

        [Required]
        [MaxLength(50)]
        public string role_name { get; set; } = "Member";

        [StringLength(200)]
        public string? description { get; set; }

        public bool is_active { get; set; } = true;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("user_id")]
        public virtual Users? User { get; set; }
    }
}
