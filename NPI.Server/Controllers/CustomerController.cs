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
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ApplicationDbContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

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
                    cust_addr = c.cust_addr,
                    contact_name = c.contact_name,
                    contact_email = c.contact_email,
                    contact_phone = c.contact_phone,
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
                    cust_addr = c.cust_addr,
                    contact_name = c.contact_name,
                    contact_email = c.contact_email,
                    contact_phone = c.contact_phone,
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
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });
            }

            var customer = new Customers
            {
                comp_name = dto.comp_name,
                cust_addr = dto.cust_addr,
                contact_name = dto.contact_name,
                contact_email = dto.contact_email,
                contact_phone = dto.contact_phone,
                is_active = dto.is_active,
                created_at = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var createdDto = new CustomerDto
            {
                cust_id = customer.cust_id,
                comp_name = customer.comp_name,
                cust_addr = customer.cust_addr,
                contact_name = customer.contact_name,
                contact_email = customer.contact_email,
                contact_phone = customer.contact_phone,
                created_at = customer.created_at,
                is_active = customer.is_active
            };

            return Ok(new { success = true, data = createdDto });
        }

        [HttpPut("{id:int}")]
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

            existingCustomer.comp_name = dto.comp_name;
            existingCustomer.cust_addr = dto.cust_addr;
            existingCustomer.contact_name = dto.contact_name;
            existingCustomer.contact_email = dto.contact_email;
            existingCustomer.contact_phone = dto.contact_phone;

            existingCustomer.is_active = dto.is_active;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
                return StatusCode(500, new { success = false, message = "Error updating customer" });
            }

            var updatedDto = new CustomerDto
            {
                cust_id = existingCustomer.cust_id,
                comp_name = existingCustomer.comp_name,
                cust_addr = existingCustomer.cust_addr,
                contact_name = existingCustomer.contact_name,
                contact_email = existingCustomer.contact_email,
                contact_phone = existingCustomer.contact_phone,
                created_at = existingCustomer.created_at,
                is_active = existingCustomer.is_active
            };

            return Ok(new { success = true, data = updatedDto });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.cust_id == id);

            if (customer == null)
            {
                return NotFound(new { success = false, message = "Customer not found" });
            }

            customer.is_active = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                return StatusCode(500, new { success = false, message = "Error deleting customer" });
            }

            return Ok(new { success = true, message = "Customer deleted successfully" });
        }
    }
}
