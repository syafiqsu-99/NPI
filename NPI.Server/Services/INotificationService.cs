using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(int userId, string type, string title, string body, int? projId = null, int? taskId = null);
        Task NotifyDepartmentAsync(int deptId, string type, string title, string body, int? projId = null);
        Task NotifyProjectTeamAsync(int projId, string type, string title, string body, int? taskId = null, int? excludeUserId = null);
        Task<List<NotificationDto>> GetUnreadAsync(int userId, int take = 50);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkReadAsync(int notifId, int userId);
        Task MarkAllReadAsync(int userId);
    }
}