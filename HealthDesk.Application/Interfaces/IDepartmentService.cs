using HealthDesk.Application.DTOs;

namespace HealthDesk.Application.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message)> CreateAsync(CreateDepartmentDto dto);
    Task<(bool Success, string Message)> UpdateAsync(UpdateDepartmentDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
