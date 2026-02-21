using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count)
        => await _dbSet
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId)
        => await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        => await _dbSet
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(
        DateTime start, DateTime end)
        => await _dbSet
            .Where(a => a.Timestamp >= start && a.Timestamp <= end)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
}
