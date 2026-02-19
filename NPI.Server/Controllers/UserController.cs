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
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public UserController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Department)
                    .Include(u => u.Role)
                    .OrderBy(u => u.username)
                    .Select(u => new UserResponseDto
                    {
                        user_id = u.user_id,
                        username = u.username,
                        full_name = u.full_name,
                        email = u.email,
                        dept_id = u.dept_id,
                        dept_name = u.Department != null ? u.Department.dept_name : null,
                        role = u.Role != null ? u.Role.role_name : null,
                        is_active = u.is_active,
                        created_at = u.created_at
                    })
                    .ToListAsync();

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
                var user = await _context.Users
                    .Include(u => u.Department)
                    .Include(u => u.Role)
                    .Where(u => u.user_id == id)
                    .Select(u => new UserResponseDto
                    {
                        user_id = u.user_id,
                        username = u.username,
                        full_name = u.full_name,
                        email = u.email,
                        dept_id = u.dept_id,
                        dept_name = u.Department != null ? u.Department.dept_name : null,
                        role = u.Role != null ? u.Role.role_name : null,
                        is_active = u.is_active,
                        created_at = u.created_at
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                return Ok(new { success = true, data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                // Check if username or email already exists
                var userExists = await _context.Users
                    .AnyAsync(u => u.username == dto.username || u.email == dto.email);

                if (userExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Username or email already exists"
                    });
                }

                var user = new Models.Users
                {
                    username = dto.username,
                    email = dto.email,
                    full_name = dto.full_name,
                    dept_id = dto.dept_id,
                    role_id = dto.role_id,
                    is_active = true,
                    created_at = DateTime.Now
                };

                var (success, message) = await _authService.RegisterAsync(user, dto.password);

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new
                {
                    success = true,
                    message = "User created successfully",
                    data = new { user_id = user.user_id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                var duplicateExists = await _context.Users
                    .AnyAsync(u => u.user_id != id &&
                                   (u.username == dto.username || u.email == dto.email));

                if (duplicateExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Username or email already exists"
                    });
                }

                user.username = dto.username;
                user.email = dto.email;
                user.full_name = dto.full_name;
                user.dept_id = dto.dept_id;
                user.role_id = dto.role_id;
                user.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (id == currentUserId)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cannot delete your own account"
                    });
                }

                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Soft delete - just deactivate instead of removing
                user.is_active = false;
                user.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                user.is_active = true;
                user.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "User activated successfully" });
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
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var isAdmin = User.IsInRole("Admin");

                // Only allow users to change their own password unless admin
                if (id != currentUserId && !isAdmin)
                {
                    return Forbid();
                }

                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Verify old password if changing own password
                if (id == currentUserId)
                {
                    var (loginSuccess, _, _) = await _authService.LoginAsync(user.username, dto.current_password);
                    if (!loginSuccess)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Current password is incorrect"
                        });
                    }
                }

                // Update password
                user.password_hash = BCrypt.Net.BCrypt.HashPassword(dto.new_password);
                user.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Password changed successfully" });
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

        [HttpGet("by-department/{deptId}")]
        public async Task<IActionResult> GetUsersByDepartment(int deptId)
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Department)
                    .Include(u => u.Role)
                    .Where(u => u.dept_id == deptId && u.is_active)
                    .OrderBy(u => u.username)
                    .Select(u => new UserResponseDto
                    {
                        user_id = u.user_id,
                        username = u.username,
                        full_name = u.full_name,
                        email = u.email,
                        dept_id = u.dept_id,
                        dept_name = u.Department != null ? u.Department.dept_name : null,
                        role = u.Role != null ? u.Role.role_name : null,
                        is_active = u.is_active,
                        created_at = u.created_at
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-role/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByRole(int roleId)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.role_id == roleId)
                    .Include(u => u.Department)
                    .Include(u => u.Role)
                    .OrderBy(u => u.username)
                    .Select(u => new UserResponseDto
                    {
                        user_id = u.user_id,
                        username = u.username,
                        full_name = u.full_name,
                        email = u.email,
                        dept_id = u.dept_id,
                        dept_name = u.Department != null ? u.Department.dept_name : null,
                        role = u.Role != null ? u.Role.role_name : null,
                        is_active = u.is_active,
                        created_at = u.created_at
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
