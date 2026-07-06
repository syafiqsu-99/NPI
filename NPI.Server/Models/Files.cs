using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    public class Files
    {
        [Key]
        public int file_id { get; set; }
        public int? proj_id { get; set; }
        public int? task_id { get; set; }
        public int? enquiry_id { get; set; }
        public int? doc_type_id { get; set; }

        public int file_version { get; set; } = 1;
        public int upload_by { get; set; }
        public int? dept_id { get; set; }

        [Required]
        [StringLength(255)]
        public string file_name { get; set; }

        [Required]
        [StringLength(500)]
        public string file_path { get; set; }

        public long file_size { get; set; }

        public DateTime? updated_at { get; set; }

        [StringLength(100)]
        public string? content_type { get; set; }

        [StringLength(50)]
        public string? status { get; set; }

        public bool is_latest { get; set; } = true;
        public int? replaced_by { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public int? deleted_by { get; set; }
        public DateTime? deleted_at { get; set; }

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("task_id")]
        public virtual Tasks? Task { get; set; }

        [ForeignKey("enquiry_id")]
        public virtual Enquiries? Enquiry { get; set; }

        [ForeignKey("doc_type_id")]
        public virtual DocumentTypes? DocumentType { get; set; }

        [ForeignKey("upload_by")]
        public virtual Users? UploadByUser { get; set; }

        [ForeignKey("dept_id")]
        public virtual Departments? Department { get; set; }

        [ForeignKey("replaced_by")]
        public virtual Files? ReplacedByFile { get; set; }
    }
}
