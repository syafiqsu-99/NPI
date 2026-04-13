using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NPI.Server.Models
{
    public class Users
    {
        [Key]
        public int user_id { get; set; }

        [Required]
        [StringLength(100)]
        public string username { get; set; }

        public string? email { get; set; }

        [StringLength(100)]
        public string? full_name { get; set; }

        public int? dept_id { get; set; }
        public int? role_id { get; set; }

        [Required]
        public string password_hash { get; set; }

        public bool is_active { get; set; } = true;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? last_login { get; set; }

        [ForeignKey("dept_id")]
        public virtual Departments? Department { get; set; }

        [ForeignKey("role_id")]
        public virtual Roles? Role { get; set; }

        public virtual ICollection<ProjectTeams>? ProjectTeams { get; set; }
        public virtual ICollection<Tasks>? AssignedTasks { get; set; }
        public virtual ICollection<Files>? UploadedFiles { get; set; }
        public virtual ICollection<Notifications>? Notifications { get; set; }
        public virtual ICollection<UserSessions>? UserSessions { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
