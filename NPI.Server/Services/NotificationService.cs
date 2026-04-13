using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Hubs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(ApplicationDbContext ctx, IHubContext<NotificationHub> hub)
        {
            _ctx = ctx;
            _hub = hub;
        }

        public async Task NotifyAsync(int userId, string type, string title, string body,
                                      int? projId = null, int? taskId = null)
        {
            var notif = new Notifications
            {
                user_id = userId,
                proj_id = projId,
                task_id = taskId,
                notif_type = type,
                subject = title,
                body = body,
                is_read = false,
                created_at = DateTime.Now
            };

            _ctx.Notifications.Add(notif);
            await _ctx.SaveChangesAsync();

            _ = _hub.Clients
                    .Group($"user_{userId}")
                    .SendAsync("NewNotification", new
                    {
                        notif.notif_id,
                        title,
                        type,
                        body,
                        proj_id = projId,
                        task_id = taskId,
                        created_at = notif.created_at
                    });
        }

        public async Task NotifyDepartmentAsync(int deptId, string type, string title,
                                                 string body, int? projId = null)
        {
            var userIds = await _ctx.Users
                .Where(u => u.dept_id == deptId && u.is_active)
                .Select(u => u.user_id)
                .ToListAsync();

            if (!userIds.Any()) return;

            var notifications = userIds.Select(uid => new Notifications
            {
                user_id = uid,
                proj_id = projId,
                notif_type = type,
                subject = title,
                body = body,
                is_read = false,
                created_at = DateTime.Now
            }).ToList();

            await _ctx.Notifications.AddRangeAsync(notifications);
            await _ctx.SaveChangesAsync();

            _ = _hub.Clients
                    .Group($"dept_{deptId}")
                    .SendAsync("NewNotification", new { title, type, body, proj_id = projId });
        }

        public async Task NotifyProjectTeamAsync(int projId, string type, string title,
                                                  string body, int? taskId = null,
                                                  int? excludeUserId = null)
        {
            var userIds = await _ctx.ProjectTeams
                .Where(pt => pt.proj_id == projId)
                .Select(pt => pt.user_id)
                .Distinct()
                .ToListAsync();

            if (!userIds.Any()) return;

            var now = DateTime.Now;
            var notifications = userIds
                .Where(uid => uid != excludeUserId)
                .Select(uid => new Notifications
                {
                    user_id = uid,
                    proj_id = projId,
                    task_id = taskId,
                    notif_type = type,
                    subject = title,
                    body = body,
                    is_read = false,
                    created_at = now
                }).ToList();

            if (notifications.Count == 0) return;

            await _ctx.Notifications.AddRangeAsync(notifications);
            await _ctx.SaveChangesAsync();

            _ = _hub.Clients
                    .Group($"project_{projId}")
                    .SendAsync("NewNotification", new
                    {
                        title,
                        type,
                        body,
                        proj_id = projId,
                        task_id = taskId
                    });
        }

        public async Task<List<NotificationDto>> GetUnreadAsync(int userId, int take = 50)
        {
            return await _ctx.Notifications
                .Where(n => n.user_id == userId && !n.is_read)
                .OrderByDescending(n => n.created_at)
                .Take(take)
                .Select(n => new NotificationDto
                {
                    notif_id = n.notif_id,
                    type = n.notif_type,
                    title = n.subject,
                    body = n.body,
                    is_read = n.is_read,
                    proj_id = n.proj_id,
                    task_id = n.task_id,
                    created_at = n.created_at
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _ctx.Notifications
                .CountAsync(n => n.user_id == userId && !n.is_read);
        }

        public async Task MarkReadAsync(int notifId, int userId)
        {
            var notif = await _ctx.Notifications
                .FirstOrDefaultAsync(n => n.notif_id == notifId && n.user_id == userId);

            if (notif is null) return;

            notif.is_read = true;
            notif.read_at = DateTime.Now;
            await _ctx.SaveChangesAsync();
        }

        public async Task MarkAllReadAsync(int userId)
        {
            await _ctx.Notifications
                .Where(n => n.user_id == userId && !n.is_read)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.is_read, true)
                    .SetProperty(n => n.read_at, DateTime.Now));
        }
    }
}