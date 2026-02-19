using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;

namespace NPI.Server.Services
{
    public class ProjectTeamService : IProjectTeamService
    {
        private readonly ApplicationDbContext _context;

        public ProjectTeamService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectTeamDto>> GetAllProjectTeamsAsync()
        {
            try
            {
                var teams = await _context.ProjectTeams
                    .Include(pt => pt.User)
                    .Include(pt => pt.Project)
                    .ToListAsync();

                return teams.Select(pt => new ProjectTeamDto
                {
                    team_id = pt.team_id,
                    proj_id = pt.proj_id,
                    user_id = pt.user_id,
                    role = pt.role ?? "Team Member",
                    user_name = pt.User?.username ?? "Unknown",
                    proj_name = pt.Project?.proj_name ?? "Unknown"
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllProjectTeamsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ProjectTeamDto>> GetTeamsByProjectAsync(int projId)
        {
            try
            {
                var teams = await _context.ProjectTeams
                    .Where(pt => pt.proj_id == projId)
                    .Include(pt => pt.User)
                    .ToListAsync();

                return teams.Select(pt => new ProjectTeamDto
                {
                    team_id = pt.team_id,
                    proj_id = pt.proj_id,
                    user_id = pt.user_id,
                    role = pt.role ?? "Team Member",
                    user_name = pt.User?.username ?? "Unknown"
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTeamsByProjectAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<int>> GetProjectsByUserAsync(int userId)
        {
            try
            {
                return await _context.ProjectTeams
                    .Where(pt => pt.user_id == userId)
                    .Select(pt => pt.proj_id)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProjectsByUserAsync: {ex.Message}");
                throw;
            }
        }
    }
}