namespace NPI.Server.DTOs
{
    public class TaskTemplateResponseDto
    {
        public int template_id { get; set; }
        public string stage_id { get; set; } = null!;
        public string stage_name { get; set; } = null!;
        public string task_code { get; set; } = null!;
        public string title { get; set; } = null!;
        public int dept_id { get; set; }
        public string? dept_name { get; set; }
        public int default_duration { get; set; }
        public bool has_link { get; set; }
        public string? doc_format { get; set; }
        public string? role_gated { get; set; }
        public int display_order { get; set; }
        public bool is_active { get; set; }
    }

    public class CreateTaskTemplateDto
    {
        public string stage_id { get; set; } = null!;
        public string task_code { get; set; } = null!;
        public string title { get; set; } = null!;
        public int dept_id { get; set; }
        public int default_duration { get; set; } = 5;
        public bool has_link { get; set; }
        public string? doc_format { get; set; }
        public string? role_gated { get; set; }
        public int display_order { get; set; }
    }

    public class UpdateTaskTemplateDto
    {
        public string? task_code { get; set; }
        public string? title { get; set; }
        public int? dept_id { get; set; }
        public int? default_duration { get; set; }
        public bool? has_link { get; set; }
        public string? doc_format { get; set; }
        public string? role_gated { get; set; }
        public int? display_order { get; set; }
        public bool? is_active { get; set; }
    }

    public class StageInfoDto
    {
        public string stage_id { get; set; } = null!;
        public string stage_name { get; set; } = null!;
        public bool is_required { get; set; }
        public bool auto_complete { get; set; }
        public int task_count { get; set; }
    }
}
