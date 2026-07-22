using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface ITaskTemplateService
    {
        Task<List<StageInfoDto>> GetStagesAsync();
        Task<List<TaskTemplateResponseDto>> GetAllAsync(bool includeInactive = false);
        Task<List<TaskTemplateResponseDto>> GetByStageAsync(string stageId, bool includeInactive = false);
        Task<(bool success, string message, int template_id)> CreateAsync(CreateTaskTemplateDto dto);
        Task<(bool success, string message)> UpdateAsync(int templateId, UpdateTaskTemplateDto dto);
        Task<(bool success, string message)> DeleteAsync(int templateId);
        Task<(bool success, string message)> ReorderAsync(string stageId, List<int> orderedTemplateIds);

    }

    public class TaskTemplateService : ITaskTemplateService
    {
        private readonly ApplicationDbContext _context;

        public TaskTemplateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StageInfoDto>> GetStagesAsync()
        {
            var counts = await _context.TaskTemplates
                .AsNoTracking()
                .Where(t => t.is_active)
                .GroupBy(t => t.stage_id)
                .Select(g => new { stage_id = g.Key, count = g.Count() })
                .ToListAsync();

            var countLookup = counts.ToDictionary(c => c.stage_id, c => c.count);

            return NpiStages.Names
                .Select(kv => new StageInfoDto
                {
                    stage_id = kv.Key,
                    stage_name = kv.Value,
                    is_required = NpiStages.Required.Contains(kv.Key),
                    auto_complete = NpiStages.AutoComplete.Contains(kv.Key),
                    task_count = countLookup.GetValueOrDefault(kv.Key, 0)
                })
                .OrderBy(s => s.stage_id)
                .ToList();
        }

        public async Task<List<TaskTemplateResponseDto>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.TaskTemplates
                .AsNoTracking()
                .Include(t => t.Department)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(t => t.is_active);

            var templates = await query
                .OrderBy(t => t.stage_id)
                .ThenBy(t => t.display_order)
                .ToListAsync();

            return templates.Select(MapToResponseDto).ToList();
        }

        public async Task<List<TaskTemplateResponseDto>> GetByStageAsync(string stageId, bool includeInactive = false)
        {
            if (!NpiStages.IsValid(stageId))
                return new List<TaskTemplateResponseDto>();

            var query = _context.TaskTemplates
                .AsNoTracking()
                .Include(t => t.Department)
                .Where(t => t.stage_id == stageId);

            if (!includeInactive)
                query = query.Where(t => t.is_active);

            var templates = await query
                .OrderBy(t => t.display_order)
                .ToListAsync();

            return templates.Select(MapToResponseDto).ToList();
        }

        public async Task<(bool success, string message, int template_id)> CreateAsync(CreateTaskTemplateDto dto)
        {
            try
            {
                if (!NpiStages.IsValid(dto.stage_id))
                    return (false, $"Invalid stage '{dto.stage_id}'.", 0);

                if (string.IsNullOrWhiteSpace(dto.title))
                    return (false, "Title is required.", 0);

                if (string.IsNullOrWhiteSpace(dto.task_code))
                    return (false, "Task code is required.", 0);

                var deptExists = await _context.Departments
                    .AnyAsync(d => d.dept_id == dto.dept_id);

                if (!deptExists)
                    return (false, "Department not found.", 0);

                var codeTaken = await _context.TaskTemplates
                    .AnyAsync(t => t.stage_id == dto.stage_id && t.task_code == dto.task_code);

                if (codeTaken)
                    return (false, $"Task code '{dto.task_code}' already exists in stage {dto.stage_id}.", 0);

                var order = dto.display_order > 0
                    ? dto.display_order
                    : await NextDisplayOrderAsync(dto.stage_id);

                var template = new TaskTemplates
                {
                    stage_id = dto.stage_id,
                    task_code = dto.task_code.Trim(),
                    title = dto.title.Trim(),
                    dept_id = dto.dept_id,
                    default_duration = dto.default_duration > 0 ? dto.default_duration : 5,
                    has_link = dto.has_link,
                    display_order = order,
                    is_active = true,
                    created_at = DateTime.Now
                };

                _context.TaskTemplates.Add(template);
                await _context.SaveChangesAsync();

                return (true, "Template task created.", template.template_id);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating template task: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> UpdateAsync(int templateId, UpdateTaskTemplateDto dto)
        {
            try
            {
                var template = await _context.TaskTemplates.FindAsync(templateId);
                if (template is null)
                    return (false, "Template task not found.");

                if (!string.IsNullOrWhiteSpace(dto.task_code) && dto.task_code != template.task_code)
                {
                    var codeTaken = await _context.TaskTemplates
                        .AnyAsync(t => t.stage_id == template.stage_id
                                    && t.task_code == dto.task_code
                                    && t.template_id != templateId);

                    if (codeTaken)
                        return (false, $"Task code '{dto.task_code}' already exists in stage {template.stage_id}.");

                    template.task_code = dto.task_code.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.title))
                    template.title = dto.title.Trim();

                if (dto.dept_id.HasValue)
                {
                    var deptExists = await _context.Departments
                        .AnyAsync(d => d.dept_id == dto.dept_id.Value);

                    if (!deptExists)
                        return (false, "Department not found.");

                    template.dept_id = dto.dept_id.Value;
                }

                if (dto.default_duration.HasValue && dto.default_duration.Value > 0)
                    template.default_duration = dto.default_duration.Value;

                if (dto.has_link.HasValue)
                    template.has_link = dto.has_link.Value;

                if (dto.display_order.HasValue)
                    template.display_order = dto.display_order.Value;

                if (dto.is_active.HasValue)
                    template.is_active = dto.is_active.Value;

                template.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();
                return (true, "Template task updated.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating template task: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DeleteAsync(int templateId)
        {
            try
            {
                var template = await _context.TaskTemplates.FindAsync(templateId);
                if (template is null)
                    return (false, "Template task not found.");

                _context.TaskTemplates.Remove(template);
                await _context.SaveChangesAsync();

                return (true, "Template task deleted.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting template task: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> ReorderAsync(string stageId, List<int> orderedTemplateIds)
        {
            try
            {
                if (!NpiStages.IsValid(stageId))
                    return (false, $"Invalid stage '{stageId}'.");

                var templates = await _context.TaskTemplates
                    .Where(t => t.stage_id == stageId)
                    .ToListAsync();

                var lookup = templates.ToDictionary(t => t.template_id);

                for (int i = 0; i < orderedTemplateIds.Count; i++)
                {
                    if (lookup.TryGetValue(orderedTemplateIds[i], out var template))
                    {
                        template.display_order = i + 1;
                        template.updated_at = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return (true, "Order updated.");
            }
            catch (Exception ex)
            {
                return (false, $"Error reordering: {ex.Message}");
            }
        }

        private async Task<int> NextDisplayOrderAsync(string stageId)
        {
            var max = await _context.TaskTemplates
                .Where(t => t.stage_id == stageId)
                .Select(t => (int?)t.display_order)
                .MaxAsync() ?? 0;

            return max + 1;
        }

        private static TaskTemplateResponseDto MapToResponseDto(TaskTemplates t) => new()
        {
            template_id = t.template_id,
            stage_id = t.stage_id,
            stage_name = NpiStages.Names.GetValueOrDefault(t.stage_id, t.stage_id),
            task_code = t.task_code,
            title = t.title,
            dept_id = t.dept_id,
            dept_name = t.Department?.dept_name,
            default_duration = t.default_duration,
            has_link = t.has_link,
            display_order = t.display_order,
            is_active = t.is_active
        };
    }
}