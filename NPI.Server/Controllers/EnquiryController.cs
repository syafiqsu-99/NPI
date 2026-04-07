using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPI.Server.DTOs;
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

        public EnquiryController(
            IEnquiryService enquiryService,
            IFileService fileService,
            IPdfService pdfService)
        {
            _enquiryService = enquiryService;
            _fileService = fileService;
            _pdfService = pdfService;
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

            var (success, message, enquiry) = await _enquiryService.CreateEnquiryAsync(dto, userId);

            return success
                ? Ok(new { success = true, message, data = new { enquiry } })
                : BadRequest(new { success = false, message });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnquiry(int id, [FromBody] EnquiryCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var (success, message) = await _enquiryService.UpdateEnquiryAsync(id, dto, userId);

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitEnquiry(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var (success, message) = await _enquiryService.SubmitEnquiryAsync(id, userId);

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnquiry(int id)
        {
            var (success, message) = await _enquiryService.DeleteEnquiryAsync(id);

            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        // ── File upload ───────────────────────────────────────────────────────

        [HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadFile(
            int id, IFormFile file, [FromQuery] string comp_name = "Unknown")
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid token." });

            var (success, message, uploadedFile) =
                await _fileService.UploadCustomerFileAsync(file, id, userId, comp_name);

            return success
                ? Ok(new { success = true, message, data = new { file = uploadedFile } })
                : BadRequest(new { success = false, message });
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
    }
}