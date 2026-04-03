using NPI.Server.DTOs;
using NPI.Server.Models;

namespace NPI.Server.Services
{
    public interface IEnquiryService
    {
        Task<(bool Success, string Message, EnquiryResponseDto Enquiry)> CreateEnquiryAsync(EnquiryCreateDto dto, int userId);
        Task<(bool Success, string Message)> UpdateEnquiryAsync(int enquiryId, EnquiryCreateDto dto, int userId);
        Task<EnquiryResponseDto> GetEnquiryByIdAsync(int enquiryId);
        Task<List<EnquiryResponseDto>> GetAllEnquiriesAsync();
        Task<List<EnquiryResponseDto>> GetEnquiriesByUserAsync(int userId);
        Task<(bool Success, string Message)> SubmitEnquiryAsync(int enquiryId, int userId);
        Task<(bool Success, string Message)> DeleteEnquiryAsync(int enquiryId);
        Task<string> GenerateEnquiryNo();
    }
}