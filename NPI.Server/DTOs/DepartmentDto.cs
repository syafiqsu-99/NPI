namespace NPI.Server.DTOs
{
    public class CreateDepartmentDto
    {
        public string dept_name { get; set; }
        public string? description { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string dept_name { get; set; }
        public string? description { get; set; }
    }
    public class DepartmentResponseDto
    {
        public int dept_id { get; set; }
        public string dept_name { get; set; }
        public string? description { get; set; }
        public DateTime created_at { get; set; }
    }
}
