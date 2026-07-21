using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IMilestoneService
    {
        Task<List<MilestoneResponseDto>> GetProjectMilestonesAsync(int projectId);
        Task<MilestoneResponseDto?> GetMilestoneByIdAsync(int milestoneId);
        Task<(bool success, string message, MilestoneResponseDto? milestone)> CreateMilestoneAsync(int projectId, CreateMilestoneDto dto);
        Task<(bool success, string message)> UpdateMilestoneAsync(int milestoneId, int projectId, UpdateMilestoneDto dto);
        Task<(bool success, string message)> DeleteMilestoneAsync(int milestoneId, int projectId);
        Task<(bool success, string message)> CompleteMilestoneAsync(int milestoneId, int projectId);
    }

    public class MilestoneService : IMilestoneService
    {
        private readonly ApplicationDbContext _context;

        public MilestoneService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MilestoneResponseDto>> GetProjectMilestonesAsync(int projectId)
        {
            var milestones = await _context.Milestones
                .Include(m => m.ResponsibleDepartment)
                .Where(m => m.proj_id == projectId)
                .OrderBy(m => m.planned_date)
                .ToListAsync();

            return milestones.Select(m => MapToResponseDto(m)).ToList();
        }

        public async Task<MilestoneResponseDto?> GetMilestoneByIdAsync(int milestoneId)
        {
            var milestone = await _context.Milestones
                .Include(m => m.ResponsibleDepartment)
                .FirstOrDefaultAsync(m => m.milestone_id == milestoneId);

            if (milestone == null)
                return null;

            return MapToResponseDto(milestone);
        }

        public async Task<(bool success, string message, MilestoneResponseDto? milestone)> CreateMilestoneAsync(
            int projectId, CreateMilestoneDto dto)
        {
            try
            {
                // Validate project exists
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return (false, "Project not found", null);

                // Validate department if provided
                if (dto.dept_id.HasValue)
                {
                    var department = await _context.Departments.FindAsync(dto.dept_id.Value);
                    if (department == null)
                        return (false, "Department not found", null);
                }

                var milestone = new Milestones
                {
                    proj_id = projectId,
                    milestone_name = dto.milestone_name,
                    description = dto.description,
                    planned_date = dto.planned_date,
                    status = "Pending",
                    responsible_dept_id = dto.dept_id,
                    created_at = DateTime.Now
                };

                _context.Milestones.Add(milestone);
                await _context.SaveChangesAsync();

                // Reload with department info
                milestone = await _context.Milestones
                    .Include(m => m.ResponsibleDepartment)
                    .FirstOrDefaultAsync(m => m.milestone_id == milestone.milestone_id);

                return (true, "Milestone created successfully", MapToResponseDto(milestone!));
            }
            catch (Exception ex)
            {
                return (false, $"Error creating milestone: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> UpdateMilestoneAsync(
            int milestoneId, int projectId, UpdateMilestoneDto dto)
        {
            try
            {
                var milestone = await _context.Milestones.FindAsync(milestoneId);

                if (milestone == null)
                    return (false, "Milestone not found");

                // Validate milestone belongs to project
                if (milestone.proj_id != projectId)
                    return (false, "Milestone does not belong to this project");

                // Update fields
                if (!string.IsNullOrEmpty(dto.milestone_name))
                    milestone.milestone_name = dto.milestone_name;

                if (dto.planned_date.HasValue)
                    milestone.planned_date = dto.planned_date.Value;

                if (dto.responsible_dept_id.HasValue)
                {
                    var department = await _context.Departments.FindAsync(dto.responsible_dept_id.Value);
                    if (department == null)
                        return (false, "Department not found");

                    milestone.responsible_dept_id = dto.responsible_dept_id.Value;
                }

                if (dto.actual_date.HasValue)
                {
                    milestone.actual_date = dto.actual_date;
                    milestone.status = TasksStatus.Completed;
                }

                await _context.SaveChangesAsync();

                return (true, "Milestone updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating milestone: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteMilestoneAsync(
            int milestoneId, int projectId)
        {
            try
            {
                var milestone = await _context.Milestones.FindAsync(milestoneId);

                if (milestone == null)
                    return (false, "Milestone not found");

                // Validate milestone belongs to project
                if (milestone.proj_id != projectId)
                    return (false, "Milestone does not belong to this project");

                _context.Milestones.Remove(milestone);
                await _context.SaveChangesAsync();

                return (true, "Milestone deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting milestone: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> CompleteMilestoneAsync(
            int milestoneId, int projectId)
        {
            try
            {
                var milestone = await _context.Milestones.FindAsync(milestoneId);

                if (milestone == null)
                    return (false, "Milestone not found");

                // Validate milestone belongs to project
                if (milestone.proj_id != projectId)
                    return (false, "Milestone does not belong to this project");

                if (milestone.actual_date.HasValue)
                    return (false, "Milestone already completed");

                milestone.actual_date = DateOnly.FromDateTime(DateTime.Now);
                milestone.status = TasksStatus.Completed;

                await _context.SaveChangesAsync();

                return (true, "Milestone marked as complete");
            }
            catch (Exception ex)
            {
                return (false, $"Error completing milestone: {ex.Message}");
            }
        }

        // Helper method to reduce code duplication
        private MilestoneResponseDto MapToResponseDto(Milestones milestone)
        {
            return new MilestoneResponseDto
            {
                milestone_id = milestone.milestone_id,
                proj_id = milestone.proj_id,
                milestone_name = milestone.milestone_name,
                description = milestone.description,
                planned_date = milestone.planned_date,
                actual_date = milestone.actual_date,
                status = milestone.status,
                responsible_dept_id = milestone.responsible_dept_id,
                dept_name = milestone.ResponsibleDepartment?.dept_name,
                is_completed = milestone.actual_date.HasValue,
                is_delayed = milestone.actual_date.HasValue &&
                            milestone.actual_date > milestone.planned_date,
                created_at = milestone.created_at
            };
        }
    }
}
