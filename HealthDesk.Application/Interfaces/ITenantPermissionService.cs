namespace HealthDesk.Application.Interfaces;

public interface ITenantPermissionService
{
    Task<List<string>> GetPermissionsForRoleAsync(int tenantId, string roleName);
    Task SavePermissionsForRoleAsync(int tenantId, string roleName, List<string> permissions);
    Task<bool> TenantHasPermissionAsync(int tenantId, string roleName, string permission);
    Task SeedDefaultPermissionsAsync(int tenantId);
}
