using HealthDesk.Domain.Entities;

namespace HealthDesk.Application.Interfaces;

public interface ITenantService
{
    Task<Tenant?> GetTenantByIdentifierAsync(string identifier);
    Task<Tenant?> GetTenantByIdAsync(int id);
    Task<IEnumerable<Tenant>> GetAllTenantsAsync();
    Task<(bool Success, string Message, int TenantId)> CreateTenantAsync(string name, string identifier);
    Task<(bool Success, string Message)> UpdateTenantAsync(int id, string name);
    Task<(bool Success, string Message)> DeleteTenantAsync(int id);

    int GetCurrentTenantId();
    Tenant? GetCurrentTenant();
    void SetCurrentTenant(int tenantId);
}
