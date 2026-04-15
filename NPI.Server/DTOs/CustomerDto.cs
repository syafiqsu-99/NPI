using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class CustomerDto
    {
        public int cust_id { get; set; }
        public string comp_name { get; set; } = string.Empty;
        public string? cust_addr { get; set; }
        public string? contact_name { get; set; }
        public string? contact_email { get; set; }
        public string? contact_phone { get; set; }
        public DateTime created_at { get; set; }
        public bool is_active { get; set; }
    }

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

        public bool is_active { get; set; } = true;
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

    public class CustomerUpdateDto
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

        public bool is_active { get; set; }
    }
}
