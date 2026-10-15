using System.ComponentModel.DataAnnotations;

namespace HealthDesk.Web.Areas.Admin.Controllers;

public class CreateTenantViewModel
{
    [Required]
    [Display(Name = "Hospital Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Identifier (subdomain)")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Only lowercase letters, numbers, and hyphens")]
    public string Identifier { get; set; } = string.Empty;

    // ── Admin User Details ────────────────────────────────────────
    [Required]
    [EmailAddress]
    [Display(Name = "Admin Email")]
    public string AdminEmail { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Admin Full Name")]
    public string AdminFullName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Admin Password")]
    public string AdminPassword { get; set; } = string.Empty;

    // ── Optional Settings ─────────────────────────────────────────
    [Display(Name = "Subscription Tier")]
    public string SubscriptionTier { get; set; } = "Basic";

    [Display(Name = "Max Users")]
    public int MaxUsers { get; set; } = 50;

    [Display(Name = "Max Patients")]
    public int MaxPatients { get; set; } = 1000;
}
