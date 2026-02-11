namespace NPI.Server.Services
{
    public interface IGanttService
    {
        Task<GanttDataDto> GetGanttDataAsync(int projectId, int? revisionId = null);
        Task<List<ProjectRevisionDto>> GetProjectRevisionsAsync(int projectId);
        Task<(bool success, string message, int? revisionId)> CreateRevisionAsync(
            int projectId, CreateRevisionDto dto, int userId);
    }

    public class GanttDataDto
    {
        public ProjectResponseDto project { get; set; }
        public int? current_revision_id { get; set; }
        public int? revision_number { get; set; }
        public List<GanttTaskDto> tasks { get; set; }
    }

    public class GanttTaskDto
    {
        public int task_id { get; set; }
        public string title { get; set; }
        public string? dept_name { get; set; }
        public string? dept_color { get; set; }
        public DateOnly? start_date { get; set; }
        public DateOnly? end_date { get; set; }
        public float? duration { get; set; }
        public float? per_complete { get; set; }
        public string? status { get; set; }
        public int? parent_task_id { get; set; }
    }

    public class ProjectRevisionDto
    {
        public int revision_id { get; set; }
        public int revision_number { get; set; }
        public DateTime revision_date { get; set; }
        public string? revised_by_name { get; set; }
        public string? revision_notes { get; set; }
        public DateOnly? previous_target_date { get; set; }
        public DateOnly? new_target_date { get; set; }
        public bool is_active { get; set; }
    }

    public class CreateRevisionDto
    {
        public string revision_notes { get; set; }
        public List<TaskUpdateDto> tasks { get; set; }
    }
}
