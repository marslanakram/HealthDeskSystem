using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDesk.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IAuditService _auditService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger,
        IAuditService auditService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _auditService = auditService;
    }

    // ── GET: /Account/Login ───────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Already logged in — redirect to dashboard
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToDashboard();

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ── POST: /Account/Login ──────────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in.", model.Email);
            // ── Audit Log ─────────────────────────────────────────────
            await _auditService.LogAsync(
                action: "Login",
                entityName: "User",
                entityId: null,
                oldValues: null,
                newValues: null,
                changes: $"User logged in: {model.Email}"
            );
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} account locked out.", model.Email);
            ModelState.AddModelError(string.Empty,
                "Account locked due to multiple failed attempts. Try again in 15 minutes.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    // ── POST: /Account/Logout ─────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity?.Name;

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User logged out.");
        // ── Audit Log ─────────────────────────────────────────────
        await _auditService.LogAsync(
            action: "Logout",
            entityName: "User",
            entityId: null,
            oldValues: null,
            newValues: null,
            changes: $"User logged out: {userName}"
        );
        return RedirectToAction("Login", "Account");
    }

    // ── GET: /Account/AccessDenied ────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
        => View();

    // ── Helpers ───────────────────────────────────────────────────
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToDashboard();
    }

    private IActionResult RedirectToDashboard()
    {
        if (User.IsInRole("SuperAdmin"))
            return RedirectToAction("Index", "Dashboard", new { area = "SuperAdmin" });

        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        if (User.IsInRole("Doctor"))
            return RedirectToAction("Index", "Dashboard");

        if (User.IsInRole("Receptionist"))
            return RedirectToAction("Index", "Dashboard");

        return RedirectToAction("Index", "Dashboard");
    }
}
