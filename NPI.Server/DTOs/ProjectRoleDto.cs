namespace NPI.Server.DTOs
{
    public class ProjectRoleDto
    {
        public int proj_role_id { get; set; }
        public int proj_id { get; set; }
        public int user_id { get; set; }
        public string? username { get; set; }
        public string? full_name { get; set; }
        public string role_name { get; set; } = "Member";
        public DateTime created_at { get; set; }
    }

    public class UpsertProjectRoleDto
    {
        public int user_id { get; set; }
        public string role_name { get; set; } = "Member";
    }
}
