using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Domain.Enums;

namespace HealthDesk.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AppointmentService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        var appointments = await _uow.Appointments.GetAllAsync();
        return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var appointment = await _uow.Appointments.GetWithDetailsAsync(id);
        return appointment is null ? null : _mapper.Map<AppointmentDto>(appointment);
    }

    public async Task<IEnumerable<AppointmentDto>> GetByPatientAsync(int patientId)
    {
        var appointments = await _uow.Appointments.GetByPatientAsync(patientId);
        return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
    }

    public async Task<IEnumerable<AppointmentDto>> GetByDoctorAsync(int doctorId)
    {
        var appointments = await _uow.Appointments.GetByDoctorAsync(doctorId);
        return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
    }

    public async Task<IEnumerable<AppointmentDto>> GetTodaysAsync()
    {
        var appointments = await _uow.Appointments.GetTodaysAppointmentsAsync();
        return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
    }

    public async Task<(bool Success, string Message, int AppointmentId)> CreateAsync(
        CreateAppointmentDto dto)
    {
        // Check for scheduling conflict
        var hasConflict = await _uow.Appointments.HasConflictAsync(
            dto.DoctorId, dto.AppointmentDate, dto.AppointmentTime);

        if (hasConflict)
            return (false, "This doctor already has an appointment at the selected time.", 0);

        var appointment = _mapper.Map<Appointment>(dto);
        appointment.Status = AppointmentStatus.Pending;

        await _uow.Appointments.AddAsync(appointment);
        await _uow.SaveChangesAsync();

        return (true, "Appointment booked successfully.", appointment.Id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateAppointmentDto dto)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(dto.Id);
        if (appointment is null)
            return (false, "Appointment not found.");

        _mapper.Map(dto, appointment);
        _uow.Appointments.Update(appointment);
        await _uow.SaveChangesAsync();

        return (true, "Appointment updated successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateStatusAsync(
        int id, AppointmentStatus status)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id);
        if (appointment is null)
            return (false, "Appointment not found.");

        appointment.Status = status;
        _uow.Appointments.Update(appointment);
        await _uow.SaveChangesAsync();

        return (true, $"Appointment status updated to {status}.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var appointment = await _uow.Appointments.GetByIdAsync(id);
        if (appointment is null)
            return (false, "Appointment not found.");

        appointment.IsDeleted = true;
        _uow.Appointments.Update(appointment);
        await _uow.SaveChangesAsync();

        return (true, "Appointment deleted successfully.");
    }
}
