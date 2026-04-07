using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskService _taskService;
        private readonly IConfiguration _configuration;
        private readonly NotificationTriggerService _triggerService;
        private readonly string _basePath;

        public FileService( ApplicationDbContext context, ITaskService taskService, IConfiguration configuration,NotificationTriggerService triggerService)
        {
            _context = context;
            _taskService = taskService;
            _configuration = configuration;
            _basePath = configuration["FileStorage:BasePath"];
            _triggerService = triggerService;
        }

        public async Task<(bool success, string message, List<int> fileIds)> UploadFilesAsync(
            List<IFormFile> files,
            int projId,
            int taskId,
            string taskFolder,
            string description,
            int userId)
        {
            var uploadedFileIds = new List<int>();
            try
            {
                var folderPath = await _taskService.GetTaskFolderPathAsync(taskId);

                if (folderPath == null)
                    return (false,
                        $"Could not resolve upload folder for task {taskId}. " +
                        "Ensure the task and its project/department exist.", uploadedFileIds);

                Directory.CreateDirectory(folderPath);

                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                    return (false, "Task not found", uploadedFileIds);

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var destPath = BuildUniqueFilePath(folderPath, file.FileName);

                    using (var stream = new FileStream(destPath, FileMode.Create))
                        await file.CopyToAsync(stream);

                    var fileRecord = new Files
                    {
                        proj_id = projId,
                        task_id = taskId,
                        enquiry_id = null,
                        doc_type_id = null,
                        file_version = 1,
                        upload_by = userId,
                        dept_id = task.dept_id,
                        file_name = file.FileName,
                        file_path = destPath,
                        file_size = file.Length,
                        content_type = file.ContentType,
                        status = "Active",
                        is_latest = true,
                        replaced_by = null,
                        created_at = DateTime.Now
                    };

                    _context.Files.Add(fileRecord);
                    await _context.SaveChangesAsync();
                    uploadedFileIds.Add(fileRecord.file_id);
                }

                if (uploadedFileIds.Any())
                    await _triggerService.OnFileUploadedAsync(projId, userId);

                return (true, $"{uploadedFileIds.Count} file(s) uploaded successfully", uploadedFileIds);
            }
            catch (Exception ex)
            {
                return (false, $"Upload failed: {ex.Message}", uploadedFileIds);
            }
        }

        public async Task<(bool Success, string Message, Files File)> UploadCustomerFileAsync(
            IFormFile file, int enquiryId, int uploadBy, string compName)
        {
            if (file == null || file.Length == 0)
                return (false, "No file uploaded.", null);

            try
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                string safeCompName = new string(compName.Where(ch => !invalidChars.Contains(ch)).ToArray()).Trim();
                if (string.IsNullOrEmpty(safeCompName)) safeCompName = "UnknownCustomer";

                string relativePath = Path.Combine("Database", "Customers", safeCompName);
                string basePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                string originalFileName = Path.GetFileName(file.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";
                string fullPath = Path.Combine(basePath, uniqueFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var newFile = new Files
                {
                    enquiry_id = enquiryId,
                    file_name = originalFileName,
                    file_path = Path.Combine(relativePath, uniqueFileName).Replace("\\", "/"),
                    file_size = file.Length,
                    upload_by = uploadBy,
                };

                _context.Files.Add(newFile);
                await _context.SaveChangesAsync();

                return (true, "Customer file uploaded successfully.", newFile);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred while uploading: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, Files? File)> UploadFileAsync(IFormFile file, int projId, int? taskId, int? docTypeId, int uploadBy, int? deptId, int? enquiryId = null, string? customerName = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, "No file provided", null);

                string filePath;

                if (taskId.HasValue)
                {
                    var folderPath = await _taskService.GetTaskFolderPathAsync(taskId.Value);
                    if (folderPath == null)
                        return (false,
                            $"Could not resolve upload folder for task {taskId.Value}. " +
                            "Ensure the task and its project/department exist.", null);

                    Directory.CreateDirectory(folderPath);
                    filePath = BuildUniqueFilePath(folderPath, file.FileName);
                }
                else if (enquiryId.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(customerName))
                    {
                        // Fallback: fetch customer name from enquiry
                        var enquiry = await _context.Enquiries
                            .Include(e => e.Customer)
                            .FirstOrDefaultAsync(e => e.enquiry_id == enquiryId.Value);

                        if (enquiry?.Customer != null)
                            customerName = enquiry.Customer.comp_name;
                        else
                            customerName = $"Enquiry_{enquiryId.Value}";
                    }

                    // Sanitize customer name
                    var sanitizedName = SanitizeFolderName(customerName);
                    var enquiryPath = Path.Combine(_basePath, "customers", sanitizedName);
                    Directory.CreateDirectory(enquiryPath);
                    filePath = BuildUniqueFilePath(enquiryPath, file.FileName);
                }
                else
                {
                    var project = await _context.Projects.FindAsync(projId);
                    if (project == null)
                        return (false, "Project not found", null);

                    var projFolder = SanitizeFolderName(project.proj_name);
                    var projectPath = Path.Combine(_basePath, "projects", projFolder);
                    Directory.CreateDirectory(projectPath);
                    filePath = BuildUniqueFilePath(projectPath, file.FileName);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                var fileRecord = new Files
                {
                    proj_id = projId,
                    task_id = taskId,
                    enquiry_id = enquiryId,
                    doc_type_id = docTypeId,
                    file_version = 1,
                    upload_by = uploadBy,
                    dept_id = deptId,
                    file_name = file.FileName,
                    file_path = filePath,
                    file_size = file.Length,
                    content_type = file.ContentType,
                    status = "Active",
                    is_latest = true,
                    replaced_by = null,
                    created_at = DateTime.Now
                };

                _context.Files.Add(fileRecord);
                await _context.SaveChangesAsync();
                return (true, "File uploaded successfully", fileRecord);
            }
            catch (Exception ex)
            {
                return (false, $"Upload failed: {ex.Message}", null);
            }
        }

        public async Task<List<FileResponseDto>> GetFilesByTaskAsync(int taskId)
        {
            var files = await _context.Files
                .Where(f => f.task_id == taskId && f.is_latest)
                .Include(f => f.UploadByUser)
                .OrderByDescending(f => f.created_at)
                .ToListAsync();

            return files.Select(MapToResponseDto).ToList();
        }

        public async Task<List<FileResponseDto>> GetFilesByProjectAsync(int projId)
        {
            var files = await _context.Files
                .Where(f => f.proj_id == projId && f.is_latest)
                .Include(f => f.UploadByUser)
                .Include(f => f.Task)
                .Include(f => f.Department)
                .OrderByDescending(f => f.created_at)
                .ToListAsync();

            return files.Select(MapToResponseDto).ToList();
        }

        public async Task<List<FileResponseDto>> GetFilesByEnquiryAsync(int enquiryId)
        {
            var files = await _context.Files
                .Where(f => f.enquiry_id == enquiryId && f.is_latest)
                .Include(f => f.UploadByUser)
                .OrderByDescending(f => f.created_at)
                .ToListAsync();

            return files.Select(MapToResponseDto).ToList();
        }

        public async Task<(bool Success, byte[]? FileData, string? ContentType)> DownloadFileAsync(int fileId)
        {
            try
            {
                var file = await _context.Files.FindAsync(fileId);
                if (file == null || !System.IO.File.Exists(file.file_path))
                    return (false, null, null);

                var bytes = await System.IO.File.ReadAllBytesAsync(file.file_path);
                return (true, bytes, file.content_type ?? "application/octet-stream");
            }
            catch { return (false, null, null); }
        }

        public async Task<(bool success, string message, string? filePath)> GetFilePathAsync(int fileId)
        {
            var file = await _context.Files.FindAsync(fileId);
            if (file == null) return (false, "File not found", null);
            if (!System.IO.File.Exists(file.file_path))
                return (false, "Physical file not found on disk", null);
            return (true, "File found", file.file_path);
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            try
            {
                var file = await _context.Files.FindAsync(fileId);
                if (file == null) return false;
                if (System.IO.File.Exists(file.file_path))
                    System.IO.File.Delete(file.file_path);
                _context.Files.Remove(file);
                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<(bool success, string message)> DeleteFileWithMessageAsync(int fileId)
        {
            try
            {
                var file = await _context.Files.FindAsync(fileId);
                if (file == null) return (false, "File not found");
                if (System.IO.File.Exists(file.file_path))
                    System.IO.File.Delete(file.file_path);
                _context.Files.Remove(file);
                await _context.SaveChangesAsync();
                return (true, "File deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Delete failed: {ex.Message}");
            }
        }

        public async Task<int> GetTaskDocumentCountAsync(int taskId)
        {
            return await _context.Files
                .Where(f => f.task_id == taskId && f.is_latest)
                .CountAsync();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string BuildUniqueFilePath(string folderPath, string originalFileName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var safeName = SanitizeFileName(originalFileName);
            var candidate = Path.Combine(folderPath, $"{timestamp}_{safeName}");

            var counter = 1;
            while (System.IO.File.Exists(candidate))
            {
                var nameOnly = Path.GetFileNameWithoutExtension(safeName);
                var ext = Path.GetExtension(safeName);
                candidate = Path.Combine(folderPath, $"{timestamp}_{nameOnly}_{counter}{ext}");
                counter++;
            }
            return candidate;
        }

        private static string SanitizeFileName(string fileName)
        {
            var nameOnly = Path.GetFileName(fileName);
            var stem = Path.GetFileNameWithoutExtension(nameOnly)
                           .Replace("..", "_")
                           .Replace(" ", "_");

            stem = string.Concat(stem.Select(c =>
                Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

            var ext = Path.GetExtension(nameOnly);

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg",
              ".dwg", ".step", ".stp", ".txt", ".csv" };

            if (!allowedExtensions.Contains(ext))
                ext = ".bin";

            return stem + ext;
        }

        private void ValidatePathWithinBase(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var fullBase = Path.GetFullPath(_basePath);
            if (!fullPath.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Path traversal attempt detected.");
        }

        private static string SanitizeFolderName(string name)
        {
            var result = name.Replace(" ", "_").Replace("/", "_");
            foreach (var c in Path.GetInvalidFileNameChars())
                result = result.Replace(c, '_');
            return result;
        }

        private static FileResponseDto MapToResponseDto(Files f) => new()
        {
            file_id = f.file_id,
            proj_id = f.proj_id,
            task_id = f.task_id,
            task_name = f.Task?.title,
            dept_name = f.Department?.dept_name,
            enquiry_id = f.enquiry_id,
            file_name = f.file_name,
            file_path = f.file_path,
            file_size = f.file_size,
            file_type = f.content_type,
            description = null,
            uploaded_by = f.upload_by,
            uploaded_by_name = f.UploadByUser?.username,
            uploaded_at = f.created_at,
            file_version = f.file_version,
            status = f.status,
            is_latest = f.is_latest
        };
    }
}