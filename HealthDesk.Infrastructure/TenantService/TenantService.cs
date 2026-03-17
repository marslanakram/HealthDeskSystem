using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace HealthDesk.Infrastructure.TenantService
{
    public class TenantService : ITenantService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int? _currentTenantId;
        private Tenant? _currentTenant;

        public TenantService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Tenant?> GetTenantByIdentifierAsync(string identifier)
        {
            return await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Identifier == identifier && t.IsActive);
        }

        public async Task<Tenant?> GetTenantByIdAsync(int id)
        {
            return await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Tenants
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, int TenantId)> CreateTenantAsync(
            string name, string identifier)
        {
            // Check if identifier already exists
            var exists = await _context.Tenants
                .AnyAsync(t => t.Identifier == identifier);

            if (exists)
                return (false, "A tenant with this identifier already exists.", 0);

            var tenant = new Tenant
            {
                Name = name,
                Identifier = identifier.ToLower().Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SubscriptionTier = "Basic",
                MaxUsers = 50,
                MaxPatients = 1000
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            return (true, "Tenant created successfully.", tenant.Id);
        }

        public async Task<(bool Success, string Message)> UpdateTenantAsync(int id, string name)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
                return (false, "Tenant not found.");

            tenant.Name = name;
            await _context.SaveChangesAsync();

            return (true, "Tenant updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteTenantAsync(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
                return (false, "Tenant not found.");

            tenant.IsActive = false;
            await _context.SaveChangesAsync();

            return (true, "Tenant deleted successfully.");
        }

        public int GetCurrentTenantId()
        {
            if (_currentTenantId.HasValue)
                return _currentTenantId.Value;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User.IsInRole("SuperAdmin") == true)
            {
                // Check if they've explicitly selected a tenant
                if (httpContext.Session.GetInt32("TenantId").HasValue)
                {
                    return httpContext.Session.GetInt32("TenantId")!.Value;
                }

                // Default to tenant 1 for SuperAdmin if not specified
                return 1;
            }
            if (_currentTenantId.HasValue)
                return _currentTenantId.Value;

            if (httpContext == null)
                return 1;

            // Try to get tenant from session
            var sessionTenant = httpContext.Session.GetInt32("TenantId");
            if (sessionTenant.HasValue)
            {
                _currentTenantId = sessionTenant.Value;
                return _currentTenantId.Value;
            }

            // Try to get from query parameter
            if (httpContext.Request.Query.TryGetValue("tenantId", out var tenantQuery))
            {
                if (int.TryParse(tenantQuery, out var tenantId))
                {
                    _currentTenantId = tenantId;
                    httpContext.Session.SetInt32("TenantId", tenantId);
                    return tenantId;
                }
            }

            // Try to get from user claims (if user is logged in)
            var tenantClaim = httpContext.User.FindFirst("TenantId");
            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var claimTenantId))
            {
                _currentTenantId = claimTenantId;
                return claimTenantId;
            }

            // Default to tenant 1
            _currentTenantId = 1;
            httpContext.Session.SetInt32("TenantId", 1);
            return 1;
        }

        public Tenant? GetCurrentTenant()
        {
            if (_currentTenant != null)
                return _currentTenant;

            var tenantId = GetCurrentTenantId();
            _currentTenant = GetTenantByIdAsync(tenantId).Result;
            return _currentTenant;
        }

        public void SetCurrentTenant(int tenantId)
        {
            _currentTenantId = tenantId;
            _currentTenant = null; // Force reload

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.SetInt32("TenantId", tenantId);
            }
        }
    }
}
