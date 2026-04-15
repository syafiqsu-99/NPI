using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Services;
using System.Security.Claims;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public UserController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
                var users = await _userService.GetAllUsersAsync();

                if (!isAdmin)
                    users = users.Where(u => u.role_name != "Admin").ToList();

                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found" });

                if (!User.IsInRole("Admin") && user.role_name == "Admin")
                    return Forbid();

                return Ok(new { success = true, data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                if (!User.IsInRole("Admin") && dto.role_id.HasValue)
                {
                    var roleCheck = await GetRoleNameAsync(dto.role_id.Value);
                    if (roleCheck == "Admin")
                        return Forbid();
                }

                // Default role to "User" if not specified
                if (dto.role_id == null)
                {
                    // Resolve "User" role id dynamically — handled in UserService
                }

                var (success, message, userId) = await _userService.CreateUserAsync(dto);
                if (!success)
                    return BadRequest(new { success = false, message });

                return Ok(new { success = true, message, data = new { user_id = userId } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var target = await _userService.GetUserByIdAsync(id);
                    if (target?.role_name == "Admin")
                        return Forbid();
                    // Managers cannot promote anyone to Admin
                    if (dto.role_id.HasValue)
                    {
                        var roleName = await GetRoleNameAsync(dto.role_id.Value);
                        if (roleName == "Admin") return Forbid();
                    }
                }

                var (success, message) = await _userService.UpdateUserAsync(id, dto);
                if (!success)
                    return BadRequest(new { success = false, message });

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                if (id == currentUserId)
                    return BadRequest(new { success = false, message = "Cannot delete your own account" });

                var (success, message) = await _userService.DeleteUserAsync(id);
                if (!success)
                    return BadRequest(new { success = false, message });

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                if (id == currentUserId)
                    return BadRequest(new { success = false, message = "Cannot deactivate your own account" });

                if (!User.IsInRole("Admin"))
                {
                    var target = await _userService.GetUserByIdAsync(id);
                    if (target?.role_name == "Admin") return Forbid();
                }

                var (success, message) = await _userService.ToggleUserStatusAsync(id);
                if (!success)
                    return BadRequest(new { success = false, message });

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                var (success, message) = await _userService.ChangePasswordAsync(id, dto);
                if (!success)
                    return BadRequest(new { success = false, message });
                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            try
            {
                var (success, message) = await _userService.ResetPasswordAsync(id, dto);
                if (!success)
                    return BadRequest(new { success = false, message });
                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-department/{deptId}")]
        public async Task<IActionResult> GetUsersByDepartment(int deptId)
        {
            try
            {
                var users = await _userService.GetUsersByDepartmentAsync(deptId);
                if (!User.IsInRole("Admin"))
                    users = users.Where(u => u.role_name != "Admin").ToList();
                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-role/{roleId}")]
        public async Task<IActionResult> GetUsersByRole(int roleId)
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(roleId);
                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> GetUserTasks(int id)
        {
            try
            {
                var tasks = await _context.Tasks
                    .Where(t => t.assigned_to == id)
                    .Include(t => t.Project)
                    .Include(t => t.Department)
                    .Select(t => new
                    {
                        t.task_id,
                        t.title,
                        t.status,
                        t.priority,
                        t.per_complete,
                        project_name = t.Project.proj_name,
                        dept_name = t.Department.dept_name,
                        t.planned_start_date,
                        t.planned_end_date,
                        t.actual_start_date,
                        t.actual_end_date
                    })
                    .OrderBy(t => t.planned_start_date)
                    .ToListAsync();

                return Ok(new { success = true, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}/projects")]
        public async Task<IActionResult> GetUserProjects(int id)
        {
            try
            {
                var projects = await _context.Projects
                    .Where(p => p.created_by == id || p.Tasks.Any(t => t.assigned_to == id))
                    .Distinct()
                    .Select(p => new
                    {
                        p.proj_id,
                        p.proj_no,
                        p.proj_name,
                        p.status,
                        p.priority,
                        p.project_start_date,
                        p.target_completion_date
                    })
                    .OrderBy(p => p.proj_name)
                    .ToListAsync();

                return Ok(new { success = true, data = projects });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Helper
        private async Task<string?> GetRoleNameAsync(int roleId)
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var role = await db.Roles.FindAsync(roleId);
            return role?.role_name;
        }
    }
}
