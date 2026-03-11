using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthDesk.Domain.Entities
{
    public class Prescription : TenantEntity
    {
        public string Diagnosis { get; set; } = string.Empty;
        public string Medicines { get; set; } = string.Empty; // JSON or formatted string
        public string? Instructions { get; set; }
        public string? LabTests { get; set; }
        public DateTime PrescribedDate { get; set; } = DateTime.UtcNow;
        public DateTime? FollowUpDate { get; set; }

        // Foreign Keys
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        // Navigation
        public Appointment Appointment { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }
}
