using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Models
{
    public class ProjectTeams
    {
        [Key]
        public int team_id { get; set; }

        public int proj_id { get; set; }
        public int user_id { get; set; }

        [StringLength(50)]
        public string? role { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
        public int? assigned_by { get; set; }

        [ForeignKey("proj_id")]
        public virtual Projects? Project { get; set; }

        [ForeignKey("user_id")]
        public virtual Users? User { get; set; }

        [ForeignKey("assigned_by")]
        public virtual Users? AssignedByUser { get; set; }
    }
}
