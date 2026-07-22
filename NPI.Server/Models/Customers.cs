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

        public DateTime created_at { get; set; } = DateTime.Now;
        public bool is_active { get; set; } = true;

        public virtual ICollection<Projects>? Projects { get; set; }
    }
}
