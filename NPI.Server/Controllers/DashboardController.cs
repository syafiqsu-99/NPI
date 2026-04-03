using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using System.Security.Claims;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;
        public DashboardController(ApplicationDbContext ctx) => _ctx = ctx;

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var projects = await _ctx.Projects.ToListAsync();
            var tasks = await _ctx.Tasks.ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    total_projects = projects.Count,
                    in_progress = projects.Count(p => p.status == "In Progress"),
                    on_hold = projects.Count(p => p.status == "On Hold"),
                    completed = projects.Count(p => p.status == "Completed"),
                    total_tasks = tasks.Count,
                    tasks_completed = tasks.Count(t => t.status == "Completed"),
                    tasks_overdue = tasks.Count(t =>
                        t.status != "Completed" &&
                        t.planned_end_date.HasValue &&
                        t.planned_end_date.Value < today),
                    avg_progress = tasks.Any()
                        ? (int)tasks.Average(t => t.per_complete ?? 0) : 0,
                    unread_notifications = await _ctx.Notifications
                        .CountAsync(n => n.user_id ==
                            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value) &&
                            n.read_at == null)
                }
            });
        }

        [HttpGet("overdue-tasks")]
        public async Task<IActionResult> GetOverdueTasks()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

            var query = _ctx.Tasks
                .Include(t => t.Project)
                .Include(t => t.Department)
                .Where(t => t.status != "Completed" &&
                            t.planned_end_date.HasValue &&
                            t.planned_end_date.Value < today);

            // Non-admin only sees their team's projects
            if (role != "Admin")
            {
                var myProjIds = await _ctx.ProjectTeams
                    .Where(pt => pt.user_id == userId)
                    .Select(pt => pt.proj_id)
                    .ToListAsync();
                query = query.Where(t => myProjIds.Contains(t.proj_id));
            }

            var results = await query
                .OrderBy(t => t.planned_end_date)
                .Take(20)
                .Select(t => new
                {
                    t.task_id,
                    t.task_code,
                    t.title,
                    t.stage_id,
                    t.status,
                    dept_name = t.Department!.dept_name,
                    proj_name = t.Project!.proj_name,
                    proj_id = t.proj_id,
                    planned_end_date = t.planned_end_date,
                    days_overdue = today.DayNumber - t.planned_end_date!.Value.DayNumber
                })
                .ToListAsync();

            return Ok(new { success = true, data = results });
        }

        [HttpGet("stage-distribution")]
        public async Task<IActionResult> GetStageDistribution()
        {
            var data = await _ctx.Tasks
                .Where(t => t.stage_id != null)
                .GroupBy(t => new { t.stage_id, t.status })
                .Select(g => new { g.Key.stage_id, g.Key.status, count = g.Count() })
                .ToListAsync();

            return Ok(new { success = true, data });
        }
    }
}
