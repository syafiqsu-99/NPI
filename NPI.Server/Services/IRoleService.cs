using NPI.Server.DTOs;

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
}
