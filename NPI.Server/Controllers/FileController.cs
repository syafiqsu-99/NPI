using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NPI.Server.Services;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadFiles(
            [FromForm] List<IFormFile> files,
            [FromForm] int proj_id,
            [FromForm] int task_id,
            [FromForm] string? task_folder,
            [FromForm] string? description)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid user identity." });

            if (files == null || files.Count == 0)
                return BadRequest(new { success = false, message = "No files provided" });

            var (success, message, fileIds) = await _fileService.UploadFilesAsync(
                files, proj_id, task_id, task_folder ?? $"Task_{task_id}",
                description ?? string.Empty, userId);

            return success
                ? Ok(new { success = true, message, data = new { file_ids = fileIds } })
                : BadRequest(new { success = false, message });
        }

        [HttpPost("upload-single")]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadSingleFile(
            [FromForm] IFormFile file,
            [FromForm] int proj_id,
            [FromForm] int? task_id,
            [FromForm] int? doc_type_id,
            [FromForm] int? dept_id,
            [FromForm] int? enquiry_id,
            [FromForm] string? customer_name)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { success = false, message = "Invalid user identity." });

            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file provided" });

            var (success, message, result) = await _fileService.UploadFileAsync(
                file, proj_id, task_id, doc_type_id,
                userId, dept_id, enquiry_id, customer_name);

            if (!success || result is null)
                return BadRequest(new { success = false, message });

            return Ok(new
            {
                success = true,
                message,
                file = new
                {
                    result.file_id,
                    result.file_name,
                    result.file_path,
                    result.file_size
                }
            });
        }

        [HttpGet("by-task/{taskId}")]
        public async Task<IActionResult> GetFilesByTask(int taskId)
        {
            var files = await _fileService.GetFilesByTaskAsync(taskId);
            return Ok(new { success = true, data = files });
        }

        [HttpGet("by-project/{projId}")]
        public async Task<IActionResult> GetFilesByProject(int projId)
        {
            var files = await _fileService.GetFilesByProjectAsync(projId);
            return Ok(new { success = true, data = files });
        }

        [HttpGet("by-enquiry/{enquiryId}")]
        public async Task<IActionResult> GetFilesByEnquiry(int enquiryId)
        {
            var files = await _fileService.GetFilesByEnquiryAsync(enquiryId);
            return Ok(new { success = true, data = files });
        }

        [HttpGet("by-customers")]
        public async Task<IActionResult> GetCustomerFiles()
        {
            var files = await _fileService.GetAllCustomerFilesAsync();
            return Ok(new { success = true, data = files });
        }

        [HttpGet("download/{fileId}")]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            var (success, fileBytes, contentType) = await _fileService.DownloadFileAsync(fileId);

            if (!success || fileBytes is null)
                return NotFound(new { success = false, message = "File not found" });

            var (_, _, filePath) = await _fileService.GetFilePathAsync(fileId);
            var fileName = filePath is not null ? Path.GetFileName(filePath) : "download";

            return File(fileBytes, contentType!, fileName);
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var (success, message) = await _fileService.DeleteFileWithMessageAsync(fileId);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }

        [HttpGet("task-count/{taskId}")]
        public async Task<IActionResult> GetTaskDocumentCount(int taskId)
        {
            var count = await _fileService.GetTaskDocumentCountAsync(taskId);
            return Ok(new { success = true, data = new { count } });
        }

        private bool TryGetUserId(out int userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out userId);
        }
    }
}