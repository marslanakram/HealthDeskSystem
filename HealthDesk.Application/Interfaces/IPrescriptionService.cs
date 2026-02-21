using HealthDesk.Application.DTOs;

namespace HealthDesk.Application.Interfaces;

public interface IPrescriptionService
{
    Task<IEnumerable<PrescriptionDto>> GetAllAsync();
    Task<PrescriptionDto?> GetByIdAsync(int id);
    Task<IEnumerable<PrescriptionDto>> GetByPatientAsync(int patientId);
    Task<IEnumerable<PrescriptionDto>> GetByDoctorAsync(int doctorId);
    Task<PrescriptionDto?> GetByAppointmentAsync(int appointmentId);
    Task<(bool Success, string Message, int PrescriptionId)> CreateAsync(CreatePrescriptionDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
