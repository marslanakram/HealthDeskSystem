using HealthDesk.Domain.Entities;

namespace HealthDesk.Application.Interfaces;

public interface IPrescriptionRepository : IGenericRepository<Prescription>
{
    Task<Prescription?> GetWithDetailsAsync(int prescriptionId);
    Task<IEnumerable<Prescription>> GetByPatientAsync(int patientId);
    Task<IEnumerable<Prescription>> GetByDoctorAsync(int doctorId);
    Task<Prescription?> GetByAppointmentAsync(int appointmentId);
}
