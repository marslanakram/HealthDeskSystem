using HealthDesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthDesk.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AuditLogController : Controller
{
    private readonly IAuditService _auditService;

    public AuditLogController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    // ── GET: Admin/AuditLog ───────────────────────────────────────
    public async Task<IActionResult> Index(string? action, string? entity)
    {
        ViewData["ActiveMenu"] = "AuditLog";
        ViewData["Title"] = "Audit Log";

        var logs = await _auditService.GetRecentAsync(100);

     

        // Filter by action
        if (!string.IsNullOrWhiteSpace(action))
            logs = logs.Where(l => l.Action == action);

        // Filter by entity
        if (!string.IsNullOrWhiteSpace(entity))
            logs = logs.Where(l => l.EntityName == entity);

        ViewData["Action"] = action;
        ViewData["Entity"] = entity;

        return View(logs);
    }

    // ── GET: Admin/AuditLog/Details/5 ─────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActiveMenu"] = "AuditLog";
        ViewData["Title"] = "Audit Log Details";

        var logs = await _auditService.GetAllAsync();
        var log = logs.FirstOrDefault(l => l.Id == id);

        if (log is null)
        {
            TempData["Error"] = "Audit log not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(log);
    }
}