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
        private readonly NotificationTriggerService _triggerService;
        private readonly string _basePath;

        public FileService(
            ApplicationDbContext context,
            ITaskService taskService,
            IConfiguration configuration,
            NotificationTriggerService triggerService)
        {
            _context = context;
            _taskService = taskService;
            _triggerService = triggerService;
            _basePath = configuration["FileStorage:BasePath"]
                        ?? throw new InvalidOperationException(
                            "FileStorage:BasePath not configured.");
        }

        // ── Upload: multiple task files ───────────────────────────────────────

        public async Task<(bool success, string message, List<int> fileIds)> UploadFilesAsync(
            List<IFormFile> files, int projId, int taskId,
            string taskFolder, string description, int userId)
        {
            var uploadedIds = new List<int>();
            try
            {
                var folderPath = await _taskService.GetTaskFolderPathAsync(taskId);
                if (folderPath is null)
                    return (false,
                        $"Could not resolve upload folder for task {taskId}.", uploadedIds);

                Directory.CreateDirectory(folderPath);

                var task = await _context.Tasks.FindAsync(taskId);
                if (task is null)
                    return (false, "Task not found", uploadedIds);

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var destPath = BuildUniqueFilePath(folderPath, file.FileName);
                    await using (var stream = new FileStream(destPath, FileMode.Create))
                        await file.CopyToAsync(stream);

                    var record = new Files
                    {
                        proj_id = projId,
                        task_id = taskId,
                        file_version = 1,
                        upload_by = userId,
                        dept_id = task.dept_id,
                        file_name = file.FileName,
                        file_path = destPath,
                        file_size = file.Length,
                        content_type = file.ContentType,
                        status = "Active",
                        is_latest = true,
                        created_at = DateTime.Now
                    };

                    _context.Files.Add(record);
                    await _context.SaveChangesAsync();
                    uploadedIds.Add(record.file_id);
                }

                if (uploadedIds.Count > 0)
                    await _triggerService.OnFileUploadedAsync(projId, userId);

                return (true, $"{uploadedIds.Count} file(s) uploaded", uploadedIds);
            }
            catch (Exception ex)
            {
                return (false, $"Upload failed: {ex.Message}", uploadedIds);
            }
        }

        // ── Upload: single file (task, project, or enquiry/customer) ─────────

        public async Task<(bool success, string message, Files? file)> UploadFileAsync(
            IFormFile file, int projId, int? taskId, int? docTypeId,
            int uploadBy, int? deptId,
            int? enquiryId = null, string? customerName = null)
        {
            try
            {
                if (file is null || file.Length == 0)
                    return (false, "No file provided", null);

                string filePath;

                if (taskId.HasValue)
                {
                    var folderPath = await _taskService.GetTaskFolderPathAsync(taskId.Value);
                    if (folderPath is null)
                        return (false,
                            $"Could not resolve upload folder for task {taskId.Value}.", null);

                    Directory.CreateDirectory(folderPath);
                    filePath = BuildUniqueFilePath(folderPath, file.FileName);
                }
                else if (enquiryId.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(customerName))
                    {
                        customerName = await _context.Enquiries
                            .Where(e => e.enquiry_id == enquiryId.Value)
                            .Select(e => e.Customer!.comp_name)
                            .FirstOrDefaultAsync()
                            ?? $"Enquiry_{enquiryId.Value}";
                    }

                    var enquiryPath = Path.Combine(
                        _basePath, "customers", SanitizeFolderName(customerName));
                    Directory.CreateDirectory(enquiryPath);
                    filePath = BuildUniqueFilePath(enquiryPath, file.FileName);
                }
                else
                {
                    var project = await _context.Projects.FindAsync(projId);
                    if (project is null)
                        return (false, "Project not found", null);

                    var projectPath = Path.Combine(
                        _basePath, "projects", SanitizeFolderName(project.proj_name));
                    Directory.CreateDirectory(projectPath);
                    filePath = BuildUniqueFilePath(projectPath, file.FileName);
                }

                await using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                var record = new Files
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
                    created_at = DateTime.Now
                };

                _context.Files.Add(record);
                await _context.SaveChangesAsync();
                return (true, "File uploaded successfully", record);
            }
            catch (Exception ex)
            {
                return (false, $"Upload failed: {ex.Message}", null);
            }
        }

        // ── Upload: customer file (enquiry attachment) ────────────────────────

        public async Task<(bool success, string message, Files? file)> UploadCustomerFileAsync(
            IFormFile file, int enquiryId, int uploadBy, string compName)
        {
            if (file is null || file.Length == 0)
                return (false, "No file uploaded.", null);

            try
            {
                var safeName = SanitizeFolderName(
                    string.IsNullOrWhiteSpace(compName) ? "UnknownCustomer" : compName);

                var folder = Path.Combine(_basePath, "customers", safeName);
                Directory.CreateDirectory(folder);

                var uniqueName = $"{Guid.NewGuid()}_{SanitizeFileName(file.FileName)}";
                var fullPath = Path.Combine(folder, uniqueName);

                await using (var stream = new FileStream(fullPath, FileMode.Create))
                    await file.CopyToAsync(stream);

                var record = new Files
                {
                    enquiry_id = enquiryId,
                    proj_id = 0,
                    file_name = file.FileName,
                    file_path = fullPath,
                    file_size = file.Length,
                    upload_by = uploadBy,
                    content_type = file.ContentType,
                    status = "Active",
                    is_latest = true,
                    created_at = DateTime.Now
                };

                _context.Files.Add(record);
                await _context.SaveChangesAsync();
                return (true, "Customer file uploaded.", record);
            }
            catch (Exception ex)
            {
                return (false, $"Error uploading: {ex.Message}", null);
            }
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public async Task<List<FileResponseDto>> GetFilesByTaskAsync(int taskId)
        {
            var files = await _context.Files
                .Where(f => f.task_id == taskId && f.is_latest)
                .Include(f => f.UploadByUser)
                .OrderByDescending(f => f.created_at)
                .ToListAsync();

            return files.Select(MapToDto).ToList();
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

            return files.Select(MapToDto).ToList();
        }

        public async Task<List<FileResponseDto>> GetFilesByEnquiryAsync(int enquiryId)
        {
            var files = await _context.Files
                .Where(f => f.enquiry_id == enquiryId && f.is_latest)
                .Include(f => f.UploadByUser)
                .OrderByDescending(f => f.created_at)
                .ToListAsync();

            return files.Select(MapToDto).ToList();
        }

        public async Task<List<FileResponseDto>> GetAllCustomerFilesAsync()
        {
            var files = await _context.Files
                .Where(f => f.enquiry_id != null && f.is_latest)
                .Include(f => f.UploadByUser)
                .Include(f => f.Enquiry)
                    .ThenInclude(e => e!.Customer)
                .OrderByDescending(f => f.created_at)
                .ToListAsync();

            return files.Select(f =>
            {
                var dto = MapToDto(f);
                dto.dept_name = f.Enquiry?.Customer?.comp_name ?? "Unknown Customer";
                return dto;
            }).ToList();
        }

        // ── Download / Delete ─────────────────────────────────────────────────

        public async Task<(bool success, byte[]? fileData, string? contentType)> DownloadFileAsync(int fileId)
        {
            try
            {
                var file = await _context.Files.FindAsync(fileId);
                if (file is null || string.IsNullOrEmpty(file.file_path) || !File.Exists(file.file_path))
                    return (false, null, null);

                var bytes = await File.ReadAllBytesAsync(file.file_path);

                var contentType = file.content_type;
                if (string.IsNullOrEmpty(contentType) || contentType == "application/octet-stream")
                {
                    contentType = GetContentType(file.file_path);
                }

                return (true, bytes, contentType);
            }
            catch
            {
                return (false, null, null);
            }
        }

        public async Task<(bool success, byte[]? fileData, string? contentType)> DownloadPhysicalFileAsync(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var basePath = Path.GetFullPath(_basePath);

            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                basePath += Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(basePath) || !File.Exists(fullPath))
                return (false, null, null);

            var bytes = await File.ReadAllBytesAsync(fullPath);
            return (true, bytes, GetContentType(fullPath));
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _ => "application/octet-stream"
            };
        }

        public async Task<(bool success, string message, string? filePath)> GetFilePathAsync(
            int fileId)
        {
            var file = await _context.Files.FindAsync(fileId);
            if (file is null) return (false, "File not found", null);
            if (string.IsNullOrEmpty(file.file_path) || !File.Exists(file.file_path))
                return (false, "Physical file not found on disk", null);

            return (true, "File found", file.file_path);
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            try
            {
                var file = await _context.Files.FindAsync(fileId);
                if (file is null) return false;

                file.updated_at = DateTime.Now;
                if (!string.IsNullOrEmpty(file.file_path) && File.Exists(file.file_path))
                    File.Delete(file.file_path);

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
                if (file is null) return (false, "File not found");

                file.updated_at = DateTime.Now;
                if (!string.IsNullOrEmpty(file.file_path) && File.Exists(file.file_path))
                    File.Delete(file.file_path);

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
                .CountAsync(f => f.task_id == taskId && f.is_latest);
        }

        public async Task<string> EnsureCustomerFolderAsync(string companyName)
        {
            var path = Path.Combine(_basePath, "customers", SanitizeFolderName(companyName));
            Directory.CreateDirectory(path);
            return path;
        }

        public async Task<string> EnsureProjectCustomerFolderAsync(int projId)
        {
            var projName = await _context.Projects
                .Where(p => p.proj_id == projId)
                .Select(p => p.proj_name)
                .FirstOrDefaultAsync()
                ?? throw new Exception("Project not found");

            var path = Path.Combine(
                _basePath, "projects", SanitizeFolderName(projName), "Customer");
            Directory.CreateDirectory(path);
            return path;
        }

        // ── Physical Structure Access ───────────────────────────────────────

        public async Task<List<DirectoryNodeDto>> GetPhysicalFolderStructureAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.user_id == userId);

            bool isManagerOrAdmin = user?.Role?.role_name == "Admin" || user?.Role?.role_name == "Manager";
            int? userDeptId = user?.dept_id;

            var assignedProjectIds = await _context.ProjectTeams
                .Where(pr => pr.user_id == userId)
                .Select(pr => pr.proj_id)
                .ToListAsync();

            var allProjects = await _context.Projects.ToListAsync();

            var allFiles = await _context.Files
                .Where(f => f.status == "Active" && f.is_latest)
                .ToListAsync();

            var fileMap = allFiles
                .Where(f => !string.IsNullOrWhiteSpace(f.file_path))
                .GroupBy(f => f.file_path!)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var rootNodes = new List<DirectoryNodeDto>();

            // --- A. Scan "Projects" ---
            var projectsPath = Path.Combine(_basePath, "projects");
            var projectsNode = new DirectoryNodeDto
            {
                id = "root-projects",
                name = "Projects",
                type = "root-projects",
                children = new List<DirectoryNodeDto>(),
                can_edit = isManagerOrAdmin
            };

            if (Directory.Exists(projectsPath))
            {
                foreach (var projDir in Directory.GetDirectories(projectsPath))
                {
                    var dirName = Path.GetFileName(projDir);
                    var project = allProjects.FirstOrDefault(p => SanitizeFolderName(p.proj_name) == dirName || p.proj_name == dirName);

                    bool canEditProject = isManagerOrAdmin;
                    if (!canEditProject && project != null)
                    {
                        canEditProject = assignedProjectIds.Contains(project.proj_id) && project.dept_id == userDeptId;
                    }

                    var pNode = BuildPhysicalNode(projDir, canEditProject, fileMap, "project");
                    projectsNode.children.Add(pNode);
                }
            }
            rootNodes.Add(projectsNode);

            // --- B. Scan "Customers" ---
            var customersPath = Path.Combine(_basePath, "customers");
            var customersNode = new DirectoryNodeDto
            {
                id = "root-customers",
                name = "Customers",
                type = "root-customers",
                children = new List<DirectoryNodeDto>(),
                can_edit = isManagerOrAdmin
            };

            if (Directory.Exists(customersPath))
            {
                foreach (var custDir in Directory.GetDirectories(customersPath))
                {
                    var cNode = BuildPhysicalNode(custDir, isManagerOrAdmin, fileMap, "customer");
                    customersNode.children.Add(cNode);
                }
            }
            rootNodes.Add(customersNode);

            return rootNodes;
        }

        private DirectoryNodeDto BuildPhysicalNode(
    string path, bool canEdit, Dictionary<string, Files> fileMap, string defaultType = "folder")
        {
            var node = new DirectoryNodeDto
            {
                id = path,
                name = Path.GetFileName(path),
                type = defaultType,
                path = path,
                can_edit = canEdit,
                children = new List<DirectoryNodeDto>()
            };

            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                    node.children.Add(BuildPhysicalNode(dir, canEdit, fileMap, "folder"));

                foreach (var file in Directory.GetFiles(path))
                {
                    var fNode = new DirectoryNodeDto
                    {
                        id = file,
                        name = Path.GetFileName(file),
                        type = "file",
                        path = file,
                        can_edit = canEdit,
                        size = new FileInfo(file).Length
                    };

                    if (fileMap.TryGetValue(file, out var dbFile))
                    {
                        fNode.file_id = dbFile.file_id;
                        fNode.uploaded_at = dbFile.created_at;
                        fNode.updated_at = dbFile.updated_at;
                        fNode.file_version = dbFile.file_version;
                        fNode.content_type = dbFile.content_type;
                    }

                    node.children.Add(fNode);
                }
            }
            catch (Exception) { /* skip inaccessible folders */ }

            return node;
        }

        public async Task<(bool success, string message)> DeletePhysicalFileAsync(string path)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                var basePath = Path.GetFullPath(_basePath);

                if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    basePath += Path.DirectorySeparatorChar;

                // Strict directory constraints handling preventing path traversal payload
                if (!fullPath.StartsWith(basePath))
                    return (false, "Invalid Path Security");

                if (!File.Exists(fullPath)) return (false, "File not found");

                File.Delete(fullPath);
                return (true, "Deleted from disk");
            }
            catch (Exception ex)
            {
                return (false, $"Delete failed: {ex.Message}");
            }
        }

        // ── Static helpers ────────────────────────────────────────────────────

        private static string BuildUniqueFilePath(string folder, string originalName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var safe = SanitizeFileName(originalName);
            var candidate = Path.Combine(folder, $"{timestamp}_{safe}");

            var counter = 1;
            while (File.Exists(candidate))
            {
                var stem = Path.GetFileNameWithoutExtension(safe);
                var ext = Path.GetExtension(safe);
                candidate = Path.Combine(folder, $"{timestamp}_{stem}_{counter}{ext}");
                counter++;
            }
            return candidate;
        }

        private static readonly HashSet<string> AllowedExtensions = new(
            StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx",
            ".png", ".jpg", ".jpeg", ".dwg", ".step",
            ".stp", ".txt", ".csv"
        };

        private static string SanitizeFileName(string fileName)
        {
            var nameOnly = Path.GetFileName(fileName);
            var stem = Path.GetFileNameWithoutExtension(nameOnly)
                .Replace("..", "_")
                .Replace(" ", "_");

            stem = string.Concat(stem.Select(c =>
                Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

            var ext = Path.GetExtension(nameOnly);
            if (!AllowedExtensions.Contains(ext))
                ext = ".bin";

            return stem + ext;
        }

        private static string SanitizeFolderName(string name)
        {
            var result = name.Replace(" ", "_").Replace("/", "_");
            foreach (var c in Path.GetInvalidFileNameChars())
                result = result.Replace(c, '_');
            return result;
        }

        private static FileResponseDto MapToDto(Files f) => new()
        {
            file_id = f.file_id,
            proj_id = f.proj_id,
            task_id = f.task_id,
            enquiry_id = f.enquiry_id,
            task_name = f.Task?.title,
            dept_name = f.Department?.dept_name,
            file_name = f.file_name,
            file_path = f.file_path,
            file_size = f.file_size,
            file_type = f.content_type,
            uploaded_by = f.upload_by,
            uploaded_by_name = f.UploadByUser?.username,
            uploaded_at = f.created_at,
            updated_at = f.updated_at,
            file_version = f.file_version,
            status = f.status,
            is_latest = f.is_latest
        };
    }
}