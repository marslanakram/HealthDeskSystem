using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthDesk.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class DepartmentController : Controller
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentController> _logger;
    private readonly IDoctorService _doctorService;

    public DepartmentController(
        IDepartmentService departmentService,
        ILogger<DepartmentController> logger,
        IDoctorService doctorService)
    {
        _departmentService = departmentService;
        _logger = logger;
        _doctorService = doctorService;
    }

    // ── GET: /Department ─────────────────────────────────────────
    public async Task<IActionResult> Index(string? search)
    {
        ViewData["ActiveMenu"] = "Departments";
        ViewData["Title"] = "Departments";
        ViewData["Search"] = search;

        var departments = await _departmentService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            departments = departments
                .Where(d => d.Name.Contains(search,
                    StringComparison.OrdinalIgnoreCase));

        return View(departments);
    }

    // ── GET: /Department/Details/5 ───────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActiveMenu"] = "Departments";
        ViewData["Title"] = "Department Details";

        var department = await _departmentService.GetByIdAsync(id);
        if (department is null)
        {
            TempData["Error"] = "Department not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(department);
    }

    // ── GET: /Department/Create ──────────────────────────────────
    public IActionResult Create()
    {
        ViewData["ActiveMenu"] = "Departments";
        ViewData["Title"] = "Add Department";
        return View();
    }

    // ── POST: /Department/Create ─────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        ViewData["ActiveMenu"] = "Departments";
        ViewData["Title"] = "Add Department";

        if (!ModelState.IsValid)
            return View(dto);

        var (success, message) = await _departmentService.CreateAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        return View(dto);
    }

    // ── GET: /Department/Edit/5 ──────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["ActiveMenu"] = "Departments";
        ViewData["Title"] = "Edit Department";

        var department = await _departmentService.GetByIdAsync(id);
        if (department is null)
        {
            TempData["Error"] = "Department not found.";
            return RedirectToAction(nameof(Index));
        }

        var dto = new UpdateDepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            Location = department.Location,
            IsActive = department.IsActive
        };

        return View(dto);
    }

    // ── POST: /Department/Edit/5 ─────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateDepartmentDto dto)
    {
        ViewData["ActiveMenu"] = "Departments";
        ViewData["Title"] = "Edit Department";

        if (!ModelState.IsValid)
            return View(dto);

        var (success, message) = await _departmentService.UpdateAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        return View(dto);
    }

    // ── POST: /Department/Delete/5 ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _departmentService.DeleteAsync(id);

        if (success)
            TempData["Success"] = message;
        else
            TempData["Error"] = message;

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> GetDoctors(int departmentId)
    {
        var doctors = await _doctorService.GetByDepartmentAsync(departmentId);

        return PartialView("_DoctorsListPartial", doctors);
    }
}
