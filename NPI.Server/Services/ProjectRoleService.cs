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
            var projectRole = await _context.ProjectRoles
                .AsNoTracking()
                .Where(pr => pr.proj_id == projectId && pr.user_id == userId && pr.is_active)
                .Select(pr => pr.role_name)
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

            var existing = await _context.ProjectRoles
                .FirstOrDefaultAsync(pr => pr.proj_id == projectId && pr.user_id == userId);

            if (existing is null)
            {
                _context.ProjectRoles.Add(new ProjectRoles
                {
                    proj_id = projectId,
                    user_id = userId,
                    role_name = roleName,
                    is_active = true,
                    created_at = DateTime.Now
                });
            }
            else
            {
                existing.role_name = roleName;
                existing.is_active = true;
                existing.updated_at = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return (true, $"Project role '{roleName}' assigned.");
        }

        public async Task<List<ProjectRoleDto>> GetProjectRolesAsync(int projectId)
        {
            return await _context.ProjectRoles
                .AsNoTracking()
                .Include(pr => pr.User)
                .Where(pr => pr.proj_id == projectId && pr.is_active)
                .Select(pr => new ProjectRoleDto
                {
                    proj_role_id = pr.proj_role_id,
                    proj_id = pr.proj_id,
                    user_id = pr.user_id,
                    username = pr.User!.username,
                    full_name = pr.User.full_name,
                    role_name = pr.role_name,
                    created_at = pr.created_at
                })
                .ToListAsync();
        }
    }
}