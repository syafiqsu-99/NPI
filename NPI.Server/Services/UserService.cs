using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IUserService
    {
        Task<List<UserListDto>> GetAllUsersAsync();
        Task<UserDetailDto?> GetUserByIdAsync(int userId);
        Task<(bool success, string message, int userId)> CreateUserAsync(CreateUserDto dto);
        Task<(bool success, string message)> UpdateUserAsync(int userId, UpdateUserDto dto);
        Task<(bool success, string message)> DeleteUserAsync(int userId);
        Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<(bool success, string message)> ResetPasswordAsync(int userId, ResetPasswordDto dto);
        Task<(bool success, string message)> ToggleUserStatusAsync(int userId);
        Task<(bool success, string message)> UpdateOwnProfileAsync(int userId, UpdateMyProfileDto dto);
        Task<List<UserListDto>> GetUsersByDepartmentAsync(int deptId);
        Task<List<UserListDto>> GetUsersByRoleAsync(int roleId);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserListDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .OrderBy(u => u.username)
                .Select(u => new UserListDto
                {
                    user_id = u.user_id,
                    username = u.username,
                    full_name = u.full_name,
                    email = u.email,
                    dept_id = u.dept_id,
                    dept_name = u.Department != null ? u.Department.dept_name : null,
                    role_name = u.Role != null ? u.Role.role_name : null,
                    is_active = u.is_active,
                    created_at = u.created_at
                })
                .ToListAsync();

            return users;
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Where(u => u.user_id == userId)
                .Select(u => new UserDetailDto
                {
                    user_id = u.user_id,
                    username = u.username,
                    full_name = u.full_name,
                    email = u.email,
                    dept_id = u.dept_id,
                    dept_name = u.Department != null ? u.Department.dept_name : null,
                    role_id = u.role_id,
                    role_name = u.Role != null ? u.Role.role_name : null,
                    is_active = u.is_active,
                    created_at = u.created_at,
                    last_login = u.last_login
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<(bool success, string message, int userId)> CreateUserAsync(CreateUserDto dto)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.username == dto.username);

                if (existingUser != null)
                    return (false, "Username already exists", 0);

                if (!string.IsNullOrEmpty(dto.email))
                {
                    var existingEmail = await _context.Users
                        .FirstOrDefaultAsync(u => u.email == dto.email);

                    if (existingEmail != null)
                        return (false, "Email already exists", 0);
                }

                if (dto.dept_id.HasValue)
                {
                    var deptExists = await _context.Departments
                        .AnyAsync(d => d.dept_id == dto.dept_id.Value);

                    if (!deptExists)
                        return (false, "Department not found", 0);
                }

                if (dto.role_id.HasValue)
                {
                    var roleExists = await _context.Roles
                        .AnyAsync(r => r.role_id == dto.role_id.Value);

                    if (!roleExists)
                        return (false, "Role not found", 0);
                }

                if (!dto.role_id.HasValue)
                {
                    var defaultRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.role_name == SystemRoles.Member);
                    dto.role_id = defaultRole?.role_id;
                }

                var hashedPassword = PasswordHelper.HashPassword(dto.password);

                var user = new Users
                {
                    username = dto.username,
                    password_hash = hashedPassword,
                    full_name = dto.full_name,
                    email = dto.email,
                    dept_id = dto.dept_id,
                    role_id = dto.role_id,
                    is_active = dto.is_active,
                    created_at = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return (true, "User created successfully", user.user_id);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating user: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> UpdateUserAsync(int userId, UpdateUserDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return (false, "User not found");

                if (!string.IsNullOrEmpty(dto.username) && dto.username != user.username)
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.username == dto.username);

                    if (existingUser != null)
                        return (false, "Username already exists");

                    user.username = dto.username;
                }

                if (!string.IsNullOrEmpty(dto.email) && dto.email != user.email)
                {
                    var existingEmail = await _context.Users
                        .FirstOrDefaultAsync(u => u.email == dto.email);

                    if (existingEmail != null)
                        return (false, "Email already exists");

                    user.email = dto.email;
                }

                if (!string.IsNullOrEmpty(dto.full_name))
                    user.full_name = dto.full_name;

                if (dto.dept_id.HasValue)
                {
                    var deptExists = await _context.Departments
                        .AnyAsync(d => d.dept_id == dto.dept_id.Value);

                    if (!deptExists)
                        return (false, "Department not found");

                    user.dept_id = dto.dept_id.Value;
                }

                if (dto.role_id.HasValue)
                {
                    var roleExists = await _context.Roles
                        .AnyAsync(r => r.role_id == dto.role_id.Value);

                    if (!roleExists)
                        return (false, "Role not found");

                    user.role_id = dto.role_id.Value;
                }

                if (dto.is_active.HasValue)
                    user.is_active = dto.is_active.Value;

                user.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return (true, "User updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating user: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return (false, "User not found");

                var hasProjects = await _context.Projects
                    .AnyAsync(p => p.created_by == userId);

                var hasTasks = await _context.Tasks
                    .AnyAsync(t => t.assigned_to == userId || t.assigned_by == userId);

                if (hasProjects || hasTasks)
                {
                    user.is_active = false;
                    user.updated_at = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return (true, "User deactivated (has related records)");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return (true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting user: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return (false, "User not found");

                if (!PasswordHelper.VerifyPassword(dto.current_password, user.password_hash))
                    return (false, "Current password is incorrect");

                user.password_hash = PasswordHelper.HashPassword(dto.new_password);
                user.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return (true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error changing password: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> ResetPasswordAsync(
            int userId, ResetPasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return (false, "User not found");

                user.password_hash = PasswordHelper.HashPassword(dto.new_password);
                user.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return (true, "Password reset successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error resetting password: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> ToggleUserStatusAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return (false, "User not found");

                user.is_active = !user.is_active;
                user.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                var status = user.is_active ? "activated" : "deactivated";
                return (true, $"User {status} successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error toggling user status: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UpdateOwnProfileAsync(int userId, UpdateMyProfileDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user is null) return (false, "User not found");

                if (!string.IsNullOrWhiteSpace(dto.full_name))
                    user.full_name = dto.full_name.Trim();

                user.updated_at = DateTime.Now;
                await _context.SaveChangesAsync();

                return (true, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating profile: {ex.Message}");
            }
        }

        public async Task<List<UserListDto>> GetUsersByDepartmentAsync(int deptId)
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Where(u => u.dept_id == deptId)
                .OrderBy(u => u.username)
                .Select(u => new UserListDto
                {
                    user_id = u.user_id,
                    username = u.username,
                    full_name = u.full_name,
                    email = u.email,
                    dept_name = u.Department != null ? u.Department.dept_name : null,
                    role_name = u.Role != null ? u.Role.role_name : null,
                    is_active = u.is_active,
                    created_at = u.created_at
                })
                .ToListAsync();

            return users;
        }

        public async Task<List<UserListDto>> GetUsersByRoleAsync(int roleId)
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Where(u => u.role_id == roleId)
                .OrderBy(u => u.username)
                .Select(u => new UserListDto
                {
                    user_id = u.user_id,
                    username = u.username,
                    full_name = u.full_name,
                    email = u.email,
                    dept_name = u.Department != null ? u.Department.dept_name : null,
                    role_name = u.Role != null ? u.Role.role_name : null,
                    is_active = u.is_active,
                    created_at = u.created_at
                })
                .ToListAsync();

            return users;
        }
    }
}
