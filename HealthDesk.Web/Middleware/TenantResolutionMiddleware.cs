using HealthDesk.Application.Interfaces;
using HealthDesk.Infrastructure.Data;

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
        // Get current tenant ID from TenantService
        var tenantId = tenantService.GetCurrentTenantId();

        // Set it on the DbContext for this request
        dbContext.SetCurrentTenant(tenantId);

        Console.WriteLine($"[TenantMiddleware] Request from Tenant: {tenantId} | Path: {context.Request.Path}");

        await _next(context);
    }
}

// Extension method to register the middleware
public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}