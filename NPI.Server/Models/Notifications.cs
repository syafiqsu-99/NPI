using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class Notifications
    {
        [Key]
        public int notif_id { get; set; }

        public int? proj_id { get; set; }
        public int user_id { get; set; }

        [StringLength(50)]
        public string? notif_type { get; set; }

        [StringLength(200)]
        public string? subject { get; set; }

        public string? body { get; set; }

        public DateTime? sent_at { get; set; }
        public DateTime? read_at { get; set; }

        [StringLength(50)]
        public string? status { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("user_id")]
        public virtual Users? User { get; set; }
    }
}
