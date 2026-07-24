using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
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
        public string form_category { get; set; } = string.Empty;

        [StringLength(50)]
        public string status { get; set; } = "Draft";
        public int revision_no { get; set; } = 1;
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

        public virtual ICollection<EnquiryFieldValues>? FieldValues { get; set; }

        public virtual EnquiryCustomerRef? CustomerRef { get; set; }

        public virtual ICollection<Files>? Files { get; set; }
        public virtual ICollection<EnquiryReviews>? Reviews { get; set; }
        public virtual ICollection<EnquiryRevisionSnapshots>? RevisionSnapshots { get; set; }
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