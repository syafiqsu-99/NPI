using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string password { get; set; }

        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? full_name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? email { get; set; }

        public int? dept_id { get; set; }

        public int? role_id { get; set; }

        public bool is_active { get; set; } = true;
    }

    public class UpdateUserDto
    {
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string? username { get; set; }

        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? full_name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? email { get; set; }

        public int? dept_id { get; set; }

        public int? role_id { get; set; }

        public bool? is_active { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string current_password { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string new_password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("new_password", ErrorMessage = "Passwords do not match")]
        public string confirm_password { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string new_password { get; set; }
    }

    public class UserDetailDto
    {
        public int user_id { get; set; }
        public string username { get; set; }
        public string? full_name { get; set; }
        public string? email { get; set; }
        public int? dept_id { get; set; }
        public string? dept_name { get; set; }
        public int? role_id { get; set; }
        public string? role_name { get; set; }
        public string? role { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? last_login { get; set; }
    }

    public class UserListDto
    {
        public int user_id { get; set; }
        public string username { get; set; }
        public string? full_name { get; set; }
        public string? email { get; set; }
        public string? dept_name { get; set; }
        public string? role_name { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
    }

    public class UserResponseDto
    {
        public int user_id { get; set; }
        public string username { get; set; }
        public string? full_name { get; set; }
        public string? email { get; set; }
        public int? dept_id { get; set; }
        public string? dept_name { get; set; }
        public string? role { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
    }
}
