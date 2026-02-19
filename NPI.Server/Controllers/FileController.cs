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
            Console.WriteLine($"Files count received: {files?.Count}");
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (files == null || files.Count == 0)
                    return BadRequest(new { success = false, message = "No files provided" });

                task_folder ??= $"Task_{task_id}";
                description ??= "";

                (bool success, string message, List<int> fileIds) =
                await _fileService.UploadFilesAsync(
                    files,
                    proj_id,
                    task_id,
                    task_folder,
                    description,
                    userId
                );

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new { success = true, message, data = new { file_ids = fileIds } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Single file upload (for enquiry compatibility)
        [HttpPost("upload-single")]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadSingleFile(
            [FromForm] IFormFile file,
            [FromForm] int proj_id,
            [FromForm] int? task_id,
            [FromForm] int? doc_type_id,
            [FromForm] int? dept_id,
            [FromForm] int? enquiry_id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file provided" });
                }

                var (success, message, fileRecord) = await _fileService.UploadFileAsync(
                    file,
                    proj_id,
                    task_id,
                    doc_type_id,
                    userId,
                    dept_id,
                    enquiry_id
                );

                if (!success)
                {
                    return BadRequest(new { Success = false, Message = message });
                }

                return Ok(new
                {
                    Success = true,
                    Message = message,
                    File = new
                    {
                        file_id = fileRecord.file_id,
                        file_name = fileRecord.file_name,
                        file_path = fileRecord.file_path,
                        file_size = fileRecord.file_size
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("by-task/{taskId}")]
        public async Task<IActionResult> GetFilesByTask(int taskId)
        {
            try
            {
                var files = await _fileService.GetFilesByTaskAsync(taskId);
                return Ok(new { success = true, data = files });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-project/{projId}")]
        public async Task<IActionResult> GetFilesByProject(int projId)
        {
            try
            {
                var files = await _fileService.GetFilesByProjectAsync(projId);
                return Ok(new { success = true, data = files });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-enquiry/{enquiryId}")]
        public async Task<IActionResult> GetFilesByEnquiry(int enquiryId)
        {
            try
            {
                var files = await _fileService.GetFilesByEnquiryAsync(enquiryId);
                return Ok(new { success = true, data = files });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("download/{fileId}")]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                var (success, fileBytes, contentType) = await _fileService.DownloadFileAsync(fileId);

                if (!success || fileBytes == null)
                {
                    return NotFound(new { Success = false, Message = "File not found" });
                }

                // Get file name from database
                var (pathSuccess, _, filePath) = await _fileService.GetFilePathAsync(fileId);
                var fileName = pathSuccess ? Path.GetFileName(filePath) : "download";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                var (success, message) = await _fileService.DeleteFileWithMessageAsync(fileId);

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("task-count/{taskId}")]
        public async Task<IActionResult> GetTaskDocumentCount(int taskId)
        {
            try
            {
                var count = await _fileService.GetTaskDocumentCountAsync(taskId);
                return Ok(new { success = true, data = new { count } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}