using HealthDesk.Application.Interfaces;
using HealthDesk.Infrastructure.Data;
using HealthDesk.Infrastructure.TenantService;

namespace HealthDesk.Web.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantService tenantService,
        AppDbContext dbContext)
    {
        // Cast to concrete type to access ResolveCurrentTenantId()
        // which returns null for SuperAdmin (no filter) vs 0 for unresolved (safe filter)
        var resolvedTenantId = tenantService is TenantService ts
            ? ts.ResolveCurrentTenantId()
            : (int?)tenantService.GetCurrentTenantId();

        // Set on DbContext — null means SuperAdmin sees all, 0 means no data (safe default)
        dbContext.SetCurrentTenant(resolvedTenantId);

        Console.WriteLine(
            resolvedTenantId == null
                ? $"[TenantMiddleware] SuperAdmin mode — no tenant filter | Path: {context.Request.Path}"
                : $"[TenantMiddleware] TenantId: {resolvedTenantId} | Path: {context.Request.Path}");

        await _next(context);
    }
}

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}