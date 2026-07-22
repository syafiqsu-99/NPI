using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class EnquiryCreateDto
    {
        public int? cust_id { get; set; }
        public CustomerCreateDto? new_customer { get; set; }

        [Required(ErrorMessage = "Form category is required")]
        [StringLength(100)]
        public string form_category { get; set; } = string.Empty;
        public Dictionary<string, Dictionary<string, string?>>? field_values { get; set; }
        public CustomerRefDto? CustomerRef { get; set; }
    }

    public class EnquiryResponseDto
    {
        public int enquiry_id { get; set; }
        public string enquiry_no { get; set; } = string.Empty;
        public int? proj_id { get; set; }
        public string? proj_no { get; set; }
        public int cust_id { get; set; }
        public string? customer_name { get; set; }
        public string form_category { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public int created_by { get; set; }
        public string? username { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? submitted_at { get; set; }
        public Dictionary<string, Dictionary<string, string?>> field_values { get; set; } = new();
        public CustomerRefResponseDto? CustomerRef { get; set; }

        public List<FileResponseDto>? Files { get; set; }
    }

    public class FormConfigResponseDto
    {
        public List<FormCategoryDto> categories { get; set; } = new();
        public List<FormSectionDto> sections { get; set; } = new();
    }

    public class FormCategoryDto
    {
        public int category_id { get; set; }
        public string category_name { get; set; } = string.Empty;
        public int display_order { get; set; }
        public bool is_active { get; set; }
    }

    public class UpsertFormCategoryDto
    {
        [Required]
        [StringLength(200)]
        public string category_name { get; set; } = string.Empty;

        public int display_order { get; set; } = 0;
        public bool is_active { get; set; } = true;
    }
    public class FormSectionDto
    {
        public int section_id { get; set; }
        public string section_key { get; set; } = string.Empty;
        public string section_label { get; set; } = string.Empty;
        public string? trigger_keywords { get; set; }
        public int display_order { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public List<FormFieldDto> fields { get; set; } = new();
    }

    public class CreateFormSectionDto
    {
        [Required(ErrorMessage = "Section key is required")]
        [StringLength(100)]
        public string section_key { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section label is required")]
        [StringLength(200)]
        public string section_label { get; set; } = string.Empty;

        [StringLength(500)]
        public string? trigger_keywords { get; set; }

        public int display_order { get; set; } = 0;
        public bool is_active { get; set; } = true;
    }

    public class UpdateFormSectionDto
    {
        [Required(ErrorMessage = "Section label is required")]
        [StringLength(200)]
        public string section_label { get; set; } = string.Empty;

        [StringLength(500)]
        public string? trigger_keywords { get; set; }

        public int display_order { get; set; }
        public bool is_active { get; set; }
    }

    public class FormFieldDto
    {
        public int field_id { get; set; }
        public int section_id { get; set; }
        public string section_key { get; set; } = string.Empty;
        public string section_label { get; set; } = string.Empty;
        public string field_key { get; set; } = string.Empty;
        public string field_label { get; set; } = string.Empty;
        public string field_type { get; set; } = "text";
        public List<string>? options { get; set; }
        public bool is_required { get; set; }
        public bool is_active { get; set; }
        public int display_order { get; set; }
    }

    public class UpsertFormFieldDto
    {
        [Required]
        public int section_id { get; set; }

        [Required]
        [StringLength(100)]
        public string field_key { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string field_label { get; set; } = string.Empty;

        [StringLength(50)]
        public string field_type { get; set; } = "text";

        public List<string>? options { get; set; }
        public bool is_required { get; set; } = false;
        public bool is_active { get; set; } = true;
        public int display_order { get; set; } = 0;
    }
}