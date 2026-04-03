using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPI.Server.DTOs;
using NPI.Server.Services;
using System.Security.Claims;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GanttController : ControllerBase
    {
        private readonly IGanttService _ganttService;

        public GanttController(IGanttService ganttService)
        {
            _ganttService = ganttService;
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetGanttData(int projectId, [FromQuery] int? revisionId = null)
        {
            var data = await _ganttService.GetGanttDataAsync(projectId, revisionId);

            if (data == null)
            {
                return NotFound(new { success = false, message = "Project not found" });
            }

            return Ok(new { success = true, data });
        }

        [HttpGet("{projectId}/revisions")]
        public async Task<IActionResult> GetRevisions(int projectId)
        {
            var revisions = await _ganttService.GetProjectRevisionsAsync(projectId);
            return Ok(new { success = true, data = revisions });
        }

        [HttpPost("{projectId}/revisions")]
        public async Task<IActionResult> CreateRevision(int projectId, [FromBody] CreateRevisionDto dto)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user identity claim." });
            }
            var (success, message, revisionId) = await _ganttService.CreateRevisionAsync(projectId, dto, userId);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message, data = new { revisionId } });
        }
    }
}
