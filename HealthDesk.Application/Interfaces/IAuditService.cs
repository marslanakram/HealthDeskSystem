using HealthDesk.Application.DTOs;

namespace HealthDesk.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, int? entityId,
        object? oldValues, object? newValues, string? changes = null);

    Task<IEnumerable<AuditLogDto>> GetAllAsync();
    Task<IEnumerable<AuditLogDto>> GetByUserAsync(string userId);
    Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, int entityId);
    Task<IEnumerable<AuditLogDto>> GetRecentAsync(int count = 50);
}
