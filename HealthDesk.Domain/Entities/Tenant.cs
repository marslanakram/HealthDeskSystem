namespace HealthDesk.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  // Hospital Name
    public string Identifier { get; set; } = string.Empty;  // Unique code
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Settings
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; } = "#0d6efd";
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }

    // Subscription
    public string SubscriptionTier { get; set; } = "Basic";  // Basic, Pro, Enterprise
    public int MaxUsers { get; set; } = 50;
    public int MaxPatients { get; set; } = 1000;
    public DateTime? SubscriptionEndDate { get; set; }
}
