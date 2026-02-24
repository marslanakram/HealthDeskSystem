using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using static HealthDesk.Domain.Constants.PermissionConstants;

namespace HealthDesk.Application.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public PrescriptionService(IUnitOfWork uow, IMapper mapper, IAuditService auditService)
    {
        _uow = uow;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<IEnumerable<PrescriptionDto>> GetAllAsync()
    {
        var prescriptions = await _uow.Prescriptions.GetAllAsync();
        return _mapper.Map<IEnumerable<PrescriptionDto>>(prescriptions);
    }

    public async Task<PrescriptionDto?> GetByIdAsync(int id)
    {
        var prescription = await _uow.Prescriptions.GetWithDetailsAsync(id);
        return prescription is null
            ? null
            : _mapper.Map<PrescriptionDto>(prescription);
    }

    public async Task<IEnumerable<PrescriptionDto>> GetByPatientAsync(int patientId)
    {
        var prescriptions = await _uow.Prescriptions.GetByPatientAsync(patientId);
        return _mapper.Map<IEnumerable<PrescriptionDto>>(prescriptions);
    }

    public async Task<IEnumerable<PrescriptionDto>> GetByDoctorAsync(int doctorId)
    {
        var prescriptions = await _uow.Prescriptions.GetByDoctorAsync(doctorId);
        return _mapper.Map<IEnumerable<PrescriptionDto>>(prescriptions);
    }

    public async Task<PrescriptionDto?> GetByAppointmentAsync(int appointmentId)
    {
        var prescription = await _uow.Prescriptions.GetByAppointmentAsync(appointmentId);
        return prescription is null
            ? null
            : _mapper.Map<PrescriptionDto>(prescription);
    }

    public async Task<(bool Success, string Message, int PrescriptionId)> CreateAsync(
        CreatePrescriptionDto dto)
    {
        // Verify appointment exists
        var appointment = await _uow.Appointments.GetByIdAsync(dto.AppointmentId);
        if (appointment is null)
            return (false, "Appointment not found.", 0);

        // Check if prescription already exists for this appointment
        var existing = await _uow.Prescriptions
            .GetByAppointmentAsync(dto.AppointmentId);
        if (existing is not null)
            return (false, "A prescription already exists for this appointment.", 0);

        var prescription = _mapper.Map<Prescription>(dto);
        prescription.PrescribedDate = DateTime.UtcNow;

        await _uow.Prescriptions.AddAsync(prescription);
        await _uow.SaveChangesAsync();
        await _auditService.LogAsync(
    action: "Created",
    entityName: "Prescription",
    entityId: prescription.Id,
    oldValues: null,
    newValues: new { prescription.Diagnosis, prescription.Medicines, prescription.Instructions, prescription.Patient, prescription.Doctor, prescription.Appointment },
    changes: $"New prescription registered: {prescription.Id}"
);

        return (true, "Prescription created successfully.", prescription.Id);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var prescription = await _uow.Prescriptions.GetByIdAsync(id);
        if (prescription is null)
            return (false, "Prescription not found.");

        prescription.IsDeleted = true;
        _uow.Prescriptions.Update(prescription);
        await _uow.SaveChangesAsync();
        await _auditService.LogAsync(
    action: "Deleted",
    entityName: "Prescription",
    entityId: id,
    oldValues: new { prescription.Diagnosis, prescription.Medicines, prescription.Instructions, prescription.Patient, prescription.Doctor, prescription.Appointment },
    newValues: null,
    changes: $"Prescription deleted: {prescription.Diagnosis}"
);

        return (true, "Prescription deleted successfully.");
    }
}
