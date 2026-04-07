using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    /// <summary>
    /// Core enquiry entity. Dynamic field answers now live in EnquiryFieldValues.
    /// EnquiryCustomerRef is preserved as a separate table per business requirements.
    /// Legacy EnquiryGeneralInfo and EnquirySealInfo navigation properties are removed
    /// after the data migration SQL script has been run.
    /// </summary>
    public class Enquiries
    {
        [Key]
        public int enquiry_id { get; set; }

        [Required]
        [StringLength(20)]
        public string enquiry_no { get; set; } = string.Empty;

        public int? proj_id { get; set; }
        public int cust_id { get; set; }

        [Required]
        [StringLength(100)]
        public string npi_category { get; set; } = string.Empty;

        [StringLength(50)]
        public string status { get; set; } = "Draft";

        public int created_by { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }
        public int? updated_by { get; set; }
        public DateTime? submitted_at { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("created_by")]
        public virtual Users? CreatedByUser { get; set; }

        [ForeignKey("cust_id")]
        public virtual Customers? Customer { get; set; }

        /// <summary>Dynamic form answers — the new system.</summary>
        public virtual ICollection<EnquiryFieldValues>? FieldValues { get; set; }

        /// <summary>
        /// Customer reference info (mould ownership, etc.).
        /// Preserved as a distinct table per business requirement.
        /// </summary>
        public virtual EnquiryCustomerRef? CustomerRef { get; set; }

        public virtual ICollection<Files>? Files { get; set; }
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