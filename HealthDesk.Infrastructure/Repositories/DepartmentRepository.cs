using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Infrastructure.Repositories;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Department>> GetAllActiveAsync()
        => await _dbSet
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();

    public async Task<Department?> GetWithDoctorsAsync(int departmentId)
        => await _dbSet
            .Include(d => d.Doctors)
                .ThenInclude(doc => doc.User)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

    public async Task<bool> HasDoctorsAsync(int departmentId)
        => await _dbSet
            .Include(d => d.Doctors)
            .AnyAsync(d => d.Id == departmentId && d.Doctors.Any());
}