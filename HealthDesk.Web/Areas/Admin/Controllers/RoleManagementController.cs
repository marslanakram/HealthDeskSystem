using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HealthDesk.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class RoleManagementController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITenantPermissionService _permissionService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<RoleManagementController> _logger;
    private readonly IAuditService _auditService;

    // These roles belong to the platform level — never shown or editable in tenant panel
    private static readonly string[] PlatformRoles = { "SuperAdmin" };

    public RoleManagementController(
        RoleManager<IdentityRole> roleManager,
        ITenantPermissionService permissionService,
        ITenantService tenantService,
        ILogger<RoleManagementController> logger,
        IAuditService auditService)
    {
        _roleManager = roleManager;
        _permissionService = permissionService;
        _tenantService = tenantService;
        _logger = logger;
        _auditService = auditService;
    }

    // ── GET: Admin/RoleManagement ─────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Roles";
        ViewData["Title"] = "Role Management";

        // Never expose platform-level roles in the tenant admin panel
        var roles = await _roleManager.Roles
            .Where(r => !PlatformRoles.Contains(r.Name))
            .ToListAsync();

        return View(roles);
    }

    // ── GET: Admin/RoleManagement/Permissions/roleId ──────────────
    public async Task<IActionResult> Permissions(string id)
    {
        ViewData["ActiveMenu"] = "Roles";
        ViewData["Title"] = "Manage Permissions";

        var role = await _roleManager.FindByIdAsync(id);

        // Block editing platform-level roles entirely
        if (role is null || PlatformRoles.Contains(role.Name))
        {
            TempData["Error"] = "Role not found or cannot be edited.";
            return RedirectToAction(nameof(Index));
        }

        // Get TENANT-SPECIFIC permissions for this role
        var tenantId = _tenantService.GetCurrentTenantId();
        var grantedPermissions = await _permissionService
            .GetPermissionsForRoleAsync(tenantId, role.Name!);

        var model = new RolePermissionsViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name!,
            AllPermissions = PermissionConstants.GetPermissionsByCategory(),
            GrantedPermissions = grantedPermissions
        };

        return View(model);
    }

    // ── POST: Admin/RoleManagement/UpdatePermissions ──────────────
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> UpdatePermissions(RolePermissionsViewModel model)
    //{
    //    var role = await _roleManager.FindByIdAsync(model.RoleId);

    //    // Double-check: block platform-level roles even via direct POST
    //    if (role is null || PlatformRoles.Contains(role.Name))
    //    {
    //        TempData["Error"] = "Role not found or cannot be edited from this panel.";
    //        return RedirectToAction(nameof(Index));
    //    }

    //    // Remove all existing permission claims
    //    var existingClaims = await _roleManager.GetClaimsAsync(role);

    //    var oldPermissions = existingClaims
    //        .Where(c => c.Type == "Permission")
    //        .Select(c => c.Value)
    //        .ToList();

    //    foreach (var claim in existingClaims.Where(c => c.Type == "Permission"))
    //        await _roleManager.RemoveClaimAsync(role, claim);

    //    // Add new permissions
    //    if (model.SelectedPermissions is not null)
    //    {
    //        foreach (var permission in model.SelectedPermissions)
    //        {
    //            await _roleManager.AddClaimAsync(role,
    //                new Claim("Permission", permission));
    //        }
    //    }

    //    // ── Audit Log ─────────────────────────────────────────────
    //    await _auditService.LogAsync(
    //        action: "PermissionChanged",
    //        entityName: "Role",
    //        entityId: null,
    //        oldValues: new { Permissions = oldPermissions },
    //        newValues: new { Permissions = model.SelectedPermissions },
    //        changes: $"Permissions updated for role '{role.Name}'"
    //    );

    //    TempData["Success"] = $"Permissions updated for role '{role.Name}'.";
    //    return RedirectToAction(nameof(Index));
    //}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermissions(RolePermissionsViewModel model)
    {
        var role = await _roleManager.FindByIdAsync(model.RoleId);
        if (role is null || PlatformRoles.Contains(role.Name))
        {
            TempData["Error"] = "Role not found or cannot be edited.";
            return RedirectToAction(nameof(Index));
        }

        var tenantId = _tenantService.GetCurrentTenantId();

        var oldPermissions = await _permissionService
            .GetPermissionsForRoleAsync(tenantId, role.Name!);

        // ── This now saves to TenantRolePermissions table ─────
        await _permissionService.SavePermissionsForRoleAsync(
            tenantId,
            role.Name!,
            model.SelectedPermissions ?? new List<string>());

        await _auditService.LogAsync(
            action: "PermissionChanged",
            entityName: "TenantRolePermission",
            entityId: null,
            oldValues: new { Permissions = oldPermissions },
            newValues: new { Permissions = model.SelectedPermissions },
            changes: $"Tenant {tenantId}: permissions updated for role '{role.Name}'");

        TempData["Success"] = $"Permissions updated for role '{role.Name}'.";
        return RedirectToAction(nameof(Index));
    }
}

// ── ViewModel ────────────────────────────────────────────────────
public class RolePermissionsViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public Dictionary<string, List<string>> AllPermissions { get; set; } = new();
    public List<string> GrantedPermissions { get; set; } = new();
    public List<string>? SelectedPermissions { get; set; }
}