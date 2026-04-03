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
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IMilestoneService _milestoneService;

        public ProjectController(IProjectService projectService, IMilestoneService milestoneService)
        {
            _projectService = projectService;
            _milestoneService = milestoneService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();
                return Ok(new { success = true, data = projects });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new { success = false, message = "Project not found" });
                }
                return Ok(new { success = true, data = project });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                }
                var (success, message, projId) = await _projectService.CreateProjectAsync(dto, userId);

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new
                {
                    success = true,
                    message,
                    data = new { proj_id = projId }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                }
                var (success, message) = await _projectService.UpdateProjectAsync(id, dto, userId);

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var (success, message) = await _projectService.DeleteProjectAsync(id);

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("from-enquiry/{enquiryId}")]
        public async Task<IActionResult> CreateProjectFromEnquiry(int enquiryId, [FromBody] CreateProjectFromEnquiryDto? dto = null)
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                }
                var (success, message, proj_id) =
                    await _projectService.CreateProjectFromEnquiryAsync(enquiryId, userId, dto);

                if (!success)
                    return BadRequest(new { success = false, message });

                return Ok(new { success = true, message, data = new { proj_id } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> GetProjectTasks(int id)
        {
            try
            {
                var tasks = await _projectService.GetProjectTasksAsync(id);
                return Ok(new { success = true, data = tasks });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}/milestones")]
        public async Task<IActionResult> GetProjectMilestones(int id)
        {
            try
            {
                var milestones = await _projectService.GetProjectMilestonesAsync(id);
                return Ok(new { success = true, data = milestones });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/launch")]
        public async Task<IActionResult> LaunchProject(int id, [FromBody] LaunchProjectDto dto)
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                }
                var (success, message, folderWarnings) = await _projectService.LaunchProjectAsync(id, dto, userId);

                if (!success)
                    return BadRequest(new { success = false, message });

                return Ok(new
                {
                    success = true,
                    message,
                    folder_warnings = folderWarnings ?? new List<string>()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateProjectStatus(int id, [FromBody] UpdateProjectStatusDto dto)
        {
            try
            {
                var (success, message) = await _projectService.UpdateProjectStatusAsync(id, dto.status);

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetProjectsByStatus(string status)
        {
            try
            {
                var projects = await _projectService.GetProjectsByStatusAsync(status);
                return Ok(new { success = true, data = projects });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-department/{deptId}")]
        public async Task<IActionResult> GetProjectsByDepartment(int deptId)
        {
            try
            {
                var projects = await _projectService.GetProjectsByDepartmentAsync(deptId);
                return Ok(new { success = true, data = projects });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-customer/{customerId}")]
        public async Task<IActionResult> GetProjectsByCustomer(int customerId)
        {
            try
            {
                var projects = await _projectService.GetProjectsByCustomerAsync(customerId);
                return Ok(new { success = true, data = projects });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
