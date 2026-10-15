using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace HealthDesk.Application.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        IUnitOfWork uow,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _uow = uow;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        string action,
        string entityName,
        int? entityId,
        object? oldValues,
        object? newValues,
        string? changes = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var tenantIdClaim = user?.FindFirst("TenantId")?.Value;
        int? tenantId = string.IsNullOrEmpty(tenantIdClaim)
            ? null
            : int.Parse(tenantIdClaim);

        var auditLog = new AuditLog
        {
            TenantId = tenantId,
            UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System",
            UserName = user?.Identity?.Name ?? "System",
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues is not null
                ? JsonSerializer.Serialize(oldValues)
                : null,
            NewValues = newValues is not null
                ? JsonSerializer.Serialize(newValues)
                : null,
            Changes = changes,
            Timestamp = DateTime.UtcNow,
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString()
        };

        await _uow.AuditLogs.AddAsync(auditLog);
        await _uow.SaveChangesAsync();
    }

    //public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
    //{
    //    var logs = await _uow.AuditLogs.GetAllAsync();
    //    return _mapper.Map<IEnumerable<AuditLogDto>>(
    //        logs.OrderByDescending(l => l.Timestamp));
    //}
    public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var isSuperAdmin = httpContext?.User.IsInRole("SuperAdmin") == true;

        IEnumerable<AuditLog> logs;

        if (isSuperAdmin)
        {
            // SuperAdmin sees ALL audit logs (bypass tenant filter)
            // We need to temporarily disable the filter
            logs = await _uow.AuditLogs.GetAllAsync();
        }
        else
        {
            // Regular users only see their tenant's logs (filter applied automatically)
            logs = await _uow.AuditLogs.GetAllAsync();
        }

        var result = _mapper.Map<List<AuditLogDto>>(
            logs.OrderByDescending(l => l.Timestamp).ToList());

        return result;
    }


    public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(string userId)
    {
        var logs = await _uow.AuditLogs.FindAsync(l => l.UserId == userId);
        return _mapper.Map<IEnumerable<AuditLogDto>>(
            logs.OrderByDescending(l => l.Timestamp));
    }

    public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(
        string entityName, int entityId)
    {
        var logs = await _uow.AuditLogs.FindAsync(
            l => l.EntityName == entityName && l.EntityId == entityId);
        return _mapper.Map<IEnumerable<AuditLogDto>>(
            logs.OrderByDescending(l => l.Timestamp));
    }

    public async Task<IEnumerable<AuditLogDto>> GetRecentAsync(int count = 50)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var isSuperAdmin = httpContext?.User.IsInRole("SuperAdmin") == true;

        IEnumerable<AuditLog> logs;

        if (isSuperAdmin)
        {
            // SuperAdmin sees ALL logs across all tenants
            logs = await _uow.AuditLogs.GetAllAsync();
        }
        else
        {
            // Regular users see only their tenant's logs
            logs = await _uow.AuditLogs.GetAllAsync();
        }

        var result = _mapper.Map<List<AuditLogDto>>(
            logs.OrderByDescending(l => l.Timestamp).Take(count).ToList());

        return result;
    }

    //public async Task<IEnumerable<AuditLogDto>> GetRecentAsync(int count = 50)
    //{
    //    var logs = await _uow.AuditLogs.GetAllAsync();
    //    return _mapper.Map<IEnumerable<AuditLogDto>>(
    //        logs.OrderByDescending(l => l.Timestamp).Take(count));
    //}
}
