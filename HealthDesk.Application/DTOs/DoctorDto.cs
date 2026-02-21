using HealthDesk.Domain.Enums;

namespace HealthDesk.Application.DTOs;

public class DoctorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public decimal ConsultationFee { get; set; }
    public ShiftType Shift { get; set; }
    public string ShiftDisplay => Shift.ToString();
    public bool IsAvailable { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateDoctorDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public string? PhoneNumber { get; set; }
    public decimal ConsultationFee { get; set; }
    public ShiftType Shift { get; set; }
    public int DepartmentId { get; set; }
}

public class UpdateDoctorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public string? PhoneNumber { get; set; }
    public decimal ConsultationFee { get; set; }
    public ShiftType Shift { get; set; }
    public bool IsAvailable { get; set; }
    public int DepartmentId { get; set; }
}
