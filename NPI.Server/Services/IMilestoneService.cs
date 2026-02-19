using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public interface IMilestoneService
    {
        Task<List<MilestoneResponseDto>> GetProjectMilestonesAsync(int projectId);
        Task<MilestoneResponseDto?> GetMilestoneByIdAsync(int milestoneId);
        Task<(bool success, string message, MilestoneResponseDto? milestone)> CreateMilestoneAsync(int projectId, CreateMilestoneDto dto);
        Task<(bool success, string message)> UpdateMilestoneAsync(int milestoneId, int projectId, UpdateMilestoneDto dto);
        Task<(bool success, string message)> DeleteMilestoneAsync(int milestoneId, int projectId);
        Task<(bool success, string message)> CompleteMilestoneAsync(int milestoneId, int projectId);
    }
}
