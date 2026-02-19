using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPI.Server.Services;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectTeamController : ControllerBase
    {
        private readonly IProjectTeamService _service;

        public ProjectTeamController(IProjectTeamService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjectTeams()
        {
            var teams = await _service.GetAllProjectTeamsAsync();
            return Ok(new { success = true, data = teams });
        }
        
        [HttpGet("by-project/{projId}")]
        public async Task<IActionResult> GetTeamsByProject(int projId)
        {
            var teams = await _service.GetTeamsByProjectAsync(projId);
            return Ok(new { success = true, data = teams });
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetProjectsByUser(int userId)
        {
            var projects = await _service.GetProjectsByUserAsync(userId);
            return Ok(new { success = true, data = projects });
        }
    }
}