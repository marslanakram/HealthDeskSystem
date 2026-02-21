using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HealthDesk.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class DoctorController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DoctorController> _logger;

    public DoctorController(
        IDoctorService doctorService,
        IDepartmentService departmentService,
        ILogger<DoctorController> logger)
    {
        _doctorService = doctorService;
        _departmentService = departmentService;
        _logger = logger;
    }

    // ── GET: /Doctor ─────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search)
    {
        ViewData["ActiveMenu"] = "Doctors";
        ViewData["Title"] = "Doctors";
        ViewData["Search"] = search;

        var doctors = await _doctorService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
            doctors = doctors.Where(d =>
                d.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                d.Specialization.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                d.DepartmentName.Contains(search, StringComparison.OrdinalIgnoreCase));

        return View(doctors);
    }

    // ── GET: /Doctor/Details/5 ───────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActiveMenu"] = "Doctors";
        ViewData["Title"] = "Doctor Details";

        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor is null)
        {
            TempData["Error"] = "Doctor not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(doctor);
    }

    // ── GET: /Doctor/Create ──────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        ViewData["ActiveMenu"] = "Doctors";
        ViewData["Title"] = "Add Doctor";
        await LoadDepartmentsAsync();
        return View();
    }

    // ── POST: /Doctor/Create ─────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDoctorDto dto)
    {
        ViewData["ActiveMenu"] = "Doctors";
        ViewData["Title"] = "Add Doctor";

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync();
            return View(dto);
        }

        var (success, message, _) = await _doctorService.CreateAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        await LoadDepartmentsAsync();
        return View(dto);
    }

    // ── GET: /Doctor/Edit/5 ──────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["ActiveMenu"] = "Doctors";
        ViewData["Title"] = "Edit Doctor";

        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor is null)
        {
            TempData["Error"] = "Doctor not found.";
            return RedirectToAction(nameof(Index));
        }

        var dto = new UpdateDoctorDto
        {
            Id = doctor.Id,
            FullName = doctor.FullName,
            Specialization = doctor.Specialization,
            Qualification = doctor.Qualification,
            PhoneNumber = doctor.PhoneNumber,
            ConsultationFee = doctor.ConsultationFee,
            Shift = doctor.Shift,
            IsAvailable = doctor.IsAvailable,
            DepartmentId = doctor.DepartmentId
        };

        await LoadDepartmentsAsync(dto.DepartmentId);
        return View(dto);
    }

    // ── POST: /Doctor/Edit/5 ─────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateDoctorDto dto)
    {
        ViewData["ActiveMenu"] = "Doctors";
        ViewData["Title"] = "Edit Doctor";

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(dto.DepartmentId);
            return View(dto);
        }

        var (success, message) = await _doctorService.UpdateAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        await LoadDepartmentsAsync(dto.DepartmentId);
        return View(dto);
    }

    // ── POST: /Doctor/Delete/5 ───────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _doctorService.DeleteAsync(id);

        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    // ── Helper ───────────────────────────────────────────────────
    private async Task LoadDepartmentsAsync(int? selectedId = null)
    {
        var departments = await _departmentService.GetAllAsync();
        ViewBag.Departments = new SelectList(
            departments, "Id", "Name", selectedId);
    }
}
