using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        public string role_name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? description { get; set; }

        public bool is_active { get; set; } = true;
    }

    public class UpdateRoleDto
    {
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        public string? role_name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? description { get; set; }

        public bool? is_active { get; set; }
    }

    public class RoleResponseDto
    {
        public int role_id { get; set; }
        public string role_name { get; set; }
        public string? description { get; set; }
        public bool is_active { get; set; }
        public int user_count { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
