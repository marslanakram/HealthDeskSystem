using HealthDesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HealthDesk.Domain.Entities
{
    public class Appointment : TenantEntity
    {
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? ReasonForVisit { get; set; }
        public string? Notes { get; set; }
        public decimal? Fee { get; set; }
        public bool IsPaid { get; set; } = false;

        // Foreign Keys
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        // Navigation
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
        
        [JsonIgnore]
        public Prescription? Prescription { get; set; }
    }
}
