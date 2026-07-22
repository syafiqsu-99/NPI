namespace NPI.Server.DTOs
{
    public class SessionInfoDto
    {
        public string session_id { get; set; } = string.Empty;
        public string? ip_address { get; set; }
        public string? user_agent { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? last_seen_at { get; set; }
        public DateTime expires_at { get; set; }
        public bool is_current { get; set; }
    }
}
