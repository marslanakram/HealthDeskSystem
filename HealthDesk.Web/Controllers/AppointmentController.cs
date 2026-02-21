using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace HealthDesk.Web.Controllers;

[Authorize]
public class AppointmentController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController(
        IAppointmentService appointmentService,
        IPatientService patientService,
        IDoctorService doctorService,
        ILogger<AppointmentController> logger)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _doctorService = doctorService;
        _logger = logger;
    }

    // ── GET: /Appointment ─────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Appointments";
        ViewData["Title"] = "Appointments";

        IEnumerable<AppointmentDto> appointments;

        if (User.IsInRole("Doctor"))
        {
            var doctor = await _doctorService
                .GetByUserIdAsync(GetCurrentUserId());
            appointments = doctor is null
                ? Enumerable.Empty<AppointmentDto>()
                : await _appointmentService.GetByDoctorAsync(doctor.Id);
        }
        else if (User.IsInRole("Patient"))
        {
            var patient = await _patientService
                .GetByUserIdAsync(GetCurrentUserId());
            appointments = patient is null
                ? Enumerable.Empty<AppointmentDto>()
                : await _appointmentService.GetByPatientAsync(patient.Id);
        }
        else
        {
            // Admin / Receptionist see all
            appointments = await _appointmentService.GetAllAsync();
        }

        return View(appointments);
    }

    // ── GET: /Appointment/Details/5 ───────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActiveMenu"] = "Appointments";
        ViewData["Title"] = "Appointment Details";

        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment is null)
        {
            TempData["Error"] = "Appointment not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(appointment);
    }

    // ── GET: /Appointment/Create ──────────────────────────────────
    [Authorize(Policy = "ReceptionistOrAdmin")]
    public async Task<IActionResult> Create()
    {
        ViewData["ActiveMenu"] = "Appointments";
        ViewData["Title"] = "Book Appointment";
        await LoadDropdownsAsync();
        return View();
    }

    // ── POST: /Appointment/Create ─────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ReceptionistOrAdmin")]
    public async Task<IActionResult> Create(CreateAppointmentDto dto)
    {
        ViewData["ActiveMenu"] = "Appointments";
        ViewData["Title"] = "Book Appointment";

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(dto);
        }

        var (success, message, _) = await _appointmentService.CreateAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        await LoadDropdownsAsync();
        return View(dto);
    }

    // ── GET: /Appointment/Edit/5 ──────────────────────────────────
    [Authorize(Policy = "ReceptionistOrAdmin")]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["ActiveMenu"] = "Appointments";
        ViewData["Title"] = "Edit Appointment";

        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment is null)
        {
            TempData["Error"] = "Appointment not found.";
            return RedirectToAction(nameof(Index));
        }

        var dto = new UpdateAppointmentDto
        {
            Id = appointment.Id,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime,
            Status = appointment.Status,
            Notes = appointment.Notes,
            IsPaid = appointment.IsPaid
        };

        LoadStatusSelectList(dto.Status);
        return View(dto);
    }

    // ── POST: /Appointment/Edit/5 ─────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ReceptionistOrAdmin")]
    public async Task<IActionResult> Edit(UpdateAppointmentDto dto)
    {
        ViewData["ActiveMenu"] = "Appointments";
        ViewData["Title"] = "Edit Appointment";

        if (!ModelState.IsValid)
        {
            LoadStatusSelectList(dto.Status);
            return View(dto);
        }

        var (success, message) = await _appointmentService.UpdateAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        LoadStatusSelectList(dto.Status);
        return View(dto);
    }

    // ── POST: /Appointment/UpdateStatus ───────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
    {
        var (success, message) = await _appointmentService
            .UpdateStatusAsync(id, status);

        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── POST: /Appointment/Delete/5 ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _appointmentService.DeleteAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────
    private async Task LoadDropdownsAsync()
    {
        var patients = await _patientService.GetAllAsync();
        var doctors = await _doctorService.GetAllAsync();

        ViewBag.Patients = new SelectList(patients, "Id", "FullName");
        ViewBag.Doctors = new SelectList(doctors, "Id", "FullName");
    }

    private void LoadStatusSelectList(AppointmentStatus selected)
    {
        ViewBag.Statuses = new SelectList(
            Enum.GetValues<AppointmentStatus>()
                .Select(s => new { Value = (int)s, Text = s.ToString() }),
            "Value", "Text", (int)selected);
    }

    private string GetCurrentUserId()
    => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
