using HealthDesk.Domain.Entities;
using HealthDesk.Domain.Enums;

namespace HealthDesk.Application.Interfaces;

public interface IAppointmentRepository : IGenericRepository<Appointment>
{
    Task<Appointment?> GetWithDetailsAsync(int appointmentId);
    Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId);
    Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
    Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status);
    Task<IEnumerable<Appointment>> GetTodaysAppointmentsAsync();
    Task<bool> HasConflictAsync(int doctorId, DateTime date, TimeSpan time, int? excludeId = null);
}
