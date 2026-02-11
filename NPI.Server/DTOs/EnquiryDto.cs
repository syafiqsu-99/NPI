namespace NPI.Server.DTOs
{
    public class EnquiryDto
    {
        public int EnquiryId { get; set; }
        public string NpiCategory { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public GeneralInfoDto GeneralInfo { get; set; }
        public SealInfoDto SealInfo { get; set; }
        public CustomerRefDto CustomerRef { get; set; }
        public List<FileDto> Files { get; set; }
    }

    public class GeneralInfoDto
    {
        public string CompanyName { get; set; }
        public int? EstimatedQtyPerYear { get; set; }
        public DateTime? EstimatedRequiredDate { get; set; }
        public string Color { get; set; }
        public string MaterialUsed { get; set; }
        public string WeightG { get; set; }
        public string NeckSizeMm { get; set; }
        public string Shape { get; set; }
        public string HotColdFilling { get; set; }
        public string QtyFirstSubmission { get; set; }
        public string CapBottleSource { get; set; }
        public string FillingContent { get; set; }
        public string CappingMethod { get; set; }
        public string CapSeal { get; set; }
    }

    public class SealInfoDto
    {
        public string CompanyName { get; set; }
        public string ApplyToProduct { get; set; }
        public DateTime? EstimatedRequiredDate { get; set; }
        public string ReasonOfChange { get; set; }
        public int? QtyFirstSubmission { get; set; }
        public string OtherRequirements { get; set; }
    }

    public class CustomerRefDto
    {
        public string MouldOwnership { get; set; }
    }

    public class FileDto
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class CreateEnquiryDto
    {
        public string NpiCategory { get; set; }
        public GeneralInfoDto GeneralInfo { get; set; }
        public SealInfoDto SealInfo { get; set; }
        public CustomerRefDto CustomerRef { get; set; }
    }
}
