using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Constants;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HealthDesk.Infrastructure.TenantServices;

public class TenantPermissionService : ITenantPermissionService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    // Cache key pattern: "TenantPerms_{tenantId}_{roleName}"
    private static string CacheKey(int tenantId, string roleName)
        => $"TenantPerms_{tenantId}_{roleName}";

    public TenantPermissionService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<string>> GetPermissionsForRoleAsync(
        int tenantId, string roleName)
    {
        var key = CacheKey(tenantId, roleName);

        // Return from cache if available — avoids DB hit on every request
        if (_cache.TryGetValue(key, out List<string>? cached) && cached != null)
            return cached;

        var permissions = await _context.TenantRolePermissions
            .Where(p => p.TenantId == tenantId && p.RoleName == roleName)
            .Select(p => p.Permission)
            .ToListAsync();

        // Cache for 10 minutes — permissions don't change often
        _cache.Set(key, permissions, TimeSpan.FromMinutes(10));

        return permissions;
    }

    public async Task SavePermissionsForRoleAsync(
        int tenantId, string roleName, List<string> permissions)
    {
        // Remove existing permissions for this tenant+role
        var existing = await _context.TenantRolePermissions
            .Where(p => p.TenantId == tenantId && p.RoleName == roleName)
            .ToListAsync();

        _context.TenantRolePermissions.RemoveRange(existing);

        // Add new set
        foreach (var permission in permissions)
        {
            _context.TenantRolePermissions.Add(new TenantRolePermission
            {
                TenantId = tenantId,
                RoleName = roleName,
                Permission = permission
            });
        }

        await _context.SaveChangesAsync();

        // ── Invalidate cache after save ───────────────────────────
        // Critical: without this, old permissions would still be served
        // for up to 10 minutes after an admin changes them
        _cache.Remove(CacheKey(tenantId, roleName));
    }

    public async Task<bool> TenantHasPermissionAsync(
        int tenantId, string roleName, string permission)
    {
        // Use the cached list rather than hitting DB directly
        var permissions = await GetPermissionsForRoleAsync(tenantId, roleName);
        return permissions.Contains(permission);
    }

    public async Task SeedDefaultPermissionsAsync(int tenantId)
    {
        // Only seed if this tenant has no permissions yet
        var alreadySeeded = await _context.TenantRolePermissions
            .AnyAsync(p => p.TenantId == tenantId);

        if (alreadySeeded) return;

        var defaults = new Dictionary<string, string[]>
        {
            ["Admin"] = PermissionConstants.GetAllPermissions().ToArray(),

            ["Doctor"] = new[]
            {
                PermissionConstants.Patients.View,
                PermissionConstants.Appointments.View,
                PermissionConstants.Appointments.UpdateStatus,
                PermissionConstants.Prescriptions.View,
                PermissionConstants.Prescriptions.Create,
            },

            ["Receptionist"] = new[]
            {
                PermissionConstants.Patients.View,
                PermissionConstants.Patients.Create,
                PermissionConstants.Patients.Edit,
                PermissionConstants.Appointments.View,
                PermissionConstants.Appointments.Create,
                PermissionConstants.Appointments.Edit,
                PermissionConstants.Doctors.View,
                PermissionConstants.Departments.View,
            },

            ["Nurse"] = new[]
            {
                PermissionConstants.Patients.View,
                PermissionConstants.Appointments.View,
                PermissionConstants.Doctors.View,
                PermissionConstants.Departments.View,
            },

            ["Patient"] = new[]
            {
                PermissionConstants.Appointments.View,
                PermissionConstants.Prescriptions.View,
            },
        };

        var rows = new List<TenantRolePermission>();

        foreach (var (role, perms) in defaults)
        {
            foreach (var perm in perms)
            {
                rows.Add(new TenantRolePermission
                {
                    TenantId = tenantId,
                    RoleName = role,
                    Permission = perm
                });
            }
        }

        // Bulk insert — much faster than one-by-one Add
        await _context.TenantRolePermissions.AddRangeAsync(rows);
        await _context.SaveChangesAsync();
    }
}