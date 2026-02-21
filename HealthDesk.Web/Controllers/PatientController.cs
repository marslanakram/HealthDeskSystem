using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthDesk.Web.Controllers;

//[Authorize(Policy = "ReceptionistOrAdmin")]
[Authorize]
public class PatientController : Controller
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientController> _logger;

    public PatientController(
        IPatientService patientService,
        ILogger<PatientController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    // ── GET: /Patient ─────────────────────────────────────────────

    [Authorize(Policy = $"Permission:{PermissionConstants.Patients.View}")]
    public async Task<IActionResult> Index(string? search)
    {
        ViewData["ActiveMenu"] = "Patients";
        ViewData["Title"] = "Patients";
        ViewData["Search"] = search;

        var patients = string.IsNullOrWhiteSpace(search)
            ? await _patientService.GetAllAsync()
            : await _patientService.SearchAsync(search);

        return View(patients);
    }

    // ── GET: /Patient/Details/5 ───────────────────────────────────

    [Authorize(Policy = $"Permission:{PermissionConstants.Patients.View}")]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActiveMenu"] = "Patients";
        ViewData["Title"] = "Patient Details";

        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null)
        {
            TempData["Error"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(patient);
    }

    // ── GET: /Patient/Create ──────────────────────────────────────

    [Authorize(Policy = $"Permission:{PermissionConstants.Patients.Create}")]
    public IActionResult Create()
    {
        ViewData["ActiveMenu"] = "Patients";
        ViewData["Title"] = "Register Patient";
        return View();
    }

    // ── POST: /Patient/Create ─────────────────────────────────────

    [Authorize(Policy = $"Permission:{PermissionConstants.Patients.Create}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePatientDto dto)
    {
        ViewData["ActiveMenu"] = "Patients";
        ViewData["Title"] = "Register Patient";

        if (!ModelState.IsValid)
            return View(dto);

        var (success, message, _) = await _patientService.CreateAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        return View(dto);
    }

    // ── GET: /Patient/Edit/5 ──────────────────────────────────────

    [Authorize(Policy = $"Permission:{PermissionConstants.Patients.Edit}")]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["ActiveMenu"] = "Patients";
        ViewData["Title"] = "Edit Patient";

        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null)
        {
            TempData["Error"] = "Patient not found.";
            return RedirectToAction(nameof(Index));
        }

        var dto = new UpdatePatientDto
        {
            Id = patient.Id,
            FullName = patient.FullName,
            PhoneNumber = patient.PhoneNumber,
            Address = patient.Address,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            BloodGroup = patient.BloodGroup,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            MedicalHistory = patient.MedicalHistory
        };

        return View(dto);
    }

    // ── POST: /Patient/Edit/5 ─────────────────────────────────────

    [Authorize(Policy = $"Permission:{PermissionConstants.Patients.Edit}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdatePatientDto dto)
    {
        ViewData["ActiveMenu"] = "Patients";
        ViewData["Title"] = "Edit Patient";

        if (!ModelState.IsValid)
            return View(dto);

        var (success, message) = await _patientService.UpdateAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        return View(dto);
    }

    // ── POST: /Patient/Delete/5 ───────────────────────────────────

    [Authorize(Policy = $"Permission:{PermissionConstants.Patients.Delete}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _patientService.DeleteAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }
}
