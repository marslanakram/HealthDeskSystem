namespace HealthDesk.Application.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Changes { get; set; }

    // Display helpers
    public string ActionBadgeClass => Action switch
    {
        "Created" => "bg-success-subtle text-success",
        "Updated" => "bg-primary-subtle text-primary",
        "Deleted" => "bg-danger-subtle text-danger",
        "Login" => "bg-info-subtle text-info",
        "Logout" => "bg-secondary-subtle text-secondary",
        "PermissionChanged" => "bg-warning-subtle text-warning",
        _ => "bg-light text-dark"
    };

    public string ActionIcon => Action switch
    {
        "Created" => "bi-plus-circle-fill",
        "Updated" => "bi-pencil-fill",
        "Deleted" => "bi-trash-fill",
        "Login" => "bi-box-arrow-in-right",
        "Logout" => "bi-box-arrow-left",
        "PermissionChanged" => "bi-key-fill",
        _ => "bi-circle-fill"
    };
}
