using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    /// <summary>
    /// Stores all dynamic enquiry form values.
    /// Replaces EnquiryGeneralInfo and EnquirySealInfo tables.
    /// One row per field answer, keyed by section_key + field_key from NpiFormFields.
    /// </summary>
    public class EnquiryFieldValues
    {
        [Key]
        public int value_id { get; set; }

        public int enquiry_id { get; set; }

        /// <summary>Maps to NpiFormSection.section_key (e.g. "generalInfo")</summary>
        [Required]
        [StringLength(100)]
        public string section_key { get; set; } = string.Empty;

        /// <summary>Maps to NpiFormField.field_key (e.g. "company_name")</summary>
        [Required]
        [StringLength(100)]
        public string field_key { get; set; } = string.Empty;

        /// <summary>User-entered value, always stored as string for flexibility.</summary>
        public string? field_value { get; set; }

        public DateTime updated_at { get; set; } = DateTime.Now;

        [ForeignKey("enquiry_id")]
        public virtual Enquiries? Enquiry { get; set; }
    }
}