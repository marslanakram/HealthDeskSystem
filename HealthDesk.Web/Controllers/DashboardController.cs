using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthDesk.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        IDoctorService doctorService,
        IPatientService patientService,
        UserManager<ApplicationUser> userManager,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _doctorService = doctorService;
        _patientService = patientService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        ViewData["ActiveMenu"] = "Dashboard";

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        if (User.IsInRole("Doctor"))
        {
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            if (doctor is null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return View("Index", null);
            }

            var doctorDashboard =
                await _dashboardService.GetDoctorDashboardAsync(doctor.Id);

            ViewData["DoctorName"] = doctor.FullName;
            ViewData["DashboardType"] = "Doctor";
            return View(doctorDashboard);
        }

        if (User.IsInRole("Patient"))
        {
            var patient = await _patientService.GetByUserIdAsync(userId);
            if (patient is null)
            {
                TempData["Error"] = "Patient profile not found.";
                return View("Index", null);
            }

            var patientDashboard =
                await _dashboardService.GetPatientDashboardAsync(patient.Id);

            ViewData["PatientName"] = patient.FullName;
            ViewData["DashboardType"] = "Patient";
            return View(patientDashboard);
        }

        // Admin / Receptionist / Nurse
        var adminDashboard = await _dashboardService.GetAdminDashboardAsync();
        ViewData["DashboardType"] = "Admin";
        return View(adminDashboard);
    }
}
