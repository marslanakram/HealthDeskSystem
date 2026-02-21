using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthDesk.Web.Controllers;

[Authorize]
public class PrescriptionController : Controller
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly ILogger<PrescriptionController> _logger;

    public PrescriptionController(
        IPrescriptionService prescriptionService,
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientService patientService,
        ILogger<PrescriptionController> logger)
    {
        _prescriptionService = prescriptionService;
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
        _logger = logger;
    }

    // ── GET: /Prescription ────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["ActiveMenu"] = "Prescriptions";
        ViewData["Title"] = "Prescriptions";

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        IEnumerable<PrescriptionDto> prescriptions;

        if (User.IsInRole("Doctor"))
        {
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            prescriptions = doctor is null
                ? Enumerable.Empty<PrescriptionDto>()
                : await _prescriptionService.GetByDoctorAsync(doctor.Id);
        }
        else if (User.IsInRole("Patient"))
        {
            var patient = await _patientService.GetByUserIdAsync(userId);
            prescriptions = patient is null
                ? Enumerable.Empty<PrescriptionDto>()
                : await _prescriptionService.GetByPatientAsync(patient.Id);
        }
        else
        {
            prescriptions = await _prescriptionService.GetAllAsync();
        }

        return View(prescriptions);
    }

    // ── GET: /Prescription/Details/5 ──────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActiveMenu"] = "Prescriptions";
        ViewData["Title"] = "Prescription Details";

        var prescription = await _prescriptionService.GetByIdAsync(id);
        if (prescription is null)
        {
            TempData["Error"] = "Prescription not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(prescription);
    }

    // ── GET: /Prescription/Create?appointmentId=5 ─────────────────
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> Create(int appointmentId)
    {
        ViewData["ActiveMenu"] = "Prescriptions";
        ViewData["Title"] = "Create Prescription";

        var appointment = await _appointmentService.GetByIdAsync(appointmentId);
        if (appointment is null)
        {
            TempData["Error"] = "Appointment not found.";
            return RedirectToAction("Index", "Appointment");
        }

        // Check if prescription already exists
        var existing = await _prescriptionService
            .GetByAppointmentAsync(appointmentId);
        if (existing is not null)
        {
            TempData["Error"] = "A prescription already exists for this appointment.";
            return RedirectToAction(nameof(Details), new { id = existing.Id });
        }

        var dto = new CreatePrescriptionDto
        {
            AppointmentId = appointmentId,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId
        };

        ViewBag.PatientName = appointment.PatientName;
        ViewBag.DoctorName = appointment.DoctorName;
        ViewBag.AppointmentDate = appointment.AppointmentDate
            .ToString("dd MMM yyyy");

        return View(dto);
    }

    // ── POST: /Prescription/Create ────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> Create(CreatePrescriptionDto dto)
    {
        ViewData["ActiveMenu"] = "Prescriptions";
        ViewData["Title"] = "Create Prescription";

        if (!ModelState.IsValid)
        {
            var appt = await _appointmentService.GetByIdAsync(dto.AppointmentId);
            if (appt is not null)
            {
                ViewBag.PatientName = appt.PatientName;
                ViewBag.DoctorName = appt.DoctorName;
                ViewBag.AppointmentDate = appt.AppointmentDate
                    .ToString("dd MMM yyyy");
            }
            return View(dto);
        }

        var (success, message, prescriptionId) =
            await _prescriptionService.CreateAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Details), new { id = prescriptionId });
        }

        ModelState.AddModelError(string.Empty, message);
        return View(dto);
    }

    // ── POST: /Prescription/Delete/5 ──────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _prescriptionService.DeleteAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }
}
