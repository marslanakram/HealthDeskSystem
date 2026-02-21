using HealthDesk.Domain.Entities;

namespace HealthDesk.Application.Interfaces;

public interface IPatientRepository : IGenericRepository<Patient>
{
    Task<Patient?> GetByUserIdAsync(string userId);
    Task<Patient?> GetWithAppointmentsAsync(int patientId);
    Task<IEnumerable<Patient>> GetAllWithUserAsync();
    Task<IEnumerable<Patient>> SearchAsync(string searchTerm);
}
