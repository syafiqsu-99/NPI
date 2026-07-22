using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class CustomerDto
    {
        public int cust_id { get; set; }
        public string comp_name { get; set; } = string.Empty;
        public DateTime created_at { get; set; }
        public bool is_active { get; set; }
    }

    public class CustomerCreateDto
    {
        [Required]
        [StringLength(200)]
        public string comp_name { get; set; } = string.Empty;

        public bool is_active { get; set; } = true;
    }

    public class CustomerUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string comp_name { get; set; } = string.Empty;

        public bool is_active { get; set; }
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
}