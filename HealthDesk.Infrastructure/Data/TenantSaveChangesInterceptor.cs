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

        // During seeding or migration, there's no HTTP context - use tenant 1
        int tenantId = 1;

        if (httpContext != null)
        {
            // Try to get from session
            var sessionTenant = httpContext.Session.GetInt32("TenantId");
            if (sessionTenant.HasValue)
            {
                tenantId = sessionTenant.Value;
            }
            else if (httpContext.User.IsInRole("SuperAdmin"))
            {
                // SuperAdmin defaults to tenant 1
                tenantId = 1;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == 0)
            {
                entry.Entity.TenantId = tenantId;
            }
        }
    }
}
