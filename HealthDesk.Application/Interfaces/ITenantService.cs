using HealthDesk.Domain.Entities;

namespace HealthDesk.Application.Interfaces;

public interface ITenantService
{
    Task<Tenant?> GetTenantByIdentifierAsync(string identifier);
    Task<Tenant?> GetTenantByIdAsync(int id);
    Task<IEnumerable<Tenant>> GetAllTenantsAsync();

    Task<(bool Success, string Message, int TenantId)> CreateTenantAsync(
        string name, string identifier);

    Task<(bool Success, string Message)> UpdateTenantAsync(int id, string name);
    Task<(bool Success, string Message)> DeleteTenantAsync(int id);

    /// <summary>
    /// Returns the current tenant ID.
    /// Returns 0 when no tenant resolved (safe — filters all data).
    /// </summary>
    int GetCurrentTenantId();

    /// <summary>
    /// Returns null for SuperAdmin (skip filter), or tenantId for regular users.
    /// </summary>
    int? ResolveCurrentTenantId();

    Tenant? GetCurrentTenant();
    void SetCurrentTenant(int tenantId);
}
