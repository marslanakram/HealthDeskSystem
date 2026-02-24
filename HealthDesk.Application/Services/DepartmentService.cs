using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using static HealthDesk.Domain.Constants.PermissionConstants;

namespace HealthDesk.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;

    public DepartmentService(IUnitOfWork uow, IMapper mapper, IAuditService auditService)
    {
        _uow = uow;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
    {
        var departments = await _uow.Departments.GetAllActiveAsync();
        return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var dept = await _uow.Departments.GetWithDoctorsAsync(id);
        return dept is null ? null : _mapper.Map<DepartmentDto>(dept);
    }

    public async Task<(bool Success, string Message)> CreateAsync(CreateDepartmentDto dto)
    {
        var exists = await _uow.Departments
            .ExistsAsync(d => d.Name == dto.Name);

        if (exists)
            return (false, "A department with this name already exists.");

        var department = _mapper.Map<Department>(dto);
        await _uow.Departments.AddAsync(department);
        await _uow.SaveChangesAsync();
        await _auditService.LogAsync(
    action: "Created",
    entityName: "Department",
    entityId: department.Id,
    oldValues: null,
    newValues: new { department.Name, department.Description, department.Location, department.IsActive },
    changes: $"New department registered: {department.Name}"
);

        return (true, "Department created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateDepartmentDto dto)
    {
        var department = await _uow.Departments.GetByIdAsync(dto.Id);
        if (department is null)
            return (false, "Department not found.");

        var oldValues = new { department.Name, department.Description, department.Location, department.IsActive };

        _mapper.Map(dto, department);
        _uow.Departments.Update(department);
        await _uow.SaveChangesAsync();
        await _auditService.LogAsync(
    action: "Updated",
    entityName: "Department",
    entityId: department.Id,
    oldValues: oldValues,
    newValues: new { department.Name, department.Description, department.Location, department.IsActive },
    changes: $"Department profile updated: {department.Name}"
);

        return (true, "Department updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var hasDoctors = await _uow.Departments.HasDoctorsAsync(id);
        if (hasDoctors)
            return (false, "Cannot delete a department that has doctors assigned.");

        var department = await _uow.Departments.GetByIdAsync(id);
        if (department is null)
            return (false, "Department not found.");

        department.IsDeleted = true;
        _uow.Departments.Update(department);
        await _uow.SaveChangesAsync();
        await _auditService.LogAsync(
    action: "Deleted",
    entityName: "Department",
    entityId: id,
    oldValues: new { department.Name},
    newValues: null,
    changes: $"Department deleted: {department.Name}"
);

        return (true, "Department deleted successfully.");
    }
}
