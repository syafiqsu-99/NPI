using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IRoleService
    {
        Task<List<RoleResponseDto>> GetAllRolesAsync();
        Task<RoleResponseDto?> GetRoleByIdAsync(int roleId);
        Task<(bool success, string message, int roleId)> CreateRoleAsync(CreateRoleDto dto);
        Task<(bool success, string message)> UpdateRoleAsync(int roleId, UpdateRoleDto dto);
        Task<(bool success, string message)> DeleteRoleAsync(int roleId);
        Task<(bool success, string message)> ToggleRoleStatusAsync(int roleId);
    }

    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;

        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoleResponseDto>> GetAllRolesAsync()
        {
            var roles = await _context.Roles
                .OrderBy(r => r.role_name)
                .Select(r => new RoleResponseDto
                {
                    role_id = r.role_id,
                    role_name = r.role_name,
                    description = r.description,
                    is_active = r.is_active,
                    user_count = r.Users != null ? r.Users.Count : 0,
                    created_at = r.created_at,
                    updated_at = r.updated_at
                })
                .ToListAsync();

            return roles;
        }

        public async Task<RoleResponseDto?> GetRoleByIdAsync(int roleId)
        {
            var role = await _context.Roles
                .Where(r => r.role_id == roleId)
                .Select(r => new RoleResponseDto
                {
                    role_id = r.role_id,
                    role_name = r.role_name,
                    description = r.description,
                    is_active = r.is_active,
                    user_count = r.Users != null ? r.Users.Count : 0,
                    created_at = r.created_at,
                    updated_at = r.updated_at
                })
                .FirstOrDefaultAsync();

            return role;
        }

        public async Task<(bool success, string message, int roleId)> CreateRoleAsync(CreateRoleDto dto)
        {
            try
            {
                // Check if role name already exists
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.role_name == dto.role_name);

                if (existingRole != null)
                    return (false, "Role name already exists", 0);

                var role = new Roles
                {
                    role_name = dto.role_name,
                    description = dto.description,
                    is_active = dto.is_active,
                    created_at = DateTime.Now
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                return (true, "Role created successfully", role.role_id);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating role: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> UpdateRoleAsync(int roleId, UpdateRoleDto dto)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);

                if (role == null)
                    return (false, "Role not found");

                // Check role name uniqueness if changed
                if (!string.IsNullOrEmpty(dto.role_name) && dto.role_name != role.role_name)
                {
                    var existingRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.role_name == dto.role_name);

                    if (existingRole != null)
                        return (false, "Role name already exists");

                    role.role_name = dto.role_name;
                }

                if (!string.IsNullOrEmpty(dto.description))
                    role.description = dto.description;

                if (dto.is_active.HasValue)
                    role.is_active = dto.is_active.Value;

                role.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return (true, "Role updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating role: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteRoleAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Users)
                    .FirstOrDefaultAsync(r => r.role_id == roleId);

                if (role == null)
                    return (false, "Role not found");

                // Check if role has users
                if (role.Users != null && role.Users.Any())
                {
                    return (false, $"Cannot delete role with {role.Users.Count} assigned users. " +
                                   "Please reassign users first or deactivate the role.");
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return (true, "Role deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting role: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> ToggleRoleStatusAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles.FindAsync(roleId);

                if (role == null)
                    return (false, "Role not found");

                role.is_active = !role.is_active;
                role.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                var status = role.is_active ? "activated" : "deactivated";
                return (true, $"Role {status} successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error toggling role status: {ex.Message}");
            }
        }
    }
}
