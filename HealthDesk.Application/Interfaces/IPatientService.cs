using HealthDesk.Application.DTOs;

namespace HealthDesk.Application.Interfaces;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllAsync();
    Task<PatientDto?> GetByIdAsync(int id);
    Task<PatientDto?> GetByUserIdAsync(string userId);
    Task<(bool Success, string Message, int PatientId)> CreateAsync(CreatePatientDto dto);
    Task<(bool Success, string Message)> UpdateAsync(UpdatePatientDto dto);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<IEnumerable<PatientDto>> SearchAsync(string searchTerm);
}
