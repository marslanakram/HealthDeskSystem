namespace HealthDesk.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int? TenantId { get; set; }   // nullable for SuperAdmin
    public Tenant? Tenant { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;  // Created / Updated / Deleted / Login / Logout
    public string EntityName { get; set; } = string.Empty; // e.g. "Appointment", "Patient"
    public int? EntityId { get; set; } // The ID of the entity that was modified
    public string? OldValues { get; set; } // JSON of old values
    public string? NewValues { get; set; } // JSON of new values
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Changes { get; set; } // Human-readable description
}
