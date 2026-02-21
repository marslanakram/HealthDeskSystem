using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Domain.Enums;
using HealthDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Infrastructure.Repositories;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context) { }

    public async Task<Appointment?> GetWithDetailsAsync(int appointmentId)
        => await _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Department)
            .Include(a => a.Prescription)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

    public async Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId)
        => await _dbSet
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Department)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

    public async Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId)
        => await _dbSet
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
        => await _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status)
        => await _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

    public async Task<IEnumerable<Appointment>> GetTodaysAppointmentsAsync()
        => await _dbSet
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Department)
            .Where(a => a.AppointmentDate.Date == DateTime.Today)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();

    public async Task<bool> HasConflictAsync(
        int doctorId, DateTime date, TimeSpan time, int? excludeId = null)
        => await _dbSet.AnyAsync(a =>
            a.DoctorId == doctorId &&
            a.AppointmentDate.Date == date.Date &&
            a.AppointmentTime == time &&
            a.Status != AppointmentStatus.Cancelled &&
            (excludeId == null || a.Id != excludeId));
}
