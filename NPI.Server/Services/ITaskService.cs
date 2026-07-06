using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetAllTasksAsync();
        Task<TaskResponseDto?> GetTaskByIdAsync(int taskId);
        Task<(bool success, string message, int taskId)> CreateTaskAsync(CreateTaskDto dto, int userId);
        Task<(bool success, string message)> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int userId, string userRole, int userDeptId);
        Task<(bool success, string message)> DeleteTaskAsync(int taskId, int userId, string userRole, int userDeptId);
        Task<(bool success, string message)> UpdateTaskStatusAsync(int taskId, string status, int userId, string userRole, int userDeptId);
        Task<(bool success, string message)> UpdateTaskProgressAsync(int taskId, float per_complete, int userId, string userRole, int userDeptId);
        Task<(bool success, string message)> UpdatePlannedDatesAsync(int taskId, DateOnly? new_start_date, DateOnly? new_end_date, string? note, int userId, string userRole, int userDeptId);
        Task<(bool authorized, string message)> ValidateTaskWriteAccessAsync(int taskId, int userId, string userRole, int userDeptId);
        Task<List<TaskResponseDto>> GetTasksByProjectAsync(int projectId);
        Task<List<TaskResponseDto>> GetTasksByUserAsync(int userId);
        Task<List<TaskResponseDto>> GetTasksByDepartmentAsync(int deptId);
        Task<string?> GetTaskFolderPathAsync(int taskId);
        string GetTaskFolderPath(string projName, string? deptName);
        Task<List<TaskResponseDto>> GetTasksByProjectTeamsAsync(int userId, string userRole);
    }
}
