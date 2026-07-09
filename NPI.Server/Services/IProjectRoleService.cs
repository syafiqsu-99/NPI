using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public interface IProjectRoleService
    {
        Task<string?> GetProjectRoleAsync(int projectId, int userId);
        Task<bool> HasProjectRoleAsync(int projectId, int userId, string systemRole, string minimumRole);
        Task<(bool success, string message)> UpsertProjectRoleAsync(int projectId, int userId, string roleName, int assignedBy);
        Task<List<ProjectTeamDto>> GetProjectRolesAsync(int projectId);
        Task<List<ProjectTeamDto>> GetAllProjectTeamsAsync();
        Task<List<int>> GetProjectsByUserAsync(int userId);
    }
}
