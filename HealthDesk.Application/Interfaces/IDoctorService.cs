using HealthDesk.Application.DTOs;

namespace HealthDesk.Application.Interfaces;

public interface IDoctorService
{
    Task<IEnumerable<DoctorDto>> GetAllAsync();
    Task<DoctorDto?> GetByIdAsync(int id);
    Task<DoctorDto?> GetByUserIdAsync(string userId);
    Task<IEnumerable<DoctorDto>> GetByDepartmentAsync(int departmentId);
    Task<(bool Success, string Message, int DoctorId)> CreateAsync(CreateDoctorDto dto);
    Task<(bool Success, string Message)> UpdateAsync(UpdateDoctorDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<IEnumerable<DoctorDto>> SearchAsync(string searchTerm);
}
