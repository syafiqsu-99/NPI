using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPI.Server.DTOs;
using NPI.Server.Services;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MilestonesController : ControllerBase
    {
        private readonly IMilestoneService _milestoneService;

        public MilestonesController(IMilestoneService milestoneService)
        {
            _milestoneService = milestoneService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectMilestones(int projectId)
        {
            try
            {
                var milestones = await _milestoneService.GetProjectMilestonesAsync(projectId);
                return Ok(new { success = true, data = milestones });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{milestoneId}")]
        public async Task<IActionResult> GetMilestone(int projectId, int milestoneId)
        {
            try
            {
                var milestone = await _milestoneService.GetMilestoneByIdAsync(milestoneId);

                if (milestone == null)
                {
                    return NotFound(new { success = false, message = "Milestone not found" });
                }

                if (milestone.proj_id != projectId)
                {
                    return BadRequest(new { success = false, message = "Milestone does not belong to this project" });
                }

                return Ok(new { success = true, data = milestone });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMilestone(int projectId, [FromBody] CreateMilestoneDto dto)
        {
            try
            {
                var result = await _milestoneService.CreateMilestoneAsync(projectId, dto);

                if (result.success)
                {
                    return Ok(new { success = true, message = result.message, data = result.milestone });
                }

                return BadRequest(new { success = false, message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{milestoneId}")]
        public async Task<IActionResult> UpdateMilestone(int projectId, int milestoneId, [FromBody] UpdateMilestoneDto dto)
        {
            try
            {
                var result = await _milestoneService.UpdateMilestoneAsync(milestoneId, projectId, dto);

                if (result.success)
                {
                    return Ok(new { success = true, message = result.message });
                }

                return BadRequest(new { success = false, message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{milestoneId}")]
        public async Task<IActionResult> DeleteMilestone(int projectId, int milestoneId)
        {
            try
            {
                var result = await _milestoneService.DeleteMilestoneAsync(milestoneId, projectId);

                if (result.success)
                {
                    return Ok(new { success = true, message = result.message });
                }

                return BadRequest(new { success = false, message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{milestoneId}/complete")]
        public async Task<IActionResult> CompleteMilestone(int projectId, int milestoneId)
        {
            try
            {
                var result = await _milestoneService.CompleteMilestoneAsync(milestoneId, projectId);

                if (result.success)
                {
                    return Ok(new { success = true, message = result.message });
                }

                return BadRequest(new { success = false, message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
