using HealthDesk.Domain.Entities;

namespace HealthDesk.Application.Interfaces;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count);
    Task<IEnumerable<AuditLog>> GetByUserAsync(string userId);
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime start, DateTime end);
}
