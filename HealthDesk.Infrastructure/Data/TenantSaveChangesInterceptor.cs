//using HealthDesk.Application.Interfaces;
//using HealthDesk.Domain.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Diagnostics;

//namespace HealthDesk.Infrastructure.Data;

//public class TenantSaveChangesInterceptor : SaveChangesInterceptor
//{
//    private readonly ITenantService _tenantService;

//    public TenantSaveChangesInterceptor(ITenantService tenantService)
//    {
//        _tenantService = tenantService;
//    }

//    public override InterceptionResult<int> SavingChanges(
//        DbContextEventData eventData,
//        InterceptionResult<int> result)
//    {
//        SetTenantId(eventData.Context);
//        return base.SavingChanges(eventData, result);
//    }

//    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
//        DbContextEventData eventData,
//        InterceptionResult<int> result,
//        CancellationToken cancellationToken = default)
//    {
//        SetTenantId(eventData.Context);
//        return base.SavingChangesAsync(eventData, result, cancellationToken);
//    }

//    private void SetTenantId(DbContext? context)
//    {
//        if (context == null) return;

//        var tenantId = _tenantService.GetCurrentTenantId();

//        foreach (var entry in context.ChangeTracker.Entries<TenantEntity>())
//        {
//            if (entry.State == EntityState.Added)
//            {
//                entry.Entity.TenantId = tenantId;
//            }
//        }
//    }
//}


using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HealthDesk.Infrastructure.Data;

public class TenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetTenantId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetTenantId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetTenantId(DbContext? context)
    {
        if (context == null) return;

        var httpContext = _httpContextAccessor.HttpContext;

        // During seeding / migration — no HTTP context, use tenant 1
        int tenantId = 1;

        if (httpContext != null && httpContext.User.Identity?.IsAuthenticated == true)
        {
            // SuperAdmin saving while impersonating a tenant?
            // Check session first (SuperAdmin may have switched tenant)
            var sessionTenant = httpContext.Session.GetInt32("TenantId");
            if (sessionTenant.HasValue)
            {
                tenantId = sessionTenant.Value;
                Console.WriteLine($"[Interceptor] ✅ TenantId from session: {tenantId}");
            }
            else
            {
                // Read from TenantId claim (set at login for regular users)
                var tenantClaim = httpContext.User.FindFirst("TenantId");
                if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var claimTenantId))
                {
                    tenantId = claimTenantId;
                    Console.WriteLine($"[Interceptor] ✅ TenantId from claim: {tenantId}");
                }
                else if (httpContext.User.IsInRole("SuperAdmin"))
                {
                    // SuperAdmin writing platform-level data — use 0
                    // TenantEntity additions will be skipped (TenantId stays as-is)
                    Console.WriteLine($"[Interceptor] SuperAdmin with no tenant context — skipping TenantId assignment");
                    return;
                }
                else
                {
                    Console.WriteLine($"[Interceptor] ⚠️ No TenantId found for {httpContext.User.Identity.Name}, defaulting to 1");
                }
            }
        }
        else
        {
            Console.WriteLine($"[Interceptor] Seeding/migration mode — defaulting TenantId to {tenantId}");
        }

        // Apply TenantId only to newly added TenantEntity records with no TenantId set
        foreach (var entry in context.ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == 0)
            {
                entry.Entity.TenantId = tenantId;
                Console.WriteLine($"[Interceptor] ✅ Set TenantId={tenantId} on {entry.Entity.GetType().Name}");
            }
        }
    }
}