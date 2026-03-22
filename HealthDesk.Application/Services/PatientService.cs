//using AutoMapper;
//using HealthDesk.Application.DTOs;
//using HealthDesk.Application.Interfaces;
//using HealthDesk.Domain.Entities;
//using Microsoft.AspNetCore.Identity;

//namespace HealthDesk.Application.Services;

//public class PatientService : IPatientService
//{
//    private readonly IUnitOfWork _uow;
//    private readonly IMapper _mapper;
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly IAuditService _auditService;

//    public PatientService(
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

//    public async Task<IEnumerable<PatientDto>> GetAllAsync()
//    {
//        var patients = await _uow.Patients.GetAllWithUserAsync();
//        return _mapper.Map<IEnumerable<PatientDto>>(patients);
//    }

//    public async Task<PatientDto?> GetByIdAsync(int id)
//    {
//        var patient = await _uow.Patients.GetWithAppointmentsAsync(id);
//        return patient is null ? null : _mapper.Map<PatientDto>(patient);
//    }

//    public async Task<PatientDto?> GetByUserIdAsync(string userId)
//    {
//        var patient = await _uow.Patients.GetByUserIdAsync(userId);
//        return patient is null ? null : _mapper.Map<PatientDto>(patient);
//    }

//    public async Task<IEnumerable<PatientDto>> SearchAsync(string searchTerm)
//    {
//        var patients = await _uow.Patients.SearchAsync(searchTerm);
//        return _mapper.Map<IEnumerable<PatientDto>>(patients);
//    }

//    public async Task<(bool Success, string Message, int PatientId)> CreateAsync(
//        CreatePatientDto dto)
//    {
//        // Check if email already exists
//        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
//            return (false, "A user with this email already exists.", 0);

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

//            await _userManager.AddToRoleAsync(user, "Patient");

//            // Create patient profile
//            var patient = _mapper.Map<Patient>(dto);
//            patient.UserId = user.Id;

//            await _uow.Patients.AddAsync(patient);
//            await _uow.SaveChangesAsync();
//            await _uow.CommitTransactionAsync();
//            await _auditService.LogAsync(
//              action: "Created",
//              entityName: "Patient",
//              entityId: patient.Id,
//              oldValues: null,
//              newValues: new { patient.FullName, patient.PhoneNumber, patient.DateOfBirth },
//              changes: $"New patient registered: {patient.FullName}"
//              );

//            return (true, "Patient registered successfully.", patient.Id);
//        }
//        catch (Exception ex)
//        {
//            await _uow.RollbackTransactionAsync();
//            return (false, $"An error occurred: {ex.Message}", 0);
//        }
//    }

//    public async Task<(bool Success, string Message)> UpdateAsync(UpdatePatientDto dto)
//    {
//        var patient = await _uow.Patients.GetByIdAsync(dto.Id);
//        if (patient is null)
//            return (false, "Patient not found.");

//        var oldValues = new { patient.FullName, patient.PhoneNumber, patient.Address };


//        _mapper.Map(dto, patient);
//        _uow.Patients.Update(patient);
//        await _uow.SaveChangesAsync();

//        await _auditService.LogAsync(
//        action: "Updated",
//        entityName: "Patient",
//        entityId: patient.Id,
//        oldValues: oldValues,
//        newValues: new { patient.FullName, patient.PhoneNumber, patient.Address },
//        changes: $"Patient profile updated: {patient.FullName}"
//         );

//        return (true, "Patient updated successfully.");
//    }

//    public async Task<(bool Success, string Message)> DeleteAsync(int id)
//    {
//        var patient = await _uow.Patients.GetByIdAsync(id);
//        if (patient is null)
//            return (false, "Patient not found.");

//        // Soft delete
//        patient.IsDeleted = true;
//        _uow.Patients.Update(patient);
//        await _uow.SaveChangesAsync();
//        await _auditService.LogAsync(
//    action: "Deleted",
//    entityName: "Patient",
//    entityId: id,
//    oldValues: new { patient.FullName },
//    newValues: null,
//    changes: $"Patient deleted: {patient.FullName}"
//);

//        return (true, "Patient deleted successfully.");
//    }
//}

using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HealthDesk.Application.Services;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public PatientService(
        IUnitOfWork uow,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService)
    {
        _uow = uow;
        _mapper = mapper;
        _userManager = userManager;
        _auditService = auditService;
    }

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
    {
        var patients = await _uow.Patients.GetAllWithUserAsync();
        return _mapper.Map<IEnumerable<PatientDto>>(patients);
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var patient = await _uow.Patients.GetWithAppointmentsAsync(id);
        return patient is null ? null : _mapper.Map<PatientDto>(patient);
    }

    public async Task<PatientDto?> GetByUserIdAsync(string userId)
    {
        var patient = await _uow.Patients.GetByUserIdAsync(userId);
        return patient is null ? null : _mapper.Map<PatientDto>(patient);
    }

    public async Task<IEnumerable<PatientDto>> SearchAsync(string searchTerm)
    {
        var patients = await _uow.Patients.SearchAsync(searchTerm);
        return _mapper.Map<IEnumerable<PatientDto>>(patients);
    }

    public async Task<(bool Success, string Message, int PatientId)> CreateAsync(
        CreatePatientDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return (false, "A user with this email already exists.", 0);

        var patientId = 0;

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
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await _userManager.AddToRoleAsync(user, "Patient");

                var patient = _mapper.Map<Patient>(dto);
                patient.UserId = user.Id;

                await _uow.Patients.AddAsync(patient);
                await _uow.SaveChangesAsync();

                patientId = patient.Id;
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
            entityName: "Patient",
            entityId: patientId,
            oldValues: null,
            newValues: new { dto.FullName, dto.PhoneNumber },
            changes: $"New patient registered: {dto.FullName}");

        return (true, "Patient registered successfully.", patientId);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdatePatientDto dto)
    {
        var patient = await _uow.Patients.GetByIdAsync(dto.Id);
        if (patient is null)
            return (false, "Patient not found.");

        var oldValues = new
        {
            patient.FullName,
            patient.PhoneNumber,
            patient.Address
        };

        _mapper.Map(dto, patient);
        _uow.Patients.Update(patient);
        await _uow.SaveChangesAsync();

        await _auditService.LogAsync(
            action: "Updated",
            entityName: "Patient",
            entityId: patient.Id,
            oldValues: oldValues,
            newValues: new
            {
                patient.FullName,
                patient.PhoneNumber,
                patient.Address
            },
            changes: $"Patient profile updated: {patient.FullName}");

        return (true, "Patient updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var patient = await _uow.Patients.GetByIdAsync(id);
        if (patient is null)
            return (false, "Patient not found.");

        patient.IsDeleted = true;
        _uow.Patients.Update(patient);
        await _uow.SaveChangesAsync();

        await _auditService.LogAsync(
            action: "Deleted",
            entityName: "Patient",
            entityId: id,
            oldValues: new { patient.FullName },
            newValues: null,
            changes: $"Patient deleted: {patient.FullName}");

        return (true, "Patient deleted successfully.");
    }
}