using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPI.Server.DTOs;
using NPI.Server.Services;

namespace NPI.Server.Controllers
{
    /// <summary>
    /// Provides:
    ///   GET  /api/NpiFormConfig            — public config (active only)
    ///   CRUD /api/NpiFormConfig/categories — Admin/NPI Team
    ///   CRUD /api/NpiFormConfig/sections   — Admin/NPI Team  ← NEW full CRUD
    ///   CRUD /api/NpiFormConfig/fields     — Admin/NPI Team
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NpiFormConfigController : ControllerBase
    {
        private readonly INpiFormConfigService _service;

        public NpiFormConfigController(INpiFormConfigService service)
        {
            _service = service;
        }

        // ── Public config (all authenticated users) ───────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetFormConfig()
        {
            var config = await _service.GetFormConfigAsync();
            return Ok(new { success = true, data = config });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CATEGORIES
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet("categories")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> GetAllCategories()
        {
            var cats = await _service.GetAllCategoriesAsync();
            return Ok(new { success = true, data = cats });
        }

        [HttpPost("categories")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> CreateCategory([FromBody] UpsertNpiCategoryDto dto)
        {
            var (success, message, id) = await _service.CreateCategoryAsync(dto);
            return success
                ? Ok(new { success = true, message, data = new { category_id = id } })
                : BadRequest(new { success = false, message });
        }

        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpsertNpiCategoryDto dto)
        {
            var (success, message) = await _service.UpdateCategoryAsync(id, dto);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var (success, message) = await _service.DeleteCategoryAsync(id);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SECTIONS  — full CRUD (previously read-only)
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet("sections")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> GetAllSections()
        {
            var sections = await _service.GetAllSectionsAsync();
            return Ok(new { success = true, data = sections });
        }

        [HttpGet("sections/{id}")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> GetSectionById(int id)
        {
            var section = await _service.GetSectionByIdAsync(id);
            return section is null
                ? NotFound(new { success = false, message = "Section not found." })
                : Ok(new { success = true, data = section });
        }

        [HttpPost("sections")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> CreateSection([FromBody] CreateNpiFormSectionDto dto)
        {
            var (success, message, id) = await _service.CreateSectionAsync(dto);
            return success
                ? Ok(new { success = true, message, data = new { section_id = id } })
                : BadRequest(new { success = false, message });
        }

        [HttpPut("sections/{id}")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> UpdateSection(int id, [FromBody] UpdateNpiFormSectionDto dto)
        {
            var (success, message) = await _service.UpdateSectionAsync(id, dto);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpDelete("sections/{id}")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var (success, message) = await _service.DeleteSectionAsync(id);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpPatch("sections/{id}/toggle-status")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> ToggleSectionStatus(int id)
        {
            var (success, message) = await _service.ToggleSectionStatusAsync(id);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpPatch("sections/reorder")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> ReorderSections([FromBody] List<int> orderedIds)
        {
            if (orderedIds is null || orderedIds.Count == 0)
                return BadRequest(new { success = false, message = "No IDs provided." });

            var (success, message) = await _service.ReorderSectionsAsync(orderedIds);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  FIELDS
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet("fields")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> GetAllFields()
        {
            var fields = await _service.GetAllFieldsAsync();
            return Ok(new { success = true, data = fields });
        }

        [HttpPost("fields")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> CreateField([FromBody] UpsertNpiFormFieldDto dto)
        {
            var (success, message, id) = await _service.CreateFieldAsync(dto);
            return success
                ? Ok(new { success = true, message, data = new { field_id = id } })
                : BadRequest(new { success = false, message });
        }

        [HttpPut("fields/{id}")]
        [Authorize(Roles = "Admin,NPI Team")]
        public async Task<IActionResult> UpdateField(int id, [FromBody] UpsertNpiFormFieldDto dto)
        {
            var (success, message) = await _service.UpdateFieldAsync(id, dto);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpDelete("fields/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteField(int id)
        {
            var (success, message) = await _service.DeleteFieldAsync(id);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }
    }
}