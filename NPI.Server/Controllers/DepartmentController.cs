using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;
using NPI.Server.Services;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string? NormalizeCode(string? code)
            => string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                var departments = await _context.Departments
                    .OrderBy(d => d.dept_name)
                    .Select(d => new DepartmentResponseDto
                    {
                        dept_id = d.dept_id,
                        dept_name = d.dept_name,
                        dept_code = d.dept_code,
                        description = d.description,
                        color_hex = d.color_hex,
                        is_assignable = d.is_assignable,
                        user_count = d.Users == null ? 0 : d.Users.Count,
                        created_at = d.created_at
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = departments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            try
            {
                var department = await _context.Departments
                    .Where(d => d.dept_id == id)
                    .Select(d => new DepartmentResponseDto
                    {
                        dept_id = d.dept_id,
                        dept_name = d.dept_name,
                        dept_code = d.dept_code,
                        description = d.description,
                        color_hex = d.color_hex,
                        is_assignable = d.is_assignable,
                        user_count = d.Users == null ? 0 : d.Users.Count,
                        created_at = d.created_at
                    })
                    .FirstOrDefaultAsync();

                if (department == null)
                {
                    return NotFound(new { success = false, message = "Department not found" });
                }

                return Ok(new { success = true, data = department });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid department data",
                        errors = ModelState
                    });
                }

                var normalizedName = dto.dept_name.Trim().ToLower();
                var normalizedCode = NormalizeCode(dto.dept_code);

                if (await _context.Departments.AnyAsync(d => d.dept_name.ToLower() == normalizedName))
                {
                    return BadRequest(new { success = false, message = "Department already exists" });
                }

                if (normalizedCode != null &&
                    await _context.Departments.AnyAsync(d => d.dept_code == normalizedCode))
                {
                    return BadRequest(new { success = false, message = "Department code already in use" });
                }

                var department = new Departments
                {
                    dept_name = dto.dept_name.Trim(),
                    dept_code = normalizedCode,
                    description = dto.description,
                    color_hex = dto.color_hex,
                    is_assignable = dto.is_assignable,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Department created successfully",
                    data = new { dept_id = department.dept_id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid department data",
                        errors = ModelState
                    });
                }

                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return NotFound(new { success = false, message = "Department not found" });
                }

                var normalizedCode = NormalizeCode(dto.dept_code);

                if (dto.dept_name != department.dept_name)
                {
                    var nameExists = await _context.Departments
                        .AnyAsync(d => d.dept_name == dto.dept_name && d.dept_id != id);

                    if (nameExists)
                    {
                        return BadRequest(new { success = false, message = "Department name already exists" });
                    }
                }

                if (normalizedCode != null && normalizedCode != department.dept_code)
                {
                    var codeExists = await _context.Departments
                        .AnyAsync(d => d.dept_code == normalizedCode && d.dept_id != id);

                    if (codeExists)
                    {
                        return BadRequest(new { success = false, message = "Department code already in use" });
                    }
                }

                department.dept_name = dto.dept_name.Trim();
                department.dept_code = normalizedCode;
                department.description = dto.description;
                department.color_hex = dto.color_hex;
                department.is_assignable = dto.is_assignable;
                department.updated_at = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Department updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return NotFound(new { success = false, message = "Department not found" });
                }

                var userCount = await _context.Users.CountAsync(u => u.dept_id == id);
                if (userCount > 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Cannot delete: {userCount} user(s) are assigned to this department."
                    });
                }

                var taskCount = await _context.Tasks.CountAsync(t => t.dept_id == id);
                if (taskCount > 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Cannot delete: {taskCount} task(s) reference this department."
                    });
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Department deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetDepartmentUsers(int id)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.dept_id == id)
                    .Select(u => new
                    {
                        u.user_id,
                        u.username,
                        u.full_name,
                        u.email,
                        role = u.Role.role_name
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> GetDepartmentTasks(int id)
        {
            try
            {
                var tasks = await _context.Tasks
                    .Where(t => t.dept_id == id)
                    .Include(t => t.Project)
                    .Select(t => new
                    {
                        t.task_id,
                        t.title,
                        t.status,
                        t.priority,
                        t.per_complete,
                        project_name = t.Project.proj_name,
                        t.planned_start_date,
                        t.planned_end_date
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
