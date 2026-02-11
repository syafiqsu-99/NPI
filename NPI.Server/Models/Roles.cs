using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class Roles
    {
        [Key]
        public int role_id { get; set; }

        [Required]
        [StringLength(50)]
        public string role_name { get; set; }

        [StringLength(200)]
        public string? description { get; set; }

        public virtual ICollection<Users>? Users { get; set; }
        public virtual ICollection<RolePermissions>? RolePermissions { get; set; }
    }
}
