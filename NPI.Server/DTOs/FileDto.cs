namespace NPI.Server.DTOs
{
    public class FileResponseDto
    {
        public int file_id { get; set; }
        public int proj_id { get; set; }
        public int? task_id { get; set; }
        public int? enquiry_id { get; set; }
        public string? task_name { get; set; }
        public string? dept_name { get; set; }
        public string file_name { get; set; }
        public string file_path { get; set; }
        public long file_size { get; set; }
        public string? file_type { get; set; }
        public string? description { get; set; }
        public int uploaded_by { get; set; }
        public string? uploaded_by_name { get; set; }
        public DateTime uploaded_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int file_version { get; set; }
        public string? status { get; set; }
        public bool is_latest { get; set; }
    }

    public class DirectoryNodeDto
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string? path { get; set; }
        public long size { get; set; }
        public DateTime? uploaded_at { get; set; }
        public DateTime? updated_at { get; set; }
        public bool can_edit { get; set; }
        public int? file_id { get; set; }
        public int? file_version { get; set; }
        public string? content_type { get; set; }
        public List<DirectoryNodeDto>? children { get; set; }
    }
}