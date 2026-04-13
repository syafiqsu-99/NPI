namespace NPI.Server.DTOs
{
    public class ProjectTeamDto
    {
        public int team_id { get; set; }
        public int proj_id { get; set; }
        public int user_id { get; set; }
        public string? role { get; set; }
        public string? user_name { get; set; }
        public string? proj_name { get; set; }
        public DateTime created_at { get; set; }
    }
}