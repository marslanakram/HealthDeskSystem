using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Infrastructure.Repositories;

public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(AppDbContext context) : base(context) { }

    public async Task<Doctor?> GetByUserIdAsync(string userId)
        => await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId);

    public async Task<Doctor?> GetWithAppointmentsAsync(int doctorId)
        => await _dbSet
            .Include(d => d.Appointments)
                .ThenInclude(a => a.Patient)
            .FirstOrDefaultAsync(d => d.Id == doctorId);

    public async Task<IEnumerable<Doctor>> GetAllWithDepartmentAsync()
        => await _dbSet
            .Include(d => d.Department)
            .Include(d => d.User)
            .OrderBy(d => d.FullName)
            .ToListAsync();

    public async Task<IEnumerable<Doctor>> GetByDepartmentAsync(int departmentId)
        => await _dbSet
            .Include(d => d.User)
            .Where(d => d.DepartmentId == departmentId)
            .ToListAsync();

    public async Task<IEnumerable<Doctor>> SearchAsync(string searchTerm)
        => await _dbSet
            .Include(d => d.Department)
            .Where(d => d.FullName.Contains(searchTerm) ||
                        d.Specialization.Contains(searchTerm) ||
                        d.Department.Name.Contains(searchTerm))
            .ToListAsync();
}