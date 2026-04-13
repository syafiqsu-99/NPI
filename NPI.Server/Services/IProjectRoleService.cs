using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IProjectRoleService
    {
        Task<string?> GetProjectRoleAsync(int projectId, int userId);
        Task<bool> HasProjectRoleAsync(int projectId, int userId, string minimumRole);
        Task<(bool success, string message)> UpsertProjectRoleAsync(int projectId, int userId, string roleName, int assignedBy);
        Task<List<ProjectTeamDto>> GetProjectRolesAsync(int projectId);
    }
}
