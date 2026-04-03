namespace NPI.Server.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateEnquiryPdfAsync(int enquiryId);
        Task<byte[]> GenerateProjectStatusReportAsync(int projectId);
    }
}
