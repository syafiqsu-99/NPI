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

        public EnquiryController(IEnquiryService enquiryService, IFileService fileService, IPdfService pdfService)
        {
            _enquiryService = enquiryService;
            _fileService = fileService;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEnquiries()
        {
            var enquiries = await _enquiryService.GetAllEnquiriesAsync();
            return Ok(new { success = true, data = enquiries });
        }

        [HttpGet("my-enquiries")]
        public async Task<IActionResult> GetMyEnquiries()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user identity claim." });
            }
            var enquiries = await _enquiryService.GetEnquiriesByUserAsync(userId);
            return Ok(new { success = true, data = enquiries });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnquiryById(int id)
        {
            var enquiry = await _enquiryService.GetEnquiryByIdAsync(id);

            if (enquiry == null)
            {
                return NotFound(new { success = false, message = "Enquiry not found" });
            }

            return Ok(new { success = true, data = enquiry });
        }

        [HttpPost]
        public async Task<IActionResult> CreateEnquiry([FromBody] EnquiryCreateDto dto)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user identity claim." });
            }
            var (success, message, enquiry) = await _enquiryService.CreateEnquiryAsync(dto, userId);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message, data = new { enquiry } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnquiry(int id, [FromBody] EnquiryCreateDto dto)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user identity claim." });
            }
            var (success, message) = await _enquiryService.UpdateEnquiryAsync(id, dto, userId);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitEnquiry(int id)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user identity claim." });
            }
            var (success, message) = await _enquiryService.SubmitEnquiryAsync(id, userId);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnquiry(int id)
        {
            var (success, message) = await _enquiryService.DeleteEnquiryAsync(id);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message });
        }

        [HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadFile(int id, IFormFile file, [FromQuery] string comp_name = "Unknown")
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user identity claim." });
            }

            var (success, message, uploadedFile) = await _fileService.UploadCustomerFileAsync(
                file, id, userId, comp_name);

            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            return Ok(new { success = true, message, data = new { file = uploadedFile } });
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetEnquiryPdf(int id)
        {
            Console.WriteLine("PREPARING PDF. . .");
            try
            {
                var pdfBytes = await _pdfService.GenerateEnquiryPdfAsync(id);
                Console.WriteLine(pdfBytes.Length);
                return File(pdfBytes, "application/pdf", $"Enquiry_{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
