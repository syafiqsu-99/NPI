using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Services;
using System.Security.Claims;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnquiryController : ControllerBase
    {
        private readonly IEnquiryService _enquiryService;
        private readonly IFileService _fileService;
        private readonly IPdfService _pdfService;
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _audit;

        public EnquiryController(
            IEnquiryService enquiryService,
            IFileService fileService,
            IPdfService pdfService,
            ApplicationDbContext context)
        {
            _enquiryService = enquiryService;
            _fileService = fileService;
            _pdfService = pdfService;
            _context = context;
        }

        // ── Read ──────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAllEnquiries()
        {
            var enquiries = await _enquiryService.GetAllEnquiriesAsync();
            return Ok(new { success = true, data = enquiries });
        }

        [HttpGet("my-enquiries")]
        public async Task<IActionResult> GetMyEnquiries()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var enquiries = await _enquiryService.GetEnquiriesByUserAsync(userId);
            return Ok(new { success = true, data = enquiries });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnquiryById(int id)
        {
            var enquiry = await _enquiryService.GetEnquiryByIdAsync(id);

            return enquiry is null
                ? NotFound(new { success = false, message = "Enquiry not found." })
                : Ok(new { success = true, data = enquiry });
        }

        // ── Write ─────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> CreateEnquiry([FromBody] EnquiryCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var (success, message, enquiry) =
                await _enquiryService.CreateEnquiryAsync(dto, userId, GetIpAddress());

            return success
                ? Ok(new { success = true, message, data = new { enquiry } })
                : BadRequest(new { success = false, message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnquiry(int id, [FromBody] EnquiryCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var (success, message) = await _enquiryService.UpdateEnquiryAsync(
                id, dto, userId, RbacHelper.GetSystemRole(User), GetIpAddress());

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitEnquiry(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var (success, message) = await _enquiryService.SubmitEnquiryAsync(
                id, userId, RbacHelper.GetSystemRole(User), GetIpAddress());

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnquiry(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var (success, message) = await _enquiryService.DeleteEnquiryAsync(
                id, userId, RbacHelper.GetSystemRole(User), GetIpAddress());

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        // ── File upload ───────────────────────────────────────────────────────

        [HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadFile(int id, IFormFile file, [FromQuery] string comp_name = "Unknown")
        {
            if (!TryGetUserId(out var user_id))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var enquiry = await _context.Enquiries
                .Include(e => e.Customer)
                .FirstOrDefaultAsync(e => e.enquiry_id == id);

            if (enquiry is null)
                return NotFound(new { success = false, message = "Enquiry not found" });

            var userRole = RbacHelper.GetSystemRole(User);
            if (!RbacHelper.IsAdminOrManager(userRole) && enquiry.created_by != user_id)
            {
                await _audit.LogAsync(user_id, null, "UPLOAD_DENIED", "Files", id,
                    null, new { enquiry_id = id }, GetIpAddress());
                return Forbid();
            }

            if (enquiry.status != EnquiryStatus.Draft)
                return BadRequest(new
                {
                    success = false,
                    message = "Files can only be attached while the enquiry is a Draft."
                });

            if (string.IsNullOrWhiteSpace(comp_name) && enquiry.Customer is not null)
                comp_name = enquiry.Customer.comp_name;

            var (success, message, uploaded) = await _fileService.UploadFileAsync(
                file,
                proj_id: null,
                task_id: null,
                doc_type_id: null,
                user_id: user_id,
                dept_id: null,
                enquiry_id: id,
                customer_name: comp_name);

            if (!success)
                return BadRequest(new { success = false, message });

            await _audit.LogAsync(user_id, null, "UPLOAD", "Files", uploaded!.file_id,
                null,
                new { uploaded.file_name, uploaded.file_size, enquiry_id = id },
                GetIpAddress());

            return Ok(new { success = true, message, data = new { file = uploaded } });
        }

        // ── PDF ───────────────────────────────────────────────────────────────

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetEnquiryPdf(int id)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateEnquiryPdfAsync(id);
                return File(pdfBytes, "application/pdf", $"Enquiry_{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private bool TryGetUserId(out int userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out userId);
        }

        private string? GetIpAddress() =>
            HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}