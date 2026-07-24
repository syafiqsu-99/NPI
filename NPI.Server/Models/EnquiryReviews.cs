using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class EnquiryReviews
    {
        [Key]
        public int review_id { get; set; }

        public int enquiry_id { get; set; }
        public int revision_no { get; set; }
        public int reviewed_by { get; set; }

        [Required]
        [StringLength(50)]
        public string decision { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? remark { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        [ForeignKey("enquiry_id")]
        public virtual Enquiries? Enquiry { get; set; }

        [ForeignKey("reviewed_by")]
        public virtual Users? ReviewedByUser { get; set; }
    }
}
