using HealthDesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthDesk.Domain.Entities
{
    public class Doctor : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? Qualification { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public decimal ConsultationFee { get; set; }
        public ShiftType Shift { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Foreign Key
        public int DepartmentId { get; set; }

        // Navigation
        public Department Department { get; set; } = null!;

        
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}

