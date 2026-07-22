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
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _audit;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ApplicationDbContext context,
            IAuditLogService audit,
            ILogger<CustomerController> logger)
        {
            _context = context;
            _audit = audit;
            _logger = logger;
        }

        private int CurrentUserId => RbacHelper.GetUserId(User);
        private string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _context.Customers
                .Where(c => c.is_active)
                .OrderBy(c => c.comp_name)
                .Select(c => new CustomerDto
                {
                    cust_id = c.cust_id,
                    comp_name = c.comp_name,
                    created_at = c.created_at,
                    is_active = c.is_active
                })
                .ToListAsync();

            return Ok(new { success = true, data = customers });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _context.Customers
                .Where(c => c.cust_id == id && c.is_active)
                .Select(c => new CustomerDto
                {
                    cust_id = c.cust_id,
                    comp_name = c.comp_name,
                    created_at = c.created_at,
                    is_active = c.is_active
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound(new { success = false, message = "Customer not found" });
            }

            return Ok(new { success = true, data = customer });
        }

        [HttpPost]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
            }

            var name = dto.comp_name.Trim();

            var duplicate = await _context.Customers
                .AnyAsync(c => c.comp_name == name && c.is_active);
            if (duplicate)
            {
                return BadRequest(new { success = false, message = "A customer with this name already exists." });
            }

            var customer = new Customers
            {
                comp_name = name,
                is_active = dto.is_active,
                created_at = DateTime.Now
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                CurrentUserId, null, "CREATE", "Customers", customer.cust_id,
                null, new { customer.comp_name, customer.is_active }, CurrentIp);

            var createdDto = new CustomerDto
            {
                cust_id = customer.cust_id,
                comp_name = customer.comp_name,
                created_at = customer.created_at,
                is_active = customer.is_active
            };

            return Ok(new { success = true, data = createdDto });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
            }

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.cust_id == id);

            if (existingCustomer == null)
            {
                return NotFound(new { success = false, message = "Customer not found" });
            }

            var name = dto.comp_name.Trim();

            if (name != existingCustomer.comp_name)
            {
                var duplicate = await _context.Customers
                    .AnyAsync(c => c.comp_name == name && c.is_active && c.cust_id != id);
                if (duplicate)
                {
                    return BadRequest(new { success = false, message = "A customer with this name already exists." });
                }
            }

            var before = new { existingCustomer.comp_name, existingCustomer.is_active };

            existingCustomer.comp_name = name;
            existingCustomer.is_active = dto.is_active;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
                await _audit.LogAsync(
                    CurrentUserId, null, "UPDATE_FAILED", "Customers", id,
                    before, new { error = ex.Message }, CurrentIp);
                return StatusCode(500, new { success = false, message = "Error updating customer" });
            }

            await _audit.LogAsync(
                CurrentUserId, null, "UPDATE", "Customers", id,
                before, new { existingCustomer.comp_name, existingCustomer.is_active }, CurrentIp);

            var updatedDto = new CustomerDto
            {
                cust_id = existingCustomer.cust_id,
                comp_name = existingCustomer.comp_name,
                created_at = existingCustomer.created_at,
                is_active = existingCustomer.is_active
            };

            return Ok(new { success = true, data = updatedDto });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.cust_id == id);

            if (customer == null)
            {
                return NotFound(new { success = false, message = "Customer not found" });
            }

            var projectCount = await _context.Projects.CountAsync(p => p.cust_id == id);
            if (projectCount > 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Cannot delete: {projectCount} project(s) reference this customer."
                });
            }

            customer.is_active = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                await _audit.LogAsync(
                    CurrentUserId, null, "DELETE_FAILED", "Customers", id,
                    null, new { error = ex.Message }, CurrentIp);
                return StatusCode(500, new { success = false, message = "Error deleting customer" });
            }

            await _audit.LogAsync(
                CurrentUserId, null, "DELETE", "Customers", id,
                new { customer.comp_name }, null, CurrentIp);

            return Ok(new { success = true, message = "Customer deleted successfully" });
        }
    }
}