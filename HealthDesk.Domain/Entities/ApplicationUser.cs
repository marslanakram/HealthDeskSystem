using Microsoft.AspNetCore.Identity;
namespace HealthDesk.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int TenantId { get; set; } = 0;

    // Navigation — a user can be linked to a Doctor or Patient profile
    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
}