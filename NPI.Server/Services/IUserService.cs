using NPI.Server.DTOs;

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
        Task<List<UserListDto>> GetUsersByDepartmentAsync(int deptId);
        Task<List<UserListDto>> GetUsersByRoleAsync(int roleId);
    }
}
