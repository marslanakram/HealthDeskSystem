using HealthDesk.Domain.Entities;

namespace HealthDesk.Application.Interfaces;

public interface IDoctorRepository : IGenericRepository<Doctor>
{
    Task<Doctor?> GetByUserIdAsync(string userId);
    Task<Doctor?> GetWithAppointmentsAsync(int doctorId);
    Task<IEnumerable<Doctor>> GetAllWithDepartmentAsync();
    Task<IEnumerable<Doctor>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Doctor>> SearchAsync(string searchTerm);
}
