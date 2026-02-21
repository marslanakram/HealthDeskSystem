using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;

namespace HealthDesk.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public DepartmentService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
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

        return (true, "Department created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateDepartmentDto dto)
    {
        var department = await _uow.Departments.GetByIdAsync(dto.Id);
        if (department is null)
            return (false, "Department not found.");

        _mapper.Map(dto, department);
        _uow.Departments.Update(department);
        await _uow.SaveChangesAsync();

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

        return (true, "Department deleted successfully.");
    }
}
