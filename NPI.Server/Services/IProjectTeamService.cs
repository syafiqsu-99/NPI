using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public interface IProjectTeamService
    {
        Task<List<ProjectTeamDto>> GetAllProjectTeamsAsync();
        Task<List<ProjectTeamDto>> GetTeamsByProjectAsync(int projId);
        Task<List<int>> GetProjectsByUserAsync(int userId);
    }
}