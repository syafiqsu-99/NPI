using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IFileService
    {
        Task<(bool success, string message, Files? file)> UploadFileAsync(IFormFile file, int proj_id, int? task_id, int? doc_type_id, int user_id, int? dept_id, int? enquiry_id = null, string? customer_name = null);
        Task<(bool success, string message, Files? file)> UploadCustomerFileAsync(IFormFile file, int enquiry_id, int user_id, string comp_name);
        Task<(bool success, byte[]? fileData, string? contentType)> DownloadFileAsync(int file_id);
        Task<bool> DeleteFileAsync(int file_id);
        Task<(bool success, string message, List<int> fileIds)> UploadFilesAsync(List<IFormFile> files, int proj_id, int task_id, string task_folder, string description, int user_id);
        Task<List<FileResponseDto>> GetFilesByTaskAsync(int task_id);
        Task<List<FileResponseDto>> GetFilesByProjectAsync(int proj_id);
        Task<List<FileResponseDto>> GetFilesByEnquiryAsync(int enquiry_id);
        Task<(bool success, string message, string? filePath)> GetFilePathAsync(int file_id);
        Task<(bool success, string message)> DeleteFileWithMessageAsync(int file_id, int deleted_by);
        Task<int> GetTaskDocumentCountAsync(int task_id);
        Task<string> EnsureCustomerFolderAsync(string comp_name);
        Task<string> EnsureProjectCustomerFolderAsync(int proj_id);
        Task<List<FileResponseDto>> GetAllCustomerFilesAsync();
        Task<List<DirectoryNodeDto>> GetPhysicalFolderStructureAsync(int user_id);
        Task<(bool success, byte[]? fileData, string? contentType)> DownloadPhysicalFileAsync(string path);
        Task<(bool success, string message)> DeletePhysicalFileAsync(string path, int deleted_by);
    }
}