using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HealthDesk.Domain.Enums;

namespace HealthDesk.Domain.Entities
{
    public class Department : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
