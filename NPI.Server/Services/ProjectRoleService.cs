using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public class ProjectRoleService : IProjectRoleService
    {
        private readonly ApplicationDbContext _context;

        private static readonly List<string> RoleHierarchy =
            ["Team Lead", "Member", "Viewer"];

        public ProjectRoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetProjectRoleAsync(int projectId, int userId)
        {
            // Check ProjectRoles table first (explicit assignment)
            var projectRole = await _context.ProjectTeams
                .AsNoTracking()
                .Where(pr => pr.proj_id == projectId && pr.user_id == userId)
                .Select(pr => pr.role)
                .FirstOrDefaultAsync();

            if (projectRole != null)
                return projectRole;

            var isTeamMember = await _context.ProjectTeams
                .AnyAsync(pt => pt.proj_id == projectId && pt.user_id == userId);

            return isTeamMember ? "Member" : null;
        }

        public async Task<bool> HasProjectRoleAsync(
            int projectId, int userId, string minimumRole)
        {
            var systemRole = await _context.Users
                .AsNoTracking()
                .Where(u => u.user_id == userId)
                .Select(u => u.Role!.role_name)
                .FirstOrDefaultAsync();

            // System Admins and Managers always pass
            if (systemRole is "Admin" or "Manager")
                return true;

            var projectRole = await GetProjectRoleAsync(projectId, userId);
            if (projectRole is null)
                return false;

            var userIdx = RoleHierarchy.IndexOf(projectRole);
            var minIdx = RoleHierarchy.IndexOf(minimumRole);

            return userIdx >= 0 && minIdx >= 0 && userIdx <= minIdx;
        }

        public async Task<(bool success, string message)> UpsertProjectRoleAsync(
            int projectId, int userId, string roleName, int assignedBy)
        {
            if (!RoleHierarchy.Contains(roleName))
                return (false,
                    $"Invalid project role '{roleName}'. Valid: {string.Join(", ", RoleHierarchy)}");

            var existing = await _context.ProjectTeams
                .FirstOrDefaultAsync(pr => pr.proj_id == projectId);

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
                .Include(pr => pr.User)
                .Where(pr => pr.proj_id == projectId)
                .Select(pr => new ProjectTeamDto
                {
                    team_id = pr.team_id,
                    proj_id = pr.proj_id,
                    user_id = pr.user_id,
                    user_name = pr.User!.username,
                    role = pr.role,
                    created_at = pr.created_at
                })
                .ToListAsync();
        }
    }
}