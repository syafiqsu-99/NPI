using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public interface IProjectService
    {
        Task<List<ProjectResponseDto>> GetAllProjectsAsync();
        Task<ProjectResponseDto?> GetProjectByIdAsync(int projectId);
        Task<(bool success, string message, int projId)> CreateProjectAsync(CreateProjectDto dto, int userId);
        Task<(bool success, string message)> UpdateProjectAsync(int projectId, UpdateProjectDto dto, int userId, string userRole);
        Task<(bool success, string message)> DeleteProjectAsync(int projectId);
        Task<(bool success, string message, int proj_id)> CreateProjectFromEnquiryAsync(int enquiryId, int userId, CreateProjectFromEnquiryDto? dto = null);
        Task<List<TaskResponseDto>> GetProjectTasksAsync(int projectId);
        Task<List<MilestoneResponseDto>> GetProjectMilestonesAsync(int projectId);
        Task<(bool success, string message, List<string>? folderWarnings)> LaunchProjectAsync(int projectId, LaunchProjectDto dto, int userId);
        Task<(bool success, string message)> UpdateProjectStatusAsync(int projectId, string status, int userId, string userRole);
        Task<List<ProjectResponseDto>> GetProjectsByStatusAsync(string status);
        Task<List<ProjectResponseDto>> GetProjectsByDepartmentAsync(int deptId);
        Task<List<ProjectResponseDto>> GetProjectsByCustomerAsync(int customerId);
    }
}