using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Where(u => u.is_active)
                .OrderBy(u => u.username)
                .Select(u => new
                {
                    u.user_id,
                    u.username,
                    u.email,
                    u.dept_id,
                    dept_name = u.Department.dept_name,
                    u.role_id,
                    role_name = u.Role.role_name
                })
                .ToListAsync();

            return Ok(new { success = true, data = users });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Where(u => u.user_id == id)
                .Select(u => new
                {
                    u.user_id,
                    u.username,
                    u.email,
                    u.dept_id,
                    dept_name = u.Department.dept_name,
                    u.role_id,
                    role_name = u.Role.role_name
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new { success = true, data = user });
        }

        [HttpGet("by-department/{deptId}")]
        public async Task<IActionResult> GetUsersByDepartment(int deptId)
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.dept_id == deptId && u.is_active)
                .OrderBy(u => u.username)
                .Select(u => new
                {
                    u.user_id,
                    u.username,
                    u.email,
                    u.dept_id,
                    u.role_id,
                    role_name = u.Role.role_name
                })
                .ToListAsync();

            return Ok(new { success = true, data = users });
        }
    }
}
