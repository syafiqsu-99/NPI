using Microsoft.EntityFrameworkCore;
using NPI.Server.Data;
using NPI.Server.Models;
using System.Text.Json;

namespace NPI.Server.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(int? userId, int? projId, string action, string tableName, int? recordId, object? oldValue, object? newValue, string? ipAddress = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<AuditLogService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public AuditLogService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<AuditLogService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task LogAsync(
            int? userId,
            int? projId,
            string action,
            string tableName,
            int? recordId,
            object? oldValue,
            object? newValue,
            string? ipAddress = null)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var log = new AuditLogs
                {
                    user_id = userId,
                    proj_id = projId,
                    action = action[..Math.Min(action.Length, 50)],
                    table_name = tableName[..Math.Min(tableName.Length, 50)],
                    record_id = recordId,
                    old_value = oldValue is null
                                    ? null
                                    : JsonSerializer.Serialize(oldValue, _jsonOptions),
                    new_value = newValue is null
                                    ? null
                                    : JsonSerializer.Serialize(newValue, _jsonOptions),
                    ip_address = ipAddress?[..Math.Min(ipAddress.Length, 50)],
                    created_at = DateTime.Now
                };

                context.AuditLogs.Add(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Audit log failed for action={Action} table={Table} record={Record}",
                    action, tableName, recordId);
            }
        }
    }
}
