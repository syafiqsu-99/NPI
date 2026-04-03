// NPI.Server/Models/TaskDocumentRequirements.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NPI.Server.Models
{
    public class TaskDocumentRequirements
    {
        [Key]
        public int req_id { get; set; }

        public int task_id { get; set; }

        public int doc_type_id { get; set; }

        public bool is_fulfilled { get; set; } = false;

        public DateTime? fulfilled_at { get; set; }

        public int? file_id { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        [ForeignKey("task_id")]
        public virtual Tasks? Task { get; set; }

        [ForeignKey("doc_type_id")]
        public virtual DocumentTypes? DocumentType { get; set; }

        [ForeignKey("file_id")]
        public virtual Files? File { get; set; }
    }
}