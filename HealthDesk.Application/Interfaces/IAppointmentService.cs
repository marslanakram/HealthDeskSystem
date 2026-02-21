using HealthDesk.Application.DTOs;
using HealthDesk.Domain.Enums;

namespace HealthDesk.Application.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync();
    Task<AppointmentDto?> GetByIdAsync(int id);
    Task<IEnumerable<AppointmentDto>> GetByPatientAsync(int patientId);
    Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(int doctorId);
    Task<IEnumerable<AppointmentDto>> GetTodaysAsync();
    Task<(bool Success, string Message, int AppointmentId)> CreateAsync(CreateAppointmentDto dto);
    Task<(bool Success, string Message)> UpdateAsync(UpdateAppointmentDto dto);
    Task<(bool Success, string Message)> UpdateStatusAsync(int id, AppointmentStatus status);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
