using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.Models;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _context.Customers
                .Where(c => c.is_active)
                .OrderBy(c => c.comp_name)
                .ToListAsync();

            return Ok(new { success = true, data = customers });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound(new { success = false, message = "Customer not found" });
            }

            return Ok(new { success = true, data = customer });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customers customer)
        {
            customer.created_at = DateTime.Now;
            customer.is_active = true;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, data = customer });
        }
    }
}
