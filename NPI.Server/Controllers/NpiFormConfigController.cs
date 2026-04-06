// NPI.Server/Controllers/NpiFormConfigController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Models;
using System.Text.Json;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NpiFormConfigController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NpiFormConfigController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Public endpoint — all authenticated users fetch the form config for EnquiryForm.vue
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFormConfig()
        {
            var categories = await _context.NpiCategories
                .Where(c => c.is_active)
                .OrderBy(c => c.display_order)
                .ThenBy(c => c.category_name)
                .Select(c => new NpiCategoryDto
                {
                    category_id = c.category_id,
                    category_name = c.category_name,
                    display_order = c.display_order,
                    is_active = c.is_active
                })
                .ToListAsync();

            var sections = await _context.NpiFormSections
                .Where(s => s.is_active)
                .Include(s => s.Fields!.Where(f => f.is_active).OrderBy(f => f.display_order))
                .OrderBy(s => s.display_order)
                .ToListAsync();

            var sectionDtos = sections.Select(s => new NpiFormSectionDto
            {
                section_id = s.section_id,
                section_key = s.section_key,
                section_label = s.section_label,
                trigger_keywords = s.trigger_keywords,
                fields = (s.Fields ?? new List<NpiFormField>()).Select(f => MapField(f)).ToList()
            }).ToList();

            return Ok(new
            {
                success = true,
                data = new NpiFormConfigResponseDto
                {
                    categories = categories,
                    sections = sectionDtos
                }
            });
        }

        // ── Categories (Admin/NPI Team only) ────────────────────────────────

        [HttpGet("categories")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> GetAllCategories()
        {
            var cats = await _context.NpiCategories
                .OrderBy(c => c.display_order).ThenBy(c => c.category_name)
                .Select(c => new NpiCategoryDto
                {
                    category_id = c.category_id,
                    category_name = c.category_name,
                    display_order = c.display_order,
                    is_active = c.is_active
                })
                .ToListAsync();

            return Ok(new { success = true, data = cats });
        }

        [HttpPost("categories")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> CreateCategory([FromBody] UpsertNpiCategoryDto dto)
        {
            var exists = await _context.NpiCategories
                .AnyAsync(c => c.category_name == dto.category_name);

            if (exists)
                return BadRequest(new { success = false, message = "Category already exists" });

            var cat = new NpiCategory
            {
                category_name = dto.category_name,
                display_order = dto.display_order,
                is_active = dto.is_active,
                created_at = DateTime.Now
            };

            _context.NpiCategories.Add(cat);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Category created", data = new { cat.category_id } });
        }

        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpsertNpiCategoryDto dto)
        {
            var cat = await _context.NpiCategories.FindAsync(id);
            if (cat == null) return NotFound(new { success = false, message = "Category not found" });

            var duplicate = await _context.NpiCategories
                .AnyAsync(c => c.category_name == dto.category_name && c.category_id != id);

            if (duplicate)
                return BadRequest(new { success = false, message = "Category name already in use" });

            cat.category_name = dto.category_name;
            cat.display_order = dto.display_order;
            cat.is_active = dto.is_active;
            cat.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Category updated" });
        }

        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.NpiCategories.FindAsync(id);
            if (cat == null) return NotFound(new { success = false, message = "Category not found" });

            // Soft delete — enquiries reference category names as strings so hard delete is safe,
            // but soft delete preserves historical data integrity.
            cat.is_active = false;
            cat.updated_at = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Category deactivated" });
        }

        // ── Form Fields (Admin/NPI Team only) ───────────────────────────────

        [HttpGet("fields")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> GetAllFields()
        {
            var fields = await _context.NpiFormFields
                .Include(f => f.Section)
                .OrderBy(f => f.Section!.display_order)
                .ThenBy(f => f.display_order)
                .ToListAsync();

            return Ok(new { success = true, data = fields.Select(MapField).ToList() });
        }

        [HttpPost("fields")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> CreateField([FromBody] UpsertNpiFormFieldDto dto)
        {
            var sectionExists = await _context.NpiFormSections.AnyAsync(s => s.section_id == dto.section_id);
            if (!sectionExists)
                return BadRequest(new { success = false, message = "Section not found" });

            var field = new NpiFormField
            {
                section_id = dto.section_id,
                field_key = dto.field_key,
                field_label = dto.field_label,
                field_type = dto.field_type,
                options = dto.options != null ? JsonSerializer.Serialize(dto.options) : null,
                is_required = dto.is_required,
                is_active = dto.is_active,
                display_order = dto.display_order
            };

            _context.NpiFormFields.Add(field);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Field created", data = new { field.field_id } });
        }

        [HttpPut("fields/{id}")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> UpdateField(int id, [FromBody] UpsertNpiFormFieldDto dto)
        {
            var field = await _context.NpiFormFields.FindAsync(id);
            if (field == null) return NotFound(new { success = false, message = "Field not found" });

            field.section_id = dto.section_id;
            field.field_key = dto.field_key;
            field.field_label = dto.field_label;
            field.field_type = dto.field_type;
            field.options = dto.options != null ? JsonSerializer.Serialize(dto.options) : null;
            field.is_required = dto.is_required;
            field.is_active = dto.is_active;
            field.display_order = dto.display_order;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Field updated" });
        }

        [HttpDelete("fields/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteField(int id)
        {
            var field = await _context.NpiFormFields.FindAsync(id);
            if (field == null) return NotFound(new { success = false, message = "Field not found" });

            field.is_active = false;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Field deactivated" });
        }

        private static NpiFormFieldDto MapField(NpiFormField f) => new()
        {
            field_id = f.field_id,
            section_id = f.section_id,
            section_key = f.Section?.section_key ?? string.Empty,
            section_label = f.Section?.section_label ?? string.Empty,
            field_key = f.field_key,
            field_label = f.field_label,
            field_type = f.field_type,
            options = f.options != null
                ? JsonSerializer.Deserialize<List<string>>(f.options)
                : null,
            is_required = f.is_required,
            is_active = f.is_active,
            display_order = f.display_order
        };
    }
}