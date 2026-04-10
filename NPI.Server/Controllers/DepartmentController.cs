using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Models;

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
                        description = d.description,
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
                        description = d.description,
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
        [Authorize(Roles = "Admin,Manager")]
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

                var exists = await _context.Departments
                    .AnyAsync(d => d.dept_name.ToLower() == normalizedName);

                if (exists)
                {
                    return BadRequest(new { success = false, message = "Department already exists" });
                }

                var department = new Departments
                {
                    dept_name = dto.dept_name,
                    description = dto.description,
                    created_at = DateTime.UtcNow
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
        [Authorize(Roles = "Admin,Manager")]
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

                if (dto.dept_name != department.dept_name)
                {
                    var nameExists = await _context.Departments
                        .AnyAsync(d => d.dept_name == dto.dept_name && d.dept_id != id);

                    if (nameExists)
                    {
                        return BadRequest(new { success = false, message = "Department name already exists" });
                    }
                }

                department.dept_name = dto.dept_name;
                department.description = dto.description;
                department.updated_at = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Department updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return NotFound(new { success = false, message = "Department not found" });
                }

                var hasUsers = await _context.Users.AnyAsync(u => u.dept_id == id);
                if (hasUsers)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cannot delete department with existing users"
                    });
                }

                var hasProjects = await _context.Projects.AnyAsync(p => p.dept_id == id);
                if (hasProjects)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cannot delete department with existing projects"
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

        [HttpGet("{id}/projects")]
        public async Task<IActionResult> GetDepartmentProjects(int id)
        {
            try
            {
                var projects = await _context.Projects
                    .Where(p => p.dept_id == id)
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
                    .ToListAsync();

                return Ok(new { success = true, data = projects });
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
