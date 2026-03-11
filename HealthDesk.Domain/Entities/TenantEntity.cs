namespace HealthDesk.Domain.Entities;

public abstract class TenantEntity : BaseEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
