using HealthDesk.Application.DTOs;

namespace HealthDesk.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAdminDashboardAsync();
    Task<DashboardDto> GetDoctorDashboardAsync(int doctorId);
    Task<DashboardDto> GetPatientDashboardAsync(int patientId);
}
