using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public interface IGanttService
    {
        Task<GanttDataDto> GetGanttDataAsync(int projectId, int? revisionId = null);
        Task<List<ProjectRevisionDto>> GetProjectRevisionsAsync(int projectId);
        Task<(bool success, string message, int? revisionId)> CreateRevisionAsync(
            int projectId, CreateRevisionDto dto, int userId);
    }
}
