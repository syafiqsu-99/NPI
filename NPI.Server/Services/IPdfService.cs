namespace NPI.Server.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateEnquiryPdfAsync(int enquiryId);
    }
}
