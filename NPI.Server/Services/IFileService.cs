using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IFileService
    {
        Task<(bool Success, string Message, Files File)> UploadFileAsync(
            IFormFile file, int projId, int? taskId, int? docTypeId, int uploadBy, int? deptId, int? enquiryId = null);

        Task<(bool Success, string Message, Files File)> UploadCustomerFileAsync(
            IFormFile file, int enquiryId, int uploadBy, string compName);

        Task<(bool Success, byte[] FileData, string ContentType)> DownloadFileAsync(int fileId);

        Task<bool> DeleteFileAsync(int fileId);
        Task<(bool success, string message, List<int> fileIds)> UploadFilesAsync(
            List<IFormFile> files,
            int projId,
            int taskId,
            string taskFolder,
            string description,
            int userId);

        Task<List<FileResponseDto>> GetFilesByTaskAsync(int taskId);

        Task<List<FileResponseDto>> GetFilesByProjectAsync(int projId);

        Task<List<FileResponseDto>> GetFilesByEnquiryAsync(int enquiryId);

        Task<(bool success, string message, string filePath)> GetFilePathAsync(int fileId);

        Task<(bool success, string message)> DeleteFileWithMessageAsync(int fileId);

        Task<int> GetTaskDocumentCountAsync(int taskId);
    }
}