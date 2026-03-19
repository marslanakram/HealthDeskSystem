using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthDesk.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class TenantManagementController : Controller
{
    private readonly ITenantService _tenantService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public TenantManagementController(
        ITenantService tenantService,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService)
    {
        _tenantService = tenantService;
        _userManager = userManager;
        _auditService = auditService;
    }

    // ── GET: Admin/TenantManagement ───────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Tenants";
        ViewData["Title"] = "Tenant Management";

        var tenants = await _tenantService.GetAllTenantsAsync();
        var currentTenant = _tenantService.GetCurrentTenant();

        ViewBag.CurrentTenantId = currentTenant?.Id ?? 1;

        return View(tenants);
    }

    // ── GET: Admin/TenantManagement/Create ────────────────────────
    public IActionResult Create()
    {
        ViewData["ActiveMenu"] = "Tenants";
        ViewData["Title"] = "Create New Tenant";
        return View();
    }

    // ── POST: Admin/TenantManagement/Create ───────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTenantViewModel model)
    {
        ViewData["ActiveMenu"] = "Tenants";
        ViewData["Title"] = "Create New Tenant";

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            // ── Step 1: Create Tenant ─────────────────────────────
            var (success, message, tenantId) = await _tenantService.CreateTenantAsync(
                model.Name,
                model.Identifier);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            // ── Step 2: Create Admin User for This Tenant ─────────
            var adminUser = new ApplicationUser
            {
                UserName = model.AdminEmail,
                Email = model.AdminEmail,
                FullName = model.AdminFullName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var userResult = await _userManager.CreateAsync(adminUser, model.AdminPassword);

            if (!userResult.Succeeded)
            {
                // If user creation fails, we should ideally rollback tenant creation
                // For now, show error
                var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                ModelState.AddModelError(string.Empty, $"Tenant created but admin user failed: {errors}");
                return View(model);
            }

            // ── Step 3: Assign Admin Role ─────────────────────────
            await _userManager.AddToRoleAsync(adminUser, "Admin");

            // ── Step 4: Assign TenantId Claim ─────────────────────
            await _userManager.AddClaimAsync(adminUser,
                new Claim("TenantId", tenantId.ToString()));

            // ── Step 5: Audit Log ──────────────────────────────────
            await _auditService.LogAsync(
                "Created",
                "Tenant",
                null,
                null,
                new { TenantName = model.Name, AdminEmail = model.AdminEmail },
                $"Created tenant '{model.Name}' with admin user {model.AdminEmail}");

            // ── Step 6: Success Message ────────────────────────────
            TempData["Success"] = $@"
                Tenant '{model.Name}' created successfully!
                <br><strong>Admin Email:</strong> {model.AdminEmail}
                <br><strong>Password:</strong> {model.AdminPassword}
                <br><em>Send these credentials to the client.</em>
            ";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating tenant: {ex.Message}");
            return View(model);
        }
    }

    // ── POST: Admin/TenantManagement/SwitchTenant ─────────────────
    [HttpPost]
    public IActionResult SwitchTenant(int tenantId)
    {
        _tenantService.SetCurrentTenant(tenantId);
        TempData["Success"] = "Switched tenant successfully!";
        return RedirectToAction("Index", "Dashboard", new { area = "" });
    }
}
