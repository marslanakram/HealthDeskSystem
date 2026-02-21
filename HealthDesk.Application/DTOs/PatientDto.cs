using HealthDesk.Domain.Enums;

namespace HealthDesk.Application.DTOs;

// ── Used for displaying patient data ─────────────────────────────
public class PatientDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age => DateTime.Today.Year - DateOfBirth.Year;
    public Gender Gender { get; set; }
    public string GenderDisplay => Gender.ToString();
    public BloodGroup BloodGroup { get; set; }
    public string BloodGroupDisplay => BloodGroup.ToString();
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? MedicalHistory { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Used for creating a new patient ──────────────────────────────
public class CreatePatientDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodGroup BloodGroup { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MedicalHistory { get; set; }
}

// ── Used for editing an existing patient ─────────────────────────
public class UpdatePatientDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodGroup BloodGroup { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MedicalHistory { get; set; }
}
