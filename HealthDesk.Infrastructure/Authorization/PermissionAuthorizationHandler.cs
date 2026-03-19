//using Microsoft.AspNetCore.Authorization;
//using System.Security.Claims;

//namespace HealthDesk.Infrastructure.Authorization;

//public class PermissionRequirement : IAuthorizationRequirement
//{
//    public string Permission { get; }

//    public PermissionRequirement(string permission)
//    {
//        Permission = permission;
//    }
//}

//public class PermissionAuthorizationHandler
//    : AuthorizationHandler<PermissionRequirement>
//{
//    protected override Task HandleRequirementAsync(
//        AuthorizationHandlerContext context,
//        PermissionRequirement requirement)
//    {
//        // Check if user has the permission as a claim
//        if (context.User.HasClaim(c =>
//            c.Type == "Permission" && c.Value == requirement.Permission))
//        {
//            context.Succeed(requirement);
//        }

//        return Task.CompletedTask;
//    }
//}

using HealthDesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HealthDesk.Infrastructure.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly ITenantPermissionService _permissionService;

    public PermissionAuthorizationHandler(
        ITenantPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // SuperAdmin bypasses everything
        if (context.User.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
            return;
        }

        var tenantClaim = context.User.FindFirst("TenantId");
        if (tenantClaim == null || !int.TryParse(tenantClaim.Value, out var tenantId))
            return;

        var userRoles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        foreach (var role in userRoles)
        {
            var hasPermission = await _permissionService
                .TenantHasPermissionAsync(tenantId, role, requirement.Permission);

            if (hasPermission)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}