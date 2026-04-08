using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Upload a single file (task, enquiry, or project-level).
        /// Resolves storage path based on context and creates directories if needed.
        /// ✅ Tuple: (success, message, File object)
        /// </summary>
        Task<(bool success, string message, Files? file)> UploadFileAsync(
            IFormFile file,
            int proj_id,
            int? task_id,
            int? doc_type_id,
            int uploadBy,
            int? dept_id,
            int? enquiry_id = null,
            string? customer_name = null);

        /// <summary>
        /// Upload file to customer-specific directory: ../customers/[CustomerName]/[FileName]
        /// ✅ Tuple: (success, message, File object)
        /// </summary>
        Task<(bool success, string message, Files? file)> UploadCustomerFileAsync(
            IFormFile file,
            int enquiryId,
            int uploadBy,
            string compName);

        /// <summary>
        /// Download file from disk and return binary data.
        /// ✅ Tuple: (success, file data bytes, content type)
        /// </summary>
        Task<(bool success, byte[]? fileData, string? contentType)> DownloadFileAsync(int fileId);

        /// <summary>
        /// Delete file and return boolean result.
        /// </summary>
        Task<bool> DeleteFileAsync(int fileId);

        /// <summary>
        /// Batch upload multiple files to task folder.
        /// ✅ Tuple: (success, message, list of file IDs)
        /// </summary>
        Task<(bool success, string message, List<int> fileIds)> UploadFilesAsync(
            List<IFormFile> files,
            int projId,
            int taskId,
            string taskFolder,
            string description,
            int userId);

        /// <summary>
        /// Retrieve all files linked to a specific task.
        /// </summary>
        Task<List<FileResponseDto>> GetFilesByTaskAsync(int taskId);

        /// <summary>
        /// Retrieve all files linked to a specific project.
        /// </summary>
        Task<List<FileResponseDto>> GetFilesByProjectAsync(int projId);

        /// <summary>
        /// Retrieve all files linked to a specific enquiry.
        /// </summary>
        Task<List<FileResponseDto>> GetFilesByEnquiryAsync(int enquiryId);

        /// <summary>
        /// Get file path from database for a specific file ID.
        /// ✅ Tuple: (success, message, file path)
        /// </summary>
        Task<(bool success, string message, string? filePath)> GetFilePathAsync(int fileId);

        /// <summary>
        /// Delete file and return success/error message.
        /// ✅ Tuple: (success, message)
        /// </summary>
        Task<(bool success, string message)> DeleteFileWithMessageAsync(int fileId);

        /// <summary>
        /// Count total documents uploaded for a task.
        /// </summary>
        Task<int> GetTaskDocumentCountAsync(int taskId);
    }
}