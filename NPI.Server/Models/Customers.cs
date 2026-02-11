using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class Customers
    {
        [Key]
        public int cust_id { get; set; }

        [Required]
        [StringLength(200)]
        public string comp_name { get; set; }

        public string? cust_addr { get; set; }

        [StringLength(100)]
        public string? contact_name { get; set; }

        [StringLength(100)]
        public string? contact_email { get; set; }

        [StringLength(50)]
        public string? contact_phone { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
        public bool is_active { get; set; } = true;

        public virtual ICollection<Projects>? Projects { get; set; }
    }
}
