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
        string GenerateEnquiryNo();
    }

    public class EnquiryCreateDto
    {
        public int? cust_id { get; set; }
        public CustomerCreateDto? new_customer { get; set; }
        public string npi_category { get; set; }
        public GeneralInfoDto? GeneralInfo { get; set; }
        public SealInfoDto? SealInfo { get; set; }
        public CustomerRefDto? CustomerRef { get; set; }
    }
    public class CustomerCreateDto
    {
        public string comp_name { get; set; }
        public string? cust_addr { get; set; }
        public string? contact_name { get; set; }
        public string? contact_email { get; set; }
        public string? contact_phone { get; set; }
    }

    public class GeneralInfoDto
    {
        public string? company_name { get; set; }
        public int? estimated_qty_per_year { get; set; }
        public DateOnly? estimated_required_date { get; set; }
        public string? color { get; set; }
        public string? material_used { get; set; }
        public float? weight_g { get; set; }
        public string? neck_size_mm { get; set; }
        public string? shape { get; set; }
        public string? hot_cold_filling { get; set; }
        public int? qty_first_submission { get; set; }
        public string? cap_bottle_source { get; set; }
        public string? filling_content { get; set; }
        public string? capping_method { get; set; }
        public string? cap_seal { get; set; }
    }

    public class SealInfoDto
    {
        public string? customer_name { get; set; }
        public string? apply_to_product { get; set; }
        public DateOnly? estimated_required_date { get; set; }
        public string? reason_of_change { get; set; }
        public int? qty_first_submission { get; set; }
        public string? other_requirements { get; set; }
    }

    public class CustomerRefDto
    {
        public string? mould_ownership { get; set; }
    }

    public class EnquiryResponseDto
    {
        public int enquiry_id { get; set; }
        public string enquiry_no { get; set; }
        public int cust_id { get; set; }
        public string? customer_name { get; set; }
        public string npi_category { get; set; }
        public string status { get; set; }
        public int created_by{ get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? submitted_at { get; set; }
        public GeneralInfoResponseDto? GeneralInfo { get; set; }
        public SealInfoResponseDto? SealInfo { get; set; }
        public CustomerRefResponseDto? CustomerRef { get; set; }
        public List<FileResponseDto>? Files { get; set; }
    }

    public class GeneralInfoResponseDto
    {
        public int general_info_id { get; set; }
        public string? company_name { get; set; }
        public int? estimated_qty_per_year { get; set; }
        public DateOnly? estimated_required_date { get; set; }
        public string? color { get; set; }
        public string? material_used { get; set; }
        public float? weight_g { get; set; }
        public string? neck_size_mm { get; set; }
        public string? shape { get; set; }
        public string? hot_cold_filling { get; set; }
        public int? qty_first_submission { get; set; }
        public string? cap_bottle_source { get; set; }
        public string? filling_content { get; set; }
        public string? capping_method { get; set; }
        public string? cap_seal { get; set; }
    }

    public class SealInfoResponseDto
    {
        public int seal_info_id { get; set; }
        public string? customer_name { get; set; }
        public string? apply_to_product { get; set; }
        public DateOnly? estimated_required_date { get; set; }
        public string? reason_of_change { get; set; }
        public int? qty_first_submission { get; set; }
        public string? other_requirements { get; set; }
    }

    public class CustomerRefResponseDto
    {
        public int customer_ref_id { get; set; }
        public string? mould_ownership { get; set; }
    }

    public class FileResponseDto
    {
        public int file_id { get; set; }
        public int proj_id { get; set; }
        public int? task_id { get; set; }
        public int? enquiry_id { get; set; }
        public string? task_name { get; set; }
        public string file_name { get; set; }
        public string file_path { get; set; }
        public long file_size { get; set; }
        public string? file_type { get; set; }
        public string? description { get; set; }
        public int uploaded_by { get; set; }
        public string? uploaded_by_name { get; set; }
        public DateTime uploaded_at { get; set; }
        public int file_version { get; set; }
        public string? status { get; set; }
        public bool is_latest { get; set; }
    }
}