using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPI.Server.DTOs;
using NPI.Server.Services;

namespace NPI.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class TaskTemplateController : ControllerBase
    {
        private readonly ITaskTemplateService _service;

        public TaskTemplateController(ITaskTemplateService service)
        {
            _service = service;
        }

        [HttpGet("stages")]
        [Authorize]
        public async Task<IActionResult> GetStages()
        {
            var stages = await _service.GetStagesAsync();
            return Ok(stages);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] bool include_inactive = false)
        {
            var templates = await _service.GetAllAsync(include_inactive);
            return Ok(templates);
        }

        [HttpGet("stage/{stageId}")]
        [Authorize]
        public async Task<IActionResult> GetByStage(string stageId, [FromQuery] bool include_inactive = false)
        {
            var templates = await _service.GetByStageAsync(stageId, include_inactive);
            return Ok(templates);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskTemplateDto dto)
        {
            var (success, message, template_id) = await _service.CreateAsync(dto);
            if (!success) return BadRequest(new { success, message });

            return Ok(new { success, message, template_id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskTemplateDto dto)
        {
            var (success, message) = await _service.UpdateAsync(id, dto);
            if (!success) return BadRequest(new { success, message });

            return Ok(new { success, message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _service.DeleteAsync(id);
            if (!success) return BadRequest(new { success, message });

            return Ok(new { success, message });
        }

        [HttpPut("stage/{stageId}/reorder")]
        public async Task<IActionResult> Reorder(string stageId, [FromBody] List<int> ordered_template_ids)
        {
            var (success, message) = await _service.ReorderAsync(stageId, ordered_template_ids);
            if (!success) return BadRequest(new { success, message });

            return Ok(new { success, message });
        }
    }
}