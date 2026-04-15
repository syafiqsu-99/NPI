using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IFileService
    {
        Task<(bool success, string message, Files? file)> UploadFileAsync(IFormFile file, int proj_id, int? task_id, int? doc_type_id, int uploadBy, int? dept_id, int? enquiry_id = null, string? customer_name = null);
        Task<(bool success, string message, Files? file)> UploadCustomerFileAsync(IFormFile file, int enquiryId, int uploadBy, string compName);
        Task<(bool success, byte[]? fileData, string? contentType)> DownloadFileAsync(int fileId);
        Task<bool> DeleteFileAsync(int fileId);
        Task<(bool success, string message, List<int> fileIds)> UploadFilesAsync(List<IFormFile> files, int projId, int taskId, string taskFolder, string description, int userId);
        Task<List<FileResponseDto>> GetFilesByTaskAsync(int taskId);
        Task<List<FileResponseDto>> GetFilesByProjectAsync(int projId);
        Task<List<FileResponseDto>> GetFilesByEnquiryAsync(int enquiryId);
        Task<(bool success, string message, string? filePath)> GetFilePathAsync(int fileId);
        Task<(bool success, string message)> DeleteFileWithMessageAsync(int fileId);
        Task<int> GetTaskDocumentCountAsync(int taskId);
        Task<string> EnsureCustomerFolderAsync(string companyName);
        Task<string> EnsureProjectCustomerFolderAsync(int projId);
        Task<List<FileResponseDto>> GetAllCustomerFilesAsync();
        Task<List<DirectoryNodeDto>> GetPhysicalFolderStructureAsync(int userId);
        Task<(bool success, byte[]? fileData, string? contentType)> DownloadPhysicalFileAsync(string path);
        Task<(bool success, string message)> DeletePhysicalFileAsync(string path);
    }
}