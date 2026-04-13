using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NPI.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace NPI.Server.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public NotificationHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await base.OnConnectedAsync();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            if (!int.TryParse(userId, out var uid))
            {
                await base.OnConnectedAsync();
                return;
            }

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.user_id == uid);

            if (user?.dept_id.HasValue == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"dept_{user.dept_id}");
            }

            var projectIds = await _context.ProjectTeams
                .AsNoTracking()
                .Where(pt => pt.user_id == uid)
                .Select(pt => pt.proj_id)
                .ToListAsync();

            foreach (var projId in projectIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

                if (int.TryParse(userId, out var uid))
                {
                    var user = await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.user_id == uid);

                    if (user?.dept_id.HasValue == true)
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"dept_{user.dept_id}");

                    var projectIds = await _context.ProjectTeams
                        .AsNoTracking()
                        .Where(pt => pt.user_id == uid)
                        .Select(pt => pt.proj_id)
                        .ToListAsync();

                    foreach (var projId in projectIds)
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projId}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinProjectGroup(int projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
        }

        public async Task LeaveProjectGroup(int projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
        }
    }
}