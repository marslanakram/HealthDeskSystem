using HealthDesk.Domain.Entities;
using HealthDesk.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserManagementController> _logger;

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

        var users = await _userManager.Users.ToListAsync();
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

        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError("Email",
                "A user with this email already exists.");
            LoadRoles(model.Role);
            return View(model);
        }

        // Check if role exists
        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
        if (!roleExists)
        {
            ModelState.AddModelError("Role", "Selected role does not exist.");
            LoadRoles(model.Role);
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);

            _logger.LogInformation(
                "User {Email} created with role {Role}",
                model.Email, model.Role);

            TempData["Success"] =
                $"User '{model.FullName}' created successfully with role '{model.Role}'.";

            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        LoadRoles(model.Role);
        return View(model);
    }

    // ── Helper ────────────────────────────────────────────────────
    private void LoadRoles(string? selectedRole = null)
    {
        var roles = _roleManager.Roles
            .Select(r => r.Name)
            .OrderBy(r => r)
            .ToList();

        ViewBag.Roles = new SelectList(roles, selectedRole);
    }
}