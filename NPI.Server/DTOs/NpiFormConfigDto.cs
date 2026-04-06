using System.ComponentModel.DataAnnotations;

namespace NPI.Server.DTOs
{
    public class NpiCategoryDto
    {
        public int category_id { get; set; }
        public string category_name { get; set; } = string.Empty;
        public int display_order { get; set; }
        public bool is_active { get; set; }
    }

    public class UpsertNpiCategoryDto
    {
        [Required, StringLength(200)]
        public string category_name { get; set; } = string.Empty;
        public int display_order { get; set; } = 0;
        public bool is_active { get; set; } = true;
    }

    public class NpiFormFieldDto
    {
        public int field_id { get; set; }
        public int section_id { get; set; }
        public string section_key { get; set; } = string.Empty;
        public string section_label { get; set; } = string.Empty;
        public string field_key { get; set; } = string.Empty;
        public string field_label { get; set; } = string.Empty;
        public string field_type { get; set; } = "text";
        public List<string>? options { get; set; }
        public bool is_required { get; set; }
        public bool is_active { get; set; }
        public int display_order { get; set; }
    }

    public class UpsertNpiFormFieldDto
    {
        [Required]
        public int section_id { get; set; }

        [Required, StringLength(100)]
        public string field_key { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string field_label { get; set; } = string.Empty;

        [StringLength(50)]
        public string field_type { get; set; } = "text";

        public List<string>? options { get; set; }
        public bool is_required { get; set; } = false;
        public bool is_active { get; set; } = true;
        public int display_order { get; set; } = 0;
    }

    public class NpiFormConfigResponseDto
    {
        public List<NpiCategoryDto> categories { get; set; } = new();
        public List<NpiFormSectionDto> sections { get; set; } = new();
    }

    public class NpiFormSectionDto
    {
        public int section_id { get; set; }
        public string section_key { get; set; } = string.Empty;
        public string section_label { get; set; } = string.Empty;
        public string? trigger_keywords { get; set; }
        public List<NpiFormFieldDto> fields { get; set; } = new();
    }
}