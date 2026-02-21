using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Infrastructure.Repositories;

public class PrescriptionRepository : GenericRepository<Prescription>, IPrescriptionRepository
{
    public PrescriptionRepository(AppDbContext context) : base(context) { }

    public async Task<Prescription?> GetWithDetailsAsync(int prescriptionId)
        => await _dbSet
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId);

    public async Task<IEnumerable<Prescription>> GetByPatientAsync(int patientId)
        => await _dbSet
            .Include(p => p.Doctor)
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PrescribedDate)
            .ToListAsync();

    public async Task<IEnumerable<Prescription>> GetByDoctorAsync(int doctorId)
        => await _dbSet
            .Include(p => p.Patient)
            .Where(p => p.DoctorId == doctorId)
            .OrderByDescending(p => p.PrescribedDate)
            .ToListAsync();

    public async Task<Prescription?> GetByAppointmentAsync(int appointmentId)
        => await _dbSet
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
}
