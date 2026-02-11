using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class AuditLogs
    {
        [Key]
        public int log_id { get; set; }

        public int? proj_id { get; set; }
        public int? user_id { get; set; }

        [StringLength(50)]
        public string? action { get; set; }

        [StringLength(50)]
        public string? table_name { get; set; }

        public int? record_id { get; set; }

        public string? old_value { get; set; }
        public string? new_value { get; set; }

        [StringLength(50)]
        public string? ip_address { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("user_id")]
        public virtual Users? User { get; set; }
    }
}
