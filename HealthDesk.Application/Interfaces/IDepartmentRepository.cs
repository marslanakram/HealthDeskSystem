using HealthDesk.Domain.Entities;

namespace HealthDesk.Application.Interfaces;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<IEnumerable<Department>> GetAllActiveAsync();
    Task<Department?> GetWithDoctorsAsync(int departmentId);
    Task<bool> HasDoctorsAsync(int departmentId);
}