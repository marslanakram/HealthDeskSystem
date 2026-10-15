//using AutoMapper;
//using HealthDesk.Application.DTOs;
//using HealthDesk.Application.Interfaces;
//using HealthDesk.Domain.Entities;
//using Microsoft.AspNetCore.Identity;
//using static HealthDesk.Domain.Constants.PermissionConstants;

//namespace HealthDesk.Application.Services;

//public class DoctorService : IDoctorService
//{
//    private readonly IUnitOfWork _uow;
//    private readonly IMapper _mapper;
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly IAuditService _auditService;       

//    public DoctorService(
//        IUnitOfWork uow,
//        IMapper mapper,
//        UserManager<ApplicationUser> userManager,
//        IAuditService auditService)
//    {
//        _uow = uow;
//        _mapper = mapper;
//        _userManager = userManager;
//        _auditService = auditService;
//    }

//    public async Task<IEnumerable<DoctorDto>> GetAllAsync()
//    {
//        var doctors = await _uow.Doctors.GetAllWithDepartmentAsync();
//        return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
//    }

//    public async Task<DoctorDto?> GetByIdAsync(int id)
//    {
//        var doctor = await _uow.Doctors.GetWithAppointmentsAsync(id);
//        return doctor is null ? null : _mapper.Map<DoctorDto>(doctor);
//    }

//    public async Task<DoctorDto?> GetByUserIdAsync(string userId)
//    {
//        var doctor = await _uow.Doctors.GetByUserIdAsync(userId);
//        return doctor is null ? null : _mapper.Map<DoctorDto>(doctor);
//    }

//    public async Task<IEnumerable<DoctorDto>> GetByDepartmentAsync(int departmentId)
//    {
//        var doctors = await _uow.Doctors.GetByDepartmentAsync(departmentId);
//        return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
//    }

//    public async Task<IEnumerable<DoctorDto>> SearchAsync(string searchTerm)
//    {
//        var doctors = await _uow.Doctors.SearchAsync(searchTerm);
//        return _mapper.Map<IEnumerable<DoctorDto>>(doctors);
//    }

//    public async Task<(bool Success, string Message, int DoctorId)> CreateAsync(
//        CreateDoctorDto dto)
//    {
//        // Check if email already exists
//        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
//            return (false, "A user with this email already exists.", 0);

//        // Check if department exists
//        var departmentExists = await _uow.Departments
//            .ExistsAsync(d => d.Id == dto.DepartmentId);

//        if (!departmentExists)
//            return (false, "Selected department does not exist.", 0);

//        await _uow.BeginTransactionAsync();
//        try
//        {
//            // Create Identity user
//            var user = new ApplicationUser
//            {
//                UserName = dto.Email,
//                Email = dto.Email,
//                FullName = dto.FullName,
//                EmailConfirmed = true,
//                IsActive = true
//            };

//            var result = await _userManager.CreateAsync(user, dto.Password);
//            if (!result.Succeeded)
//                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), 0);

//            await _userManager.AddToRoleAsync(user, "Doctor");

//            // Create doctor profile
//            var doctor = _mapper.Map<Doctor>(dto);
//            doctor.UserId = user.Id;

//            await _uow.Doctors.AddAsync(doctor);
//            await _uow.SaveChangesAsync();
//            await _uow.CommitTransactionAsync();
//            await _auditService.LogAsync(
//    action: "Created",
//    entityName: "Doctor",
//    entityId: doctor.Id,
//    oldValues: null,
//    newValues: new { doctor.FullName, doctor.PhoneNumber, doctor.Specialization, doctor.Qualification, doctor.ConsultationFee, doctor.Shift, doctor.IsAvailable, doctor.Department, doctor.Prescriptions, doctor.Appointments },
//    changes: $"New Doctor registered: {doctor.FullName}"
//);

//            return (true, "Doctor registered successfully.", doctor.Id);
//        }
//        catch (Exception ex)
//        {
//            await _uow.RollbackTransactionAsync();
//            return (false, $"An error occurred: {ex.Message}", 0);
//        }
//    }

//    public async Task<(bool Success, string Message)> UpdateAsync(UpdateDoctorDto dto)
//    {
//        var doctor = await _uow.Doctors.GetByIdAsync(dto.Id);
//        if (doctor is null)
//            return (false, "Doctor not found.");
//        var oldValues = new { doctor.FullName, doctor.PhoneNumber, doctor.Specialization, doctor.Qualification, doctor.ConsultationFee, doctor.Shift, doctor.IsAvailable, doctor.Department, doctor.Prescriptions, doctor.Appointments };

//            // Verify new department exists
//            var departmentExists = await _uow.Departments
//            .ExistsAsync(d => d.Id == dto.DepartmentId);

//        if (!departmentExists)
//            return (false, "Selected department does not exist.");

//        _mapper.Map(dto, doctor);
//        _uow.Doctors.Update(doctor);
//        await _uow.SaveChangesAsync();
//        await _auditService.LogAsync(
//    action: "Updated",
//    entityName: "Doctor",
//    entityId: doctor.Id,
//    oldValues: oldValues,
//    newValues: new { doctor.FullName, doctor.PhoneNumber, doctor.Specialization, doctor.Qualification, doctor.ConsultationFee, doctor.Shift, doctor.IsAvailable, doctor.Department, doctor.Prescriptions, doctor.Appointments },
//    changes: $"Doctor profile updated: {doctor.FullName}"
//);

//        return (true, "Doctor updated successfully.");
//    }

//    public async Task<(bool Success, string Message)> DeleteAsync(int id)
//    {
//        var doctor = await _uow.Doctors.GetByIdAsync(id);
//        if (doctor is null)
//            return (false, "Doctor not found.");

//        // Check if doctor has upcoming appointments
//        var hasUpcoming = await _uow.Appointments.ExistsAsync(a =>
//            a.DoctorId == id &&
//            a.AppointmentDate >= DateTime.Today &&
//            a.Status != Domain.Enums.AppointmentStatus.Cancelled);

//        if (hasUpcoming)
//            return (false, "Cannot delete a doctor who has upcoming appointments.");

//        // Soft delete
//        doctor.IsDeleted = true;
//        _uow.Doctors.Update(doctor);
//        await _uow.SaveChangesAsync();
//        await _auditService.LogAsync(
//    action: "Deleted",
//    entityName: "Doctor",
//    entityId: id,
//    oldValues: new { doctor.FullName },
//    newValues: null,
//    changes: $"Doctor deleted: {doctor.FullName}"
//    );
//        return (true, "Doctor deleted successfully.");
//    }
//}


using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace HealthDesk.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;
    private readonly ITenantService _tenantService; // ← inject this

    public DoctorService(
        IUnitOfWork uow,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService,
        ITenantService tenantService) // ← add this
    {
        _uow = uow;
        _mapper = mapper;
        _userManager = userManager;
        _auditService = auditService;
        _tenantService = tenantService;
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
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return (false, "A user with this email already exists.", 0);

        var departmentExists = await _uow.Departments
            .ExistsAsync(d => d.Id == dto.DepartmentId);

        if (!departmentExists)
            return (false, "Selected department does not exist.", 0);

        // ── Get current tenant BEFORE entering transaction ────────
        var tenantId = _tenantService.GetCurrentTenantId();
        if (tenantId == 0)
            return (false, "Tenant context could not be resolved.", 0);

        var doctorId = 0;

        try
        {
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    EmailConfirmed = true,
                    IsActive = true,
                    TenantId = tenantId  // ← stamp on user row
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await _userManager.AddToRoleAsync(user, "Doctor");

                // ── Stamp TenantId claim — this is what was missing ──
                // Without this, doctor login has no TenantId in cookie
                // and the tenant filter returns 0 → no data visible
                await _userManager.AddClaimAsync(user,
                    new Claim("TenantId", tenantId.ToString()));

                // Create doctor profile
                var doctor = _mapper.Map<Doctor>(dto);
                doctor.UserId = user.Id;

                await _uow.Doctors.AddAsync(doctor);
                await _uow.SaveChangesAsync();

                doctorId = doctor.Id;
            });
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message, 0);
        }
        catch (Exception ex)
        {
            return (false, $"An error occurred: {ex.Message}", 0);
        }

        await _auditService.LogAsync(
            action: "Created",
            entityName: "Doctor",
            entityId: doctorId,
            oldValues: null,
            newValues: new { dto.FullName, dto.Specialization, dto.ConsultationFee },
            changes: $"New Doctor registered: {dto.FullName}");

        return (true, "Doctor registered successfully.", doctorId);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateDoctorDto dto)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(dto.Id);
        if (doctor is null)
            return (false, "Doctor not found.");

        var oldValues = new
        {
            doctor.FullName,
            doctor.PhoneNumber,
            doctor.Specialization,
            doctor.Qualification,
            doctor.ConsultationFee,
            doctor.Shift,
            doctor.IsAvailable
        };

        var departmentExists = await _uow.Departments
            .ExistsAsync(d => d.Id == dto.DepartmentId);

        if (!departmentExists)
            return (false, "Selected department does not exist.");

        _mapper.Map(dto, doctor);
        _uow.Doctors.Update(doctor);
        await _uow.SaveChangesAsync();

        await _auditService.LogAsync(
            action: "Updated",
            entityName: "Doctor",
            entityId: doctor.Id,
            oldValues: oldValues,
            newValues: new
            {
                doctor.FullName,
                doctor.PhoneNumber,
                doctor.Specialization,
                doctor.Qualification,
                doctor.ConsultationFee,
                doctor.Shift,
                doctor.IsAvailable
            },
            changes: $"Doctor profile updated: {doctor.FullName}");

        return (true, "Doctor updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var doctor = await _uow.Doctors.GetByIdAsync(id);
        if (doctor is null)
            return (false, "Doctor not found.");

        var hasUpcoming = await _uow.Appointments.ExistsAsync(a =>
            a.DoctorId == id &&
            a.AppointmentDate >= DateTime.Today &&
            a.Status != Domain.Enums.AppointmentStatus.Cancelled);

        if (hasUpcoming)
            return (false, "Cannot delete a doctor who has upcoming appointments.");

        doctor.IsDeleted = true;
        _uow.Doctors.Update(doctor);
        await _uow.SaveChangesAsync();

        await _auditService.LogAsync(
            action: "Deleted",
            entityName: "Doctor",
            entityId: id,
            oldValues: new { doctor.FullName },
            newValues: null,
            changes: $"Doctor deleted: {doctor.FullName}");

        return (true, "Doctor deleted successfully.");
    }
}