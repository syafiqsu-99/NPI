using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class EnquiryRevisionSnapshots
    {
        [Key]
        public int snapshot_id { get; set; }

        public int enquiry_id { get; set; }
        public int revision_no { get; set; }
        public int cust_id { get; set; }

        [StringLength(100)]
        public string form_category { get; set; } = string.Empty;

        public string field_values_json { get; set; } = "{}";

        public int submitted_by { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;

        [ForeignKey("enquiry_id")]
        public virtual Enquiries? Enquiry { get; set; }

        [ForeignKey("submitted_by")]
        public virtual Users? SubmittedByUser { get; set; }
    }
}
