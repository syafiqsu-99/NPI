using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class CreateDepartmentDto
    {
        [Required]
        [StringLength(100)]
        public string dept_name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? description { get; set; }
    }

    public class UpdateDepartmentDto
    {
        [Required]
        [StringLength(100)]
        public string dept_name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? description { get; set; }
    }
    public class DepartmentResponseDto
    {
        public int dept_id { get; set; }
        public string dept_name { get; set; }
        public string? description { get; set; }
        public DateTime created_at { get; set; }
    }
}
