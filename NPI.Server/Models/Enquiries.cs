using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class Enquiries
    {
        [Key]
        public int enquiry_id { get; set; }

        [Required]
        [StringLength(20)]
        public string enquiry_no { get; set; }
        public int? proj_id { get; set; }
        public int cust_id { get; set; }

        [Required]
        [StringLength(100)]
        public string npi_category { get; set; }

        [StringLength(50)]
        public string status { get; set; } = "Draft";
        public int created_by { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? submitted_at { get; set; }

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("created_by")]
        public virtual Users? CreatedByUser { get; set; }

        [ForeignKey("cust_id")]
        public virtual Customers? Customer { get; set; }

        public virtual EnquiryGeneralInfo? GeneralInfo { get; set; }
        public virtual EnquirySealInfo? SealInfo { get; set; }
        public virtual EnquiryCustomerRef? CustomerRef { get; set; }
        public virtual ICollection<Files>? Files { get; set; }
    }
    public class EnquiryGeneralInfo
    {
        [Key]

        public int enquiry_id { get; set; }

        [StringLength(200)]
        public string? company_name { get; set; }

        public int? estimated_qty_per_year { get; set; }

        public DateOnly? estimated_required_date { get; set; }

        [StringLength(100)]
        public string? color { get; set; }

        [StringLength(100)]
        public string? material_used { get; set; }

        public float? weight_g { get; set; }

        [StringLength(50)]
        public string? neck_size_mm { get; set; }

        [StringLength(100)]
        public string? shape { get; set; }

        [StringLength(50)]
        public string? hot_cold_filling { get; set; }

        public int? qty_first_submission { get; set; }

        [StringLength(200)]
        public string? cap_bottle_source { get; set; }

        [StringLength(200)]
        public string? filling_content { get; set; }

        [StringLength(100)]
        public string? capping_method { get; set; }

        [StringLength(100)]
        public string? cap_seal { get; set; }

        [ForeignKey("enquiry_id")]
        public virtual Enquiries? Enquiry { get; set; }
    }

    public class EnquirySealInfo
    {
        [Key]

        public int enquiry_id { get; set; }

        [StringLength(200)]
        public string? customer_name { get; set; }

        [StringLength(200)]
        public string? apply_to_product { get; set; }

        public DateOnly? estimated_required_date { get; set; }

        public string? reason_of_change { get; set; }

        public int? qty_first_submission { get; set; }

        public string? other_requirements { get; set; }

        [ForeignKey("enquiry_id")]
        public virtual Enquiries? Enquiry { get; set; }
    }

    public class EnquiryCustomerRef
    {
        [Key]
        public int enquiry_id { get; set; }

        [StringLength(100)]
        public string? mould_ownership { get; set; }

        [ForeignKey("enquiry_id")]
        public virtual Enquiries? Enquiry { get; set; }
    }
}
