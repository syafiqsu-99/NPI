using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class Departments
    {
        [Key]
        public int dept_id { get; set; }

        [Required]
        [StringLength(100)]
        public string dept_name { get; set; }

        [StringLength(20)]
        public string? dept_code { get; set; }

        [StringLength(200)]
        public string? description { get; set; }

        [StringLength(10)]
        public string? color_hex { get; set; }
        public bool is_assignable { get; set; } = true;

        public virtual ICollection<Users>? Users { get; set; }
        public virtual ICollection<Files>? Files { get; set; }
        public DateTime created_at { get; internal set; }
        public DateTime updated_at { get; internal set; }
    }
}
