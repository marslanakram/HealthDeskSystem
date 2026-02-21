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
    private readonly ILogger<RoleManagementController> _logger;
    private readonly IAuditService _auditService;

    public RoleManagementController(
        RoleManager<IdentityRole> roleManager,
        ILogger<RoleManagementController> logger,
        IAuditService auditService)
    {
        _roleManager = roleManager;
        _logger = logger;
        _auditService = auditService;
    }

    // ── GET: Admin/RoleManagement ─────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Roles";
        ViewData["Title"] = "Role Management";

        var roles = await _roleManager.Roles.ToListAsync();
        return View(roles);
    }

    // ── GET: Admin/RoleManagement/Permissions/roleId ──────────────
    public async Task<IActionResult> Permissions(string id)
    {
        ViewData["ActiveMenu"] = "Roles";
        ViewData["Title"] = "Manage Permissions";

        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            TempData["Error"] = "Role not found.";
            return RedirectToAction(nameof(Index));
        }

        var roleClaims = await _roleManager.GetClaimsAsync(role);
        var rolePermissions = roleClaims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

        var allPermissions = PermissionConstants.GetPermissionsByCategory();

        var model = new RolePermissionsViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name!,
            AllPermissions = allPermissions,
            GrantedPermissions = rolePermissions
        };

        return View(model);
    }

    // ── POST: Admin/RoleManagement/Permissions ────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermissions(RolePermissionsViewModel model)
    {
        var role = await _roleManager.FindByIdAsync(model.RoleId);
        if (role is null)
        {
            TempData["Error"] = "Role not found.";
            return RedirectToAction(nameof(Index));
        }

        // Remove all existing permission claims
        var existingClaims = await _roleManager.GetClaimsAsync(role);

        var oldPermissions = existingClaims
        .Where(c => c.Type == "Permission")
        .Select(c => c.Value)
        .ToList();

        foreach (var claim in existingClaims.Where(c => c.Type == "Permission"))
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Add new permissions
        if (model.SelectedPermissions is not null)
        {
            foreach (var permission in model.SelectedPermissions)
            {
                await _roleManager.AddClaimAsync(role,
                    new Claim("Permission", permission));
            }
        }

        // ── Audit Log ─────────────────────────────────────────────────
        await _auditService.LogAsync(
            action: "PermissionChanged",
            entityName: "Role",
            entityId: null,
            oldValues: new { Permissions = oldPermissions },
            newValues: new { Permissions = model.SelectedPermissions },
            changes: $"Permissions updated for role '{role.Name}'"
        );

        TempData["Success"] = $"Permissions updated for role '{role.Name}'.";
        return RedirectToAction(nameof(Index));
    }
}

// ViewModel
public class RolePermissionsViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public Dictionary<string, List<string>> AllPermissions { get; set; } = new();
    public List<string> GrantedPermissions { get; set; } = new();
    public List<string>? SelectedPermissions { get; set; }
}