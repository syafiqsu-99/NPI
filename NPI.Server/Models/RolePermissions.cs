using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class RolePermissions
    {
        [Key]
        public int permission_id { get; set; }

        public int role_id { get; set; }

        [Required]
        [StringLength(50)]
        public string resource { get; set; }

        public bool can_create { get; set; } = false;
        public bool can_read { get; set; } = false;
        public bool can_update { get; set; } = false;
        public bool can_delete { get; set; } = false;
        public bool can_approve { get; set; } = false;

        [ForeignKey("role_id")]
        public virtual Roles? Role { get; set; }
    }
}
