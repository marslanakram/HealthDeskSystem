using System.ComponentModel.DataAnnotations;

namespace HealthDesk.Application.DTOs;

public class PrescriptionDto
{
    public int Id { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Medicines { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public string? LabTests { get; set; }
    public DateTime PrescribedDate { get; set; }
    public DateTime? FollowUpDate { get; set; }

    // Related data
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
}

public class CreatePrescriptionDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Diagnosis is required")]
    [StringLength(500, MinimumLength = 10)]
    public string Diagnosis { get; set; } = string.Empty;

    [Required(ErrorMessage = "Medicines are required")]
    [StringLength(2000, MinimumLength = 10)]
    public string Medicines { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Instructions { get; set; }

    [StringLength(500)]
    public string? LabTests { get; set; }

    public DateTime? FollowUpDate { get; set; }
}