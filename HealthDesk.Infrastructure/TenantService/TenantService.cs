using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Infrastructure.TenantService;

public class TenantService : ITenantService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _currentTenantId;
    private Tenant? _currentTenant;

    public TenantService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    // ── Tenant CRUD ───────────────────────────────────────────────

    public async Task<Tenant?> GetTenantByIdentifierAsync(string identifier)
    {
        return await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Identifier == identifier && t.IsActive);
    }

    public async Task<Tenant?> GetTenantByIdAsync(int id)
    {
        return await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
    {
        return await _context.Tenants
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, int TenantId)> CreateTenantAsync(
        string name, string identifier)
    {
        var exists = await _context.Tenants
            .AnyAsync(t => t.Identifier == identifier);

        if (exists)
            return (false, "A tenant with this identifier already exists.", 0);

        var tenant = new Tenant
        {
            Name = name,
            Identifier = identifier.ToLower().Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            SubscriptionTier = "Basic",
            MaxUsers = 50,
            MaxPatients = 1000
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return (true, "Tenant created successfully.", tenant.Id);
    }

    public async Task<(bool Success, string Message)> UpdateTenantAsync(int id, string name)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return (false, "Tenant not found.");

        tenant.Name = name;
        await _context.SaveChangesAsync();

        return (true, "Tenant updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteTenantAsync(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
            return (false, "Tenant not found.");

        tenant.IsActive = false;
        await _context.SaveChangesAsync();

        return (true, "Tenant deleted successfully.");
    }

    // ── Tenant Resolution ─────────────────────────────────────────

    /// <summary>
    /// Returns the current tenant ID.
    /// Returns null for SuperAdmin with no tenant selected (meaning: see all).
    /// Returns 0 when no valid tenant could be resolved (safe — filters all data).
    /// </summary>
    public int GetCurrentTenantId()
    {
        var resolved = ResolveCurrentTenantId();
        return resolved ?? 0;
    }

    /// <summary>
    /// Internal: returns null for SuperAdmin (skip filter),
    /// or the actual tenantId for regular users.
    /// </summary>
    public int? ResolveCurrentTenantId()
    {
        if (_currentTenantId.HasValue)
            return _currentTenantId.Value;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null; // Migration / seeding — no filter

        var isSuperAdmin = httpContext.User.IsInRole("SuperAdmin");

        // ── SuperAdmin: check if they've switched to a specific tenant ──
        if (isSuperAdmin)
        {
            var sessionTenant = httpContext.Session.GetInt32("TenantId");
            if (sessionTenant.HasValue)
            {
                _currentTenantId = sessionTenant.Value;
                return _currentTenantId.Value;
            }
            // SuperAdmin with no selected tenant → null = see all data
            return null;
        }

        // ── Regular users: check session first ───────────────────
        var sessionVal = httpContext.Session.GetInt32("TenantId");
        if (sessionVal.HasValue)
        {
            _currentTenantId = sessionVal.Value;
            return _currentTenantId.Value;
        }

        // ── Then check TenantId claim (set during login) ──────────
        var tenantClaim = httpContext.User.FindFirst("TenantId");
        if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var claimTenantId))
        {
            _currentTenantId = claimTenantId;
            httpContext.Session.SetInt32("TenantId", claimTenantId);
            return claimTenantId;
        }

        // ── No tenant resolved for a non-SuperAdmin — return 0 (safe) ──
        // This means the query filter will produce no results,
        // which is safer than leaking another tenant's data.
        return 0;
    }

    public Tenant? GetCurrentTenant()
    {
        if (_currentTenant != null)
            return _currentTenant;

        var tenantId = ResolveCurrentTenantId();
        if (tenantId == null || tenantId == 0)
            return null;

        _currentTenant = GetTenantByIdAsync(tenantId.Value).GetAwaiter().GetResult();
        return _currentTenant;
    }

    public void SetCurrentTenant(int tenantId)
    {
        _currentTenantId = tenantId;
        _currentTenant = null; // Force reload on next access

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
            httpContext.Session.SetInt32("TenantId", tenantId);
    }
}