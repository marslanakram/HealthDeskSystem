using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Infrastructure.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(AppDbContext context) : base(context) { }

    public async Task<Patient?> GetByUserIdAsync(string userId)
        => await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<Patient?> GetWithAppointmentsAsync(int patientId)
        => await _dbSet
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(p => p.Id == patientId);

    public async Task<IEnumerable<Patient>> GetAllWithUserAsync()
        => await _dbSet
            .Include(p => p.User)
            .OrderBy(p => p.FullName)
            .ToListAsync();

    public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm)
        => await _dbSet
            .Include(p => p.User)
            .Where(p => p.FullName.Contains(searchTerm) ||
                        p.PhoneNumber!.Contains(searchTerm) ||
                        p.User.Email!.Contains(searchTerm))
            .ToListAsync();
}