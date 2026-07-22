using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    public class EnquiryFieldValues
    {
        [Key]
        public int value_id { get; set; }

        public int enquiry_id { get; set; }

        [Required]
        [StringLength(100)]
        public string section_key { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string field_key { get; set; } = string.Empty;
        public string? field_value { get; set; }

        public DateTime updated_at { get; set; } = DateTime.Now;

        [ForeignKey("enquiry_id")]
        public virtual Enquiries? Enquiry { get; set; }
    }
}