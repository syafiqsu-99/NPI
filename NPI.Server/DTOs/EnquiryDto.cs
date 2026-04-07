using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ENQUIRY CRUD DTOs
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Payload for creating or updating an enquiry.
    /// field_values mirrors how the Vue form organises data:
    ///   { section_key → { field_key → user_value } }
    /// </summary>
    public class EnquiryCreateDto
    {
        /// <summary>Omit when creating a new customer.</summary>
        public int? cust_id { get; set; }

        /// <summary>Provide when creating a new customer inline.</summary>
        public CustomerCreateDto? new_customer { get; set; }

        [Required(ErrorMessage = "NPI category is required")]
        [StringLength(100)]
        public string npi_category { get; set; } = string.Empty;

        /// <summary>
        /// Dynamic field answers keyed by section_key → field_key.
        /// Example: { "generalInfo": { "company_name": "Acme", "color": "Red" } }
        /// </summary>
        public Dictionary<string, Dictionary<string, string?>>? field_values { get; set; }

        /// <summary>Customer reference data — always persisted separately.</summary>
        public CustomerRefDto? CustomerRef { get; set; }
    }

    /// <summary>
    /// Full enquiry response including dynamic field values and customer reference.
    /// </summary>
    public class EnquiryResponseDto
    {
        public int enquiry_id { get; set; }
        public string enquiry_no { get; set; } = string.Empty;
        public int? proj_id { get; set; }
        public string? proj_no { get; set; }
        public int cust_id { get; set; }
        public string? customer_name { get; set; }
        public string npi_category { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public int created_by { get; set; }
        public string? username { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? submitted_at { get; set; }

        /// <summary>
        /// Dynamic field answers: section_key → { field_key → value }.
        /// The frontend consumes this directly — no mapping required.
        /// </summary>
        public Dictionary<string, Dictionary<string, string?>> field_values { get; set; } = new();

        /// <summary>Customer reference (mould ownership etc.) — preserved as-is.</summary>
        public CustomerRefResponseDto? CustomerRef { get; set; }

        public List<FileResponseDto>? Files { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CUSTOMER DTOs
    // ══════════════════════════════════════════════════════════════════════════

    public class CustomerCreateDto
    {
        [Required]
        [StringLength(200)]
        public string comp_name { get; set; } = string.Empty;

        public string? cust_addr { get; set; }

        [StringLength(100)]
        public string? contact_name { get; set; }

        [StringLength(100)]
        public string? contact_email { get; set; }

        [StringLength(50)]
        public string? contact_phone { get; set; }
    }

    public class CustomerRefDto
    {
        public string? mould_ownership { get; set; }
    }

    public class CustomerRefResponseDto
    {
        public int enquiry_id { get; set; }
        public string? mould_ownership { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  NPI FORM CONFIG DTOs (Categories, Sections, Fields)
    // ══════════════════════════════════════════════════════════════════════════

    public class NpiFormConfigResponseDto
    {
        public List<NpiCategoryDto> categories { get; set; } = new();
        public List<NpiFormSectionDto> sections { get; set; } = new();
    }

    public class NpiCategoryDto
    {
        public int category_id { get; set; }
        public string category_name { get; set; } = string.Empty;
        public int display_order { get; set; }
        public bool is_active { get; set; }
    }

    public class UpsertNpiCategoryDto
    {
        [Required]
        [StringLength(200)]
        public string category_name { get; set; } = string.Empty;

        public int display_order { get; set; } = 0;
        public bool is_active { get; set; } = true;
    }

    // ── Sections ──────────────────────────────────────────────────────────────

    public class NpiFormSectionDto
    {
        public int section_id { get; set; }
        public string section_key { get; set; } = string.Empty;
        public string section_label { get; set; } = string.Empty;
        public string? trigger_keywords { get; set; }
        public int display_order { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public List<NpiFormFieldDto> fields { get; set; } = new();
    }

    public class CreateNpiFormSectionDto
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

    public class UpdateNpiFormSectionDto
    {
        [Required(ErrorMessage = "Section label is required")]
        [StringLength(200)]
        public string section_label { get; set; } = string.Empty;

        [StringLength(500)]
        public string? trigger_keywords { get; set; }

        public int display_order { get; set; }
        public bool is_active { get; set; }
    }

    // ── Fields ────────────────────────────────────────────────────────────────

    public class NpiFormFieldDto
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

    public class UpsertNpiFormFieldDto
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