using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class DocumentTypes
    {
        [Key]
        public int doc_type_id { get; set; }

        [Required]
        [StringLength(100)]
        public string type_name { get; set; }

        public int? dept_id { get; set; }
        public bool is_required { get; set; } = false;

        [StringLength(200)]
        public string? description { get; set; }

        [ForeignKey("dept_id")]
        public virtual Departments? Department { get; set; }

        public virtual ICollection<Files>? Files { get; set; }
    }
}
