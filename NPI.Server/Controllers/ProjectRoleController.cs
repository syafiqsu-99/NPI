using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Services;
using System.Security.Claims;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectRoleController : ControllerBase
    {
        private readonly IProjectRoleService _projectRoleService;

        public ProjectRoleController(IProjectRoleService projectRoleService)
        {
            _projectRoleService = projectRoleService;
        }

        /// <summary>
        /// Get all project-scoped roles for a project.
        /// Accessible by Team Lead or above.
        /// </summary>
        [HttpGet("{projectId}/roles")]
        public async Task<IActionResult> GetProjectRoles(int projectId)
        {
            var hasAccess = await RbacHelper.HasProjectAccess(
                User, projectId, "Team Lead", _projectRoleService);

            if (!hasAccess)
                return Forbid();

            var roles = await _projectRoleService.GetProjectRolesAsync(projectId);
            return Ok(new { success = true, data = roles });
        }

        /// <summary>
        /// Returns the current user's project role.
        /// Used by the frontend auth store to cache permissions.
        /// </summary>
        [HttpGet("{projectId}/my-role")]
        public async Task<IActionResult> GetMyRole(int projectId)
        {
            var userId = RbacHelper.GetUserId(User);
            if (userId == 0)
                return Unauthorized();

            // Admin / Manager always get Team Lead
            var systemRole = RbacHelper.GetSystemRole(User);
            if (systemRole is "Admin" or "Manager")
                return Ok(new { success = true, data = new { role_name = "Team Lead" } });

            var role = await _projectRoleService.GetProjectRoleAsync(projectId, userId);
            return Ok(new { success = true, data = new { role_name = role ?? "Viewer" } });
        }

        /// <summary>
        /// Assign or update a user's project-scoped role.
        /// Only Team Lead (and above) on the project can do this.
        /// </summary>
        [HttpPost("{projectId}/roles")]
        public async Task<IActionResult> UpsertRole(int projectId, [FromBody] UpsertProjectRoleDto dto)
        {
            var hasAccess = await RbacHelper.HasProjectAccess(
                User, projectId, "Team Lead", _projectRoleService);

            if (!hasAccess)
                return Forbid();

            var assignedBy = RbacHelper.GetUserId(User);
            var (success, message) = await _projectRoleService
                .UpsertProjectRoleAsync(projectId, dto.user_id, dto.role_name, assignedBy);

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }
    }
}