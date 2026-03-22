namespace HealthDesk.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IPatientRepository Patients { get; }
    IDoctorRepository Doctors { get; }
    IAppointmentRepository Appointments { get; }
    IPrescriptionRepository Prescriptions { get; }
    IDepartmentRepository Departments { get; }
    IAuditLogRepository AuditLogs { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    // ── New method — wraps work in execution strategy + transaction ─
    // Application services call this instead of managing
    // transactions directly. Keeps AppDbContext in Infrastructure.
    Task ExecuteInTransactionAsync(Func<Task> work);

}
