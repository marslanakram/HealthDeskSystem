using HealthDesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthDesk.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class TenantManagementController : Controller
{
    private readonly ITenantService _tenantService;

    public TenantManagementController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Tenants";
        ViewData["Title"] = "Tenant Management";

        var tenants = await _tenantService.GetAllTenantsAsync();
        var currentTenant = _tenantService.GetCurrentTenant();

        ViewBag.CurrentTenantId = currentTenant?.Id ?? 1;

        return View(tenants);
    }

    public IActionResult Create()
    {
        ViewData["ActiveMenu"] = "Tenants";
        ViewData["Title"] = "Create Tenant";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string identifier)
    {
        ViewData["ActiveMenu"] = "Tenants";
        ViewData["Title"] = "Create Tenant";

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(identifier))
        {
            ModelState.AddModelError(string.Empty, "Name and Identifier are required.");
            return View();
        }

        var (success, message, tenantId) = await _tenantService.CreateTenantAsync(name, identifier);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        return View();
    }

    [HttpPost]
    public IActionResult SwitchTenant(int tenantId)
    {
        _tenantService.SetCurrentTenant(tenantId);
        TempData["Success"] = "Switched tenant successfully!";
        return RedirectToAction("Index", "Dashboard", new { area = "" });
    }
}
