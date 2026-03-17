using HealthDesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthDesk.Web.Areas.SuperAdmin.Controllers;

[Area("SuperAdmin")]
[Authorize(Policy = "SuperAdminOnly")]
public class DashboardController : Controller
{
    private readonly ITenantService _tenantService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentService _appointmentService;

    public DashboardController(
        ITenantService tenantService,
        IPatientService patientService,
        IDoctorService doctorService,
        IAppointmentService appointmentService)
    {
        _tenantService = tenantService;
        _patientService = patientService;
        _doctorService = doctorService;
        _appointmentService = appointmentService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "SuperAdmin Dashboard";
        ViewData["ActiveMenu"] = "SuperAdminDashboard";

        var tenants = await _tenantService.GetAllTenantsAsync();

        // Get stats across ALL tenants
        var viewModel = new SuperAdminDashboardViewModel
        {
            TotalTenants = tenants.Count(),
            ActiveTenants = tenants.Count(t => t.IsActive),
            Tenants = tenants.ToList()
        };

        return View(viewModel);
    }
}

public class SuperAdminDashboardViewModel
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public List<HealthDesk.Domain.Entities.Tenant> Tenants { get; set; } = new();
}