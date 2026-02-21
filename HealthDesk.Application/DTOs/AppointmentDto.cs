using HealthDesk.Domain.Enums;

namespace HealthDesk.Application.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string TimeDisplay => DateTime.Today.Add(AppointmentTime).ToString("hh:mm tt");
    public AppointmentStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public string? ReasonForVisit { get; set; }
    public string? Notes { get; set; }
    public decimal? Fee { get; set; }
    public bool IsPaid { get; set; }

    // Related data
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public bool HasPrescription { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAppointmentDto
{
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string? ReasonForVisit { get; set; }
    public decimal? Fee { get; set; }
}

public class UpdateAppointmentDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsPaid { get; set; }
}
