using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public class ProjectRoleService : IProjectRoleService
    {
        private readonly ApplicationDbContext _context;

        private static readonly List<string> RoleHierarchy =
            [ProjectRoleNames.TeamLead, ProjectRoleNames.Member, ProjectRoleNames.Viewer];

        public ProjectRoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetProjectRoleAsync(int projectId, int userId)
        {
            var role = await _context.ProjectTeams
                .AsNoTracking()
                .Where(pt => pt.proj_id == projectId && pt.user_id == userId)
                .Select(pt => pt.role)
                .FirstOrDefaultAsync();

            if (role is not null)
                return role;

            var isTeamMember = await _context.ProjectTeams
                .AsNoTracking()
                .AnyAsync(pt => pt.proj_id == projectId && pt.user_id == userId);

            return isTeamMember ? ProjectRoleNames.Member : null;
        }

        public async Task<bool> HasProjectRoleAsync(int projectId, int userId, string systemRole, string minimumRole)
        {
            if (RbacHelper.IsAdminOrManager(systemRole))
                return true;

            var projectRole = await GetProjectRoleAsync(projectId, userId);
            if (projectRole is null)
                return false;

            var userIdx = RoleHierarchy.IndexOf(projectRole);
            var minIdx = RoleHierarchy.IndexOf(minimumRole);

            return userIdx >= 0 && minIdx >= 0 && userIdx <= minIdx;
        }

        public async Task<(bool success, string message)> UpsertProjectRoleAsync(int projectId, int userId, string roleName, int assignedBy)
        {
            if (!RoleHierarchy.Contains(roleName))
                return (false,
                    $"Invalid project role '{roleName}'. Valid: {string.Join(", ", RoleHierarchy)}");

            var existing = await _context.ProjectTeams
                .FirstOrDefaultAsync(pt => pt.proj_id == projectId && pt.user_id == userId);

            if (existing is null)
            {
                _context.ProjectTeams.Add(new ProjectTeams
                {
                    proj_id = projectId,
                    user_id = userId,
                    role = roleName,
                    created_at = DateTime.Now
                });
            }
            else
            {
                existing.role = roleName;
            }

            await _context.SaveChangesAsync();
            return (true, $"Project role '{roleName}' assigned.");
        }

        public async Task<List<ProjectTeamDto>> GetProjectRolesAsync(int projectId)
        {
            return await _context.ProjectTeams
                .AsNoTracking()
                .Include(pt => pt.User)
                    .ThenInclude(u => u!.Department)
                .Where(pt => pt.proj_id == projectId)
                .Select(pt => new ProjectTeamDto
                {
                    team_id = pt.team_id,
                    proj_id = pt.proj_id,
                    user_id = pt.user_id,
                    user_name = pt.User!.username,
                    dept_name = pt.User!.Department != null ? pt.User.Department.dept_name : null,
                    role = pt.role ?? ProjectRoleNames.Member,
                    created_at = pt.created_at
                })
                .ToListAsync();
        }

        public async Task<List<ProjectTeamDto>> GetAllProjectTeamsAsync()
        {
            return await _context.ProjectTeams
                .AsNoTracking()
                .Include(pt => pt.User)
                .Include(pt => pt.Project)
                .Select(pt => new ProjectTeamDto
                {
                    team_id = pt.team_id,
                    proj_id = pt.proj_id,
                    user_id = pt.user_id,
                    role = pt.role ?? ProjectRoleNames.Member,
                    user_name = pt.User != null ? pt.User.username : "Unknown",
                    proj_name = pt.Project != null ? pt.Project.proj_name : "Unknown"
                })
                .ToListAsync();
        }

        public async Task<List<int>> GetProjectsByUserAsync(int userId)
        {
            return await _context.ProjectTeams
                .AsNoTracking()
                .Where(pt => pt.user_id == userId)
                .Select(pt => pt.proj_id)
                .Distinct()
                .ToListAsync();
        }
    }
}