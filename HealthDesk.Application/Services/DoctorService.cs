using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HealthDesk.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public DoctorService(
        IUnitOfWork uow,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
    {
        var doctors = await _uow.Doctors.GetAllWithDepartmentAsync();
        return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
    }

    public async Task<DoctorDto?> GetByIdAsync(int id)
    {
        var doctor = await _uow.Doctors.GetWithAppointmentsAsync(id);
        return doctor is null ? null : _mapper.Map<DoctorDto>(doctor);
    }

    public async Task<DoctorDto?> GetByUserIdAsync(string userId)
    {
        var doctor = await _uow.Doctors.GetByUserIdAsync(userId);
        return doctor is null ? null : _mapper.Map<DoctorDto>(doctor);
    }

    public async Task<IEnumerable<DoctorDto>> GetByDepartmentAsync(int departmentId)
    {
        var doctors = await _uow.Doctors.GetByDepartmentAsync(departmentId);
        return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
    }

    public async Task<IEnumerable<DoctorDto>> SearchAsync(string searchTerm)
    {
        var doctors = await _uow.Doctors.SearchAsync(searchTerm);
        return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
    }

    public async Task<(bool Success, string Message, int DoctorId)> CreateAsync(
        CreateDoctorDto dto)
    {
        // Check if email already exists
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return (false, "A user with this email already exists.", 0);

        // Check if department exists
        var departmentExists = await _uow.Departments
            .ExistsAsync(d => d.Id == dto.DepartmentId);

        if (!departmentExists)
            return (false, "Selected department does not exist.", 0);

        await _uow.BeginTransactionAsync();
        try
        {
            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), 0);

            await _userManager.AddToRoleAsync(user, "Doctor");

            // Create doctor profile
            var doctor = _mapper.Map<Doctor>(dto);
            doctor.UserId = user.Id;

            await _uow.Doctors.AddAsync(doctor);
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();

            return (true, "Doctor registered successfully.", doctor.Id);
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync();
            return (false, $"An error occurred: {ex.Message}", 0);
        }
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateDoctorDto dto)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(dto.Id);
        if (doctor is null)
            return (false, "Doctor not found.");

        // Verify new department exists
        var departmentExists = await _uow.Departments
            .ExistsAsync(d => d.Id == dto.DepartmentId);

        if (!departmentExists)
            return (false, "Selected department does not exist.");

        _mapper.Map(dto, doctor);
        _uow.Doctors.Update(doctor);
        await _uow.SaveChangesAsync();

        return (true, "Doctor updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(id);
        if (doctor is null)
            return (false, "Doctor not found.");

        // Check if doctor has upcoming appointments
        var hasUpcoming = await _uow.Appointments.ExistsAsync(a =>
            a.DoctorId == id &&
            a.AppointmentDate >= DateTime.Today &&
            a.Status != Domain.Enums.AppointmentStatus.Cancelled);

        if (hasUpcoming)
            return (false, "Cannot delete a doctor who has upcoming appointments.");

        // Soft delete
        doctor.IsDeleted = true;
        _uow.Doctors.Update(doctor);
        await _uow.SaveChangesAsync();

        return (true, "Doctor deleted successfully.");
    }
}
