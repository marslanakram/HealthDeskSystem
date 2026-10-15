namespace HealthDesk.Domain.Entities;

public class TenantRolePermission
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string RoleName { get; set; } = string.Empty; 
    public string Permission { get; set; } = string.Empty; 

    public Tenant? Tenant { get; set; }
}
