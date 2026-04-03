using Microsoft.AspNetCore.Mvc;
using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetAllTasksAsync();
        Task<TaskResponseDto?> GetTaskByIdAsync(int taskId);
        Task<(bool success, string message, int taskId)> CreateTaskAsync(CreateTaskDto dto, int userId);
        Task<(bool success, string message)> UpdateTaskAsync(int taskId, UpdateTaskDto dto, int userId);
        Task<(bool success, string message)> DeleteTaskAsync(int taskId);
        Task<(bool success, string message)> UpdateTaskStatusAsync(int taskId, string status);
        Task<(bool success, string message)> UpdateTaskProgressAsync(int taskId, float progress);
        Task<(bool success, string message)> UpdatePlannedDatesAsync(int taskId, DateOnly newStart, DateOnly newEnd, string note);
        Task<List<TaskResponseDto>> GetTasksByProjectAsync(int projectId);
        Task<List<TaskResponseDto>> GetTasksByUserAsync(int userId);
        Task<List<TaskResponseDto>> GetTasksByDepartmentAsync(int deptId);
        Task<string?> GetTaskFolderPathAsync(int taskId);
        string GetTaskFolderPath(string projName, string? deptName);
        Task<List<TaskResponseDto>> GetTasksByProjectTeamsAsync(int userId, string userRole);
    }
}
