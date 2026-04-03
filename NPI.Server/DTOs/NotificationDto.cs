namespace NPI.Server.DTOs
{
    public class NotificationDto
    {
        public int notif_id { get; set; }
        public string? type { get; set; }
        public string? title { get; set; }
        public string? body { get; set; }
        public bool is_read { get; set; }
        public int? proj_id { get; set; }
        public int? task_id { get; set; }
        public DateTime created_at { get; set; }
    }
}