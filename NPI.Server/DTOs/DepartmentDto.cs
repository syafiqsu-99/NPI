using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class CreateDepartmentDto
    {
        [Required]
        [StringLength(100)]
        public string dept_name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? dept_code { get; set; }

        [StringLength(500)]
        public string? description { get; set; }

        [StringLength(10)]
        public string? color_hex { get; set; }

        public bool is_assignable { get; set; } = true;
    }

    public class UpdateDepartmentDto
    {
        [Required]
        [StringLength(100)]
        public string dept_name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? dept_code { get; set; }

        [StringLength(500)]
        public string? description { get; set; }

        [StringLength(10)]
        public string? color_hex { get; set; }

        public bool is_assignable { get; set; } = true;
    }

    public class DepartmentResponseDto
    {
        public int dept_id { get; set; }
        public string dept_name { get; set; } = string.Empty;
        public string? dept_code { get; set; }
        public string? description { get; set; }
        public string? color_hex { get; set; }
        public bool is_assignable { get; set; }
        public int user_count { get; set; }
        public DateTime created_at { get; set; }
    }
}