using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    public class NpiCategory
    {
        [Key]
        public int category_id { get; set; }

        [Required, StringLength(200)]
        public string category_name { get; set; } = string.Empty;

        public int display_order { get; set; } = 0;
        public bool is_active { get; set; } = true;
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }
    }

    public class NpiFormSection
    {
        [Key]
        public int section_id { get; set; }

        [Required, StringLength(100)]
        public string section_key { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string section_label { get; set; } = string.Empty;

        [StringLength(500)]
        public string? trigger_keywords { get; set; }

        public int display_order { get; set; } = 0;
        public bool is_active { get; set; } = true;
        public virtual ICollection<NpiFormField>? Fields { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }
    }

    public class NpiFormField
    {
        [Key]
        public int field_id { get; set; }

        public int section_id { get; set; }

        [Required, StringLength(100)]
        public string field_key { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string field_label { get; set; } = string.Empty;

        [StringLength(50)]
        public string field_type { get; set; } = "text";

        public string? options { get; set; }

        public bool is_required { get; set; } = false;
        public bool is_active { get; set; } = true;
        public int display_order { get; set; } = 0;

        [ForeignKey("section_id")]
        public virtual NpiFormSection? Section { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }
    }
}