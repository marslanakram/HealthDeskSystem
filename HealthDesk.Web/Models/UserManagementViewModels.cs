using System.ComponentModel.DataAnnotations;

namespace HealthDesk.Web.Models;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, MinimumLength = 3)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a role")]
    public string Role { get; set; } = string.Empty;
}