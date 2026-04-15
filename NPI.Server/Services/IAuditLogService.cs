namespace NPI.Server.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(
            int? userId,
            int? projId,
            string action,
            string tableName,
            int? recordId,
            object? oldValue,
            object? newValue,
            string? ipAddress = null);
    }
}
