using NPI.Server.DTOs;

public interface IEnquiryService
{
    Task<(bool Success, string Message, EnquiryResponseDto? Enquiry)> CreateEnquiryAsync(
        EnquiryCreateDto dto, int userId, string? ipAddress);

    Task<(bool Success, string Message)> UpdateEnquiryAsync(
        int enquiryId, EnquiryCreateDto dto, int userId, string userRole, string? ipAddress);

    Task<(bool Success, string Message)> SubmitEnquiryAsync(
        int enquiryId, int userId, string userRole, string? ipAddress);

    Task<(bool Success, string Message)> DeleteEnquiryAsync(
        int enquiryId, int userId, string userRole, string? ipAddress);

    Task<EnquiryResponseDto?> GetEnquiryByIdAsync(int enquiryId);
    Task<List<EnquiryResponseDto>> GetAllEnquiriesAsync();
    Task<List<EnquiryResponseDto>> GetEnquiriesByUserAsync(int userId);
    Task<string> GenerateEnquiryNoAsync();
}