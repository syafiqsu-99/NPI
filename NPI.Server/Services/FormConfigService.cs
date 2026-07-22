using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Models;
using System.Text.Json;

namespace NPI.Server.Services
{
    public interface IFormConfigService
    {
        Task<FormConfigResponseDto> GetFormConfigAsync();
        Task<List<FormCategoryDto>> GetAllCategoriesAsync();
        Task<(bool Success, string Message, int Id)> CreateCategoryAsync(UpsertFormCategoryDto dto);
        Task<(bool Success, string Message)> UpdateCategoryAsync(int id, UpsertFormCategoryDto dto);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
        Task<List<FormSectionDto>> GetAllSectionsAsync();
        Task<FormSectionDto?> GetSectionByIdAsync(int id);
        Task<(bool Success, string Message, int Id)> CreateSectionAsync(CreateFormSectionDto dto);
        Task<(bool Success, string Message)> UpdateSectionAsync(int id, UpdateFormSectionDto dto);
        Task<(bool Success, string Message)> DeleteSectionAsync(int id);
        Task<(bool Success, string Message)> ToggleSectionStatusAsync(int id);
        Task<(bool Success, string Message)> ReorderSectionsAsync(List<int> orderedIds);
        Task<List<FormFieldDto>> GetAllFieldsAsync();
        Task<(bool Success, string Message, int Id)> CreateFieldAsync(UpsertFormFieldDto dto);
        Task<(bool Success, string Message)> UpdateFieldAsync(int id, UpsertFormFieldDto dto);
        Task<(bool Success, string Message)> DeleteFieldAsync(int id);
    }

    public class FormConfigService : IFormConfigService
    {
        private readonly ApplicationDbContext _context;

        public FormConfigService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── Public config endpoint ────────────────────────────────────────────

        public async Task<FormConfigResponseDto> GetFormConfigAsync()
        {
            var categories = await _context.FormCategories
                .Where(c => c.is_active)
                .OrderBy(c => c.display_order)
                .ThenBy(c => c.category_name)
                .Select(c => new FormCategoryDto
                {
                    category_id = c.category_id,
                    category_name = c.category_name,
                    display_order = c.display_order,
                    is_active = c.is_active
                })
                .ToListAsync();

            var sections = await _context.FormSections
                .Where(s => s.is_active)
                .Include(s => s.Fields!.Where(f => f.is_active).OrderBy(f => f.display_order))
                .OrderBy(s => s.display_order)
                .ToListAsync();

            return new FormConfigResponseDto
            {
                categories = categories,
                sections = sections.Select(MapSectionToDto).ToList()
            };
        }

        // ── Categories ────────────────────────────────────────────────────────

        public async Task<List<FormCategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.FormCategories
                .OrderBy(c => c.display_order)
                .ThenBy(c => c.category_name)
                .Select(c => new FormCategoryDto
                {
                    category_id = c.category_id,
                    category_name = c.category_name,
                    display_order = c.display_order,
                    is_active = c.is_active
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, int Id)>
            CreateCategoryAsync(UpsertFormCategoryDto dto)
        {
            try
            {
                if (await _context.FormCategories.AnyAsync(c => c.category_name == dto.category_name))
                    return (false, "A category with this name already exists.", 0);

                var cat = new FormCategory
                {
                    category_name = dto.category_name,
                    display_order = dto.display_order,
                    is_active = dto.is_active,
                    created_at = DateTime.Now
                };

                _context.FormCategories.Add(cat);
                await _context.SaveChangesAsync();
                return (true, "Category created.", cat.category_id);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        public async Task<(bool Success, string Message)>
            UpdateCategoryAsync(int id, UpsertFormCategoryDto dto)
        {
            try
            {
                var cat = await _context.FormCategories.FindAsync(id);
                if (cat is null) return (false, "Category not found.");

                if (await _context.FormCategories
                        .AnyAsync(c => c.category_name == dto.category_name && c.category_id != id))
                    return (false, "Another category already has this name.");

                cat.category_name = dto.category_name;
                cat.display_order = dto.display_order;
                cat.is_active = dto.is_active;
                cat.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();
                return (true, "Category updated.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
        {
            try
            {
                var cat = await _context.FormCategories.FindAsync(id);
                if (cat is null) return (false, "Category not found.");

                cat.is_active = false;
                cat.updated_at = DateTime.Now;
                await _context.SaveChangesAsync();
                return (true, "Category deactivated.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // ── Sections ──────────────────────────────────────────────────────────

        public async Task<List<FormSectionDto>> GetAllSectionsAsync()
        {
            var sections = await _context.FormSections
                .Include(s => s.Fields!.OrderBy(f => f.display_order))
                .OrderBy(s => s.display_order)
                .ToListAsync();

            return sections.Select(MapSectionToDto).ToList();
        }

        public async Task<FormSectionDto?> GetSectionByIdAsync(int id)
        {
            var section = await _context.FormSections
                .Include(s => s.Fields!.OrderBy(f => f.display_order))
                .FirstOrDefaultAsync(s => s.section_id == id);

            return section is null ? null : MapSectionToDto(section);
        }

        public async Task<(bool Success, string Message, int Id)>
            CreateSectionAsync(CreateFormSectionDto dto)
        {
            try
            {
                if (await _context.FormSections
                        .AnyAsync(s => s.section_key == dto.section_key))
                    return (false, $"Section key '{dto.section_key}' already exists.", 0);

                var section = new FormSection
                {
                    section_key = dto.section_key,
                    section_label = dto.section_label,
                    trigger_keywords = dto.trigger_keywords,
                    display_order = dto.display_order,
                    is_active = dto.is_active
                };

                _context.FormSections.Add(section);
                await _context.SaveChangesAsync();
                return (true, "Section created.", section.section_id);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        public async Task<(bool Success, string Message)>
            UpdateSectionAsync(int id, UpdateFormSectionDto dto)
        {
            try
            {
                var section = await _context.FormSections.FindAsync(id);
                if (section is null) return (false, "Section not found.");

                // section_key is immutable once created — changing it would
                // orphan existing EnquiryFieldValues rows.
                section.section_label = dto.section_label;
                section.trigger_keywords = dto.trigger_keywords;
                section.display_order = dto.display_order;
                section.is_active = dto.is_active;
                section.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();
                return (true, "Section updated.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteSectionAsync(int id)
        {
            try
            {
                var section = await _context.FormSections
                    .Include(s => s.Fields)
                    .FirstOrDefaultAsync(s => s.section_id == id);

                if (section is null) return (false, "Section not found.");

                // Guard: refuse hard delete if any enquiry has used this section
                var sectionKey = section.section_key;
                bool hasAnswers = await _context.EnquiryFieldValues
                    .AnyAsync(v => v.section_key == sectionKey);

                if (hasAnswers)
                {
                    // Soft delete — historical answers are preserved
                    section.is_active = false;
                    section.updated_at = DateTime.Now;
                    if (section.Fields is not null)
                        foreach (var f in section.Fields) f.is_active = false;

                    await _context.SaveChangesAsync();
                    return (true,
                        "Section deactivated (existing enquiry data preserved). " +
                        "Hard delete is not available once answers have been recorded.");
                }

                // Safe to hard delete — no answers exist
                _context.FormSections.Remove(section);
                await _context.SaveChangesAsync();
                return (true, "Section deleted.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ToggleSectionStatusAsync(int id)
        {
            try
            {
                var section = await _context.FormSections.FindAsync(id);
                if (section is null) return (false, "Section not found.");

                section.is_active = !section.is_active;
                section.updated_at = DateTime.Now;
                await _context.SaveChangesAsync();

                return (true, section.is_active ? "Section activated." : "Section deactivated.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates display_order for a batch of sections in one transaction.
        /// orderedIds: section IDs in the desired display order (index 0 = order 1).
        /// </summary>
        public async Task<(bool Success, string Message)>
            ReorderSectionsAsync(List<int> orderedIds)
        {
            try
            {
                var sections = await _context.FormSections
                    .Where(s => orderedIds.Contains(s.section_id))
                    .ToListAsync();

                for (int i = 0; i < orderedIds.Count; i++)
                {
                    var section = sections.FirstOrDefault(s => s.section_id == orderedIds[i]);
                    if (section is not null)
                    {
                        section.display_order = i + 1;
                        section.updated_at = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return (true, "Sections reordered.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // ── Fields ────────────────────────────────────────────────────────────

        public async Task<List<FormFieldDto>> GetAllFieldsAsync()
        {
            var fields = await _context.FormFields
                .Include(f => f.Section)
                .OrderBy(f => f.Section!.display_order)
                .ThenBy(f => f.display_order)
                .ToListAsync();

            return fields.Select(MapFieldToDto).ToList();
        }

        public async Task<(bool Success, string Message, int Id)>
            CreateFieldAsync(UpsertFormFieldDto dto)
        {
            try
            {
                if (!await _context.FormSections.AnyAsync(s => s.section_id == dto.section_id))
                    return (false, "Section not found.", 0);

                var field = new FormField
                {
                    section_id = dto.section_id,
                    field_key = dto.field_key,
                    field_label = dto.field_label,
                    field_type = dto.field_type,
                    options = dto.options is not null
                                      ? JsonSerializer.Serialize(dto.options)
                                      : null,
                    is_required = dto.is_required,
                    is_active = dto.is_active,
                    display_order = dto.display_order
                };

                _context.FormFields.Add(field);
                await _context.SaveChangesAsync();
                return (true, "Field created.", field.field_id);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        public async Task<(bool Success, string Message)>
            UpdateFieldAsync(int id, UpsertFormFieldDto dto)
        {
            try
            {
                var field = await _context.FormFields.FindAsync(id);
                if (field is null) return (false, "Field not found.");

                field.section_id = dto.section_id;
                field.field_label = dto.field_label;
                field.field_type = dto.field_type;
                field.options = dto.options is not null
                                        ? JsonSerializer.Serialize(dto.options)
                                        : null;
                field.is_required = dto.is_required;
                field.is_active = dto.is_active;
                field.display_order = dto.display_order;

                await _context.SaveChangesAsync();
                return (true, "Field updated.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteFieldAsync(int id)
        {
            try
            {
                var field = await _context.FormFields
                    .Include(f => f.Section)
                    .FirstOrDefaultAsync(f => f.field_id == id);

                if (field is null) return (false, "Field not found.");

                bool hasAnswers = await _context.EnquiryFieldValues
                    .AnyAsync(v => v.field_key == field.field_key
                                && v.section_key == field.Section!.section_key);

                if (hasAnswers)
                {
                    field.is_active = false;
                    await _context.SaveChangesAsync();
                    return (true,
                        "Field deactivated (existing answers preserved). " +
                        "Hard delete is not available once answers have been recorded.");
                }

                _context.FormFields.Remove(field);
                await _context.SaveChangesAsync();
                return (true, "Field deleted.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // ── Mapping helpers ───────────────────────────────────────────────────

        private static FormSectionDto MapSectionToDto(FormSection s) => new()
        {
            section_id = s.section_id,
            section_key = s.section_key,
            section_label = s.section_label,
            trigger_keywords = s.trigger_keywords,
            display_order = s.display_order,
            is_active = s.is_active,
            created_at = s.created_at,
            updated_at = s.updated_at,
            fields = (s.Fields ?? Enumerable.Empty<FormField>())
                                   .Select(MapFieldToDto)
                                   .ToList()
        };

        private static FormFieldDto MapFieldToDto(FormField f) => new()
        {
            field_id = f.field_id,
            section_id = f.section_id,
            section_key = f.Section?.section_key ?? string.Empty,
            section_label = f.Section?.section_label ?? string.Empty,
            field_key = f.field_key,
            field_label = f.field_label,
            field_type = f.field_type,
            options = f.options is not null
                              ? JsonSerializer.Deserialize<List<string>>(f.options)
                              : null,
            is_required = f.is_required,
            is_active = f.is_active,
            display_order = f.display_order
        };
    }
}