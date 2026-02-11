using Microsoft.AspNetCore.Mvc;
using NPI.Server.Data;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IFileService
    {
        Task<(bool Success, string Message, Files File)> UploadFileAsync(
            IFormFile file, int projId, int? taskId, int? docTypeId, int uploadBy, int? deptId, int? enquiryId = null);
        Task<(bool Success, byte[] FileData, string ContentType)> DownloadFileAsync(int fileId);
        Task<bool> DeleteFileAsync(int fileId);
    }

    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _basePath;
        private readonly long _maxFileSizeBytes;
        private readonly string[] _allowedExtensions;

        public FileService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _basePath = configuration["FileStorage:BasePath"];
            _maxFileSizeBytes = long.Parse(configuration["FileStorage:MaxFileSizeMB"]) * 1024 * 1024;
            _allowedExtensions = configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>();
        }

        public async Task<(bool Success, string Message, Files File)> UploadFileAsync(
            IFormFile file, int projId, int? taskId, int? docTypeId, int uploadBy, int? deptId, int? enquiryId = null)
        {
            // Validate file exists
            if (file == null || file.Length == 0)
            {
                return (false, "No file provided", null);
            }

            // Validate file size
            if (file.Length > _maxFileSizeBytes)
            {
                return (false, $"File size exceeds maximum allowed size of {_maxFileSizeBytes / 1024 / 1024} MB", null);
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return (false, $"File type {extension} is not allowed", null);
            }

            // Sanitize filename
            var originalFileName = Path.GetFileName(file.FileName);
            var safeFileName = Path.GetFileNameWithoutExtension(originalFileName)
                .Replace(" ", "_")
                .Replace("..", "")
                + extension;

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";

            // Create directory structure: BasePath/ProjectId/Year/Month/
            var projectFolder = Path.Combine(_basePath, projId.ToString(),
                DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString("D2"));

            if (!Directory.Exists(projectFolder))
            {
                Directory.CreateDirectory(projectFolder);
            }

            var filePath = Path.Combine(projectFolder, uniqueFileName);

            try
            {
                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save to database
                var fileRecord = new Files
                {
                    proj_id = projId,
                    task_id = taskId,
                    enquiry_id = enquiryId,
                    doc_type_id = docTypeId,
                    file_version = 1,
                    upload_by = uploadBy,
                    dept_id = deptId,
                    file_name = originalFileName,
                    file_path = filePath,
                    file_size = file.Length,
                    content_type = file.ContentType,
                    status = "uploaded",
                    is_latest = true,
                    created_at = DateTime.Now
                };

                _context.Files.Add(fileRecord);
                await _context.SaveChangesAsync();

                return (true, "File uploaded successfully", fileRecord);
            }
            catch (Exception ex)
            {
                // Clean up file if database save fails
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return (false, $"Error uploading file: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, byte[] FileData, string ContentType)> DownloadFileAsync(int fileId)
        {
            var file = await _context.Files.FindAsync(fileId);

            if (file == null)
            {
                return (false, null, null);
            }

            if (!File.Exists(file.file_path))
            {
                return (false, null, null);
            }

            try
            {
                var fileData = await File.ReadAllBytesAsync(file.file_path);
                return (true, fileData, file.content_type);
            }
            catch
            {
                return (false, null, null);
            }
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var file = await _context.Files.FindAsync(fileId);

            if (file == null)
            {
                return false;
            }

            try
            {
                // Soft delete - mark as inactive
                file.status = "deleted";
                await _context.SaveChangesAsync();

                // Optional: physically delete file
                // if (File.Exists(file.file_path))
                // {
                //     File.Delete(file.file_path);
                // }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
