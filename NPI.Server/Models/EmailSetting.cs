namespace NPI.Server.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool UseAuthentication { get; set; }
        public bool EnableSsl { get; set; }
    }
}
