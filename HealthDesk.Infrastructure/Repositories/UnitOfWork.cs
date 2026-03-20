using HealthDesk.Application.Interfaces;
using HealthDesk.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;

namespace HealthDesk.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IDbContextTransaction? _transaction;

    // ── Lazy-loaded repositories ──────────────────────────────────
    private IPatientRepository? _patients;
    private IDoctorRepository? _doctors;
    private IAppointmentRepository? _appointments;
    private IPrescriptionRepository? _prescriptions;
    private IDepartmentRepository? _departments;
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(AppDbContext context,
        IHttpContextAccessor httpContextAccessor
        )
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public IPatientRepository Patients
        => _patients ??= new PatientRepository(_context);

    public IDoctorRepository Doctors
        => _doctors ??= new DoctorRepository(_context);

    public IAppointmentRepository Appointments
        => _appointments ??= new AppointmentRepository(_context);

    public IPrescriptionRepository Prescriptions
        => _prescriptions ??= new PrescriptionRepository(_context);

    public IDepartmentRepository Departments
        => _departments ??= new DepartmentRepository(_context);

    public IAuditLogRepository AuditLogs
        => _auditLogs ??= new AuditLogRepository(_context, _httpContextAccessor);

    // ── Save & Transaction ────────────────────────────────────────
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
