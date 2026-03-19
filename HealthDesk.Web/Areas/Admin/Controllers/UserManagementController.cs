using HealthDesk.Domain.Entities;
using HealthDesk.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HealthDesk.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserManagementController> _logger;

    // These roles belong to the platform level — tenant admins must never see or assign them
    private static readonly string[] PlatformRoles = { "SuperAdmin" };

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserManagementController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    // ── GET: Admin/UserManagement ────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Users";
        ViewData["Title"] = "User Management";

        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var tenantClaim = User.FindFirst("TenantId");

        IList<ApplicationUser> users;

        if (isSuperAdmin)
        {
            // SuperAdmin sees all users when accessed from SuperAdmin area
            // But if they've switched to a specific tenant, show only that tenant's users
            var sessionTenant = HttpContext.Session.GetInt32("TenantId");
            if (sessionTenant.HasValue)
            {
                users = await _userManager.Users
                    .Where(u => u.TenantId == sessionTenant.Value)
                    .ToListAsync();
            }
            else
            {
                users = await _userManager.Users
                    .Where(u => u.TenantId != 0) // Exclude platform-level users
                    .ToListAsync();
            }
        }
        else if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var tenantId))
        {
            // Tenant Admin sees ONLY their own tenant's users
            users = await _userManager.Users
                .Where(u => u.TenantId == tenantId)
                .ToListAsync();
        }
        else
        {
            // No tenant resolved — show nothing (safe fallback)
            users = new List<ApplicationUser>();
            TempData["Error"] = "Tenant context could not be determined.";
        }

        var userRoles = new List<(ApplicationUser User, string Roles)>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles.Add((user, string.Join(", ", roles)));
        }

        return View(userRoles);
    }

    // ── GET: Admin/UserManagement/Create ──────────────────────────
    public IActionResult Create()
    {
        ViewData["ActiveMenu"] = "Users";
        ViewData["Title"] = "Create User";

        LoadRoles();
        return View();
    }

    // ── POST: Admin/UserManagement/Create ─────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        ViewData["ActiveMenu"] = "Users";
        ViewData["Title"] = "Create User";

        if (!ModelState.IsValid)
        {
            LoadRoles(model.Role);
            return View(model);
        }

        // ── Block assigning platform-level roles from tenant panel ──
        if (PlatformRoles.Contains(model.Role))
        {
            ModelState.AddModelError("Role",
                $"The role '{model.Role}' cannot be assigned from this panel.");
            LoadRoles(model.Role);
            return View(model);
        }

        // ── Resolve current tenant ────────────────────────────────
        int tenantId;
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        if (isSuperAdmin)
        {
            var sessionTenant = HttpContext.Session.GetInt32("TenantId");
            if (!sessionTenant.HasValue)
            {
                ModelState.AddModelError(string.Empty,
                    "Please switch to a specific tenant before creating a user.");
                LoadRoles(model.Role);
                return View(model);
            }
            tenantId = sessionTenant.Value;
        }
        else
        {
            var tenantClaim = User.FindFirst("TenantId");
            if (tenantClaim == null || !int.TryParse(tenantClaim.Value, out tenantId))
            {
                ModelState.AddModelError(string.Empty,
                    "Tenant context could not be determined. Please log in again.");
                LoadRoles(model.Role);
                return View(model);
            }
        }

        // ── Check email uniqueness ────────────────────────────────
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError("Email", "A user with this email already exists.");
            LoadRoles(model.Role);
            return View(model);
        }

        // ── Check role exists ─────────────────────────────────────
        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
        if (!roleExists)
        {
            ModelState.AddModelError("Role", "Selected role does not exist.");
            LoadRoles(model.Role);
            return View(model);
        }

        // ── Create user ───────────────────────────────────────────
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true,
            IsActive = true,
            TenantId = tenantId,    // ← Scoped to current tenant
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);

            // ── Store TenantId as a claim so it survives across logins ──
            await _userManager.AddClaimAsync(user,
                new Claim("TenantId", tenantId.ToString()));

            _logger.LogInformation(
                "User {Email} created with role {Role} for tenant {TenantId}",
                model.Email, model.Role, tenantId);

            TempData["Success"] =
                $"User '{model.FullName}' created successfully with role '{model.Role}'.";

            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        LoadRoles(model.Role);
        return View(model);
    }

    // ── Helper ────────────────────────────────────────────────────
    private void LoadRoles(string? selectedRole = null)
    {
        // Exclude platform-level roles from tenant user creation dropdown
        var roles = _roleManager.Roles
            .Where(r => !PlatformRoles.Contains(r.Name))
            .Select(r => r.Name)
            .OrderBy(r => r)
            .ToList();

        ViewBag.Roles = new SelectList(roles, selectedRole);
    }
}