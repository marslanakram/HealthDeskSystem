using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    

    public AuditLogRepository(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor)
        : base(context)
    {
        _httpContextAccessor = httpContextAccessor;
    }

   

    public override async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        var isSuperAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("SuperAdmin") == true;

        if (isSuperAdmin)
        {
            // SuperAdmin: Return ALL audit logs (bypass tenant filter)
            return await _dbSet
                .IgnoreQueryFilters()  // ← Bypass tenant filter
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
        else
        {
            // Regular users: Tenant filter applied automatically
            return await _dbSet
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count)
    {
        var isSuperAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("SuperAdmin") == true;

        if (isSuperAdmin)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }
        else
        {
            return await _dbSet
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId)
    {
        var isSuperAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("SuperAdmin") == true;

        if (isSuperAdmin)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
        else
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
    {
        var isSuperAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("SuperAdmin") == true;

        if (isSuperAdmin)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
        else
        {
            return await _dbSet
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(
        DateTime start, DateTime end)
    {
        var isSuperAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("SuperAdmin") == true;

        if (isSuperAdmin)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .Where(a => a.Timestamp >= start && a.Timestamp <= end)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
        else
        {
            return await _dbSet
                .Where(a => a.Timestamp >= start && a.Timestamp <= end)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}
