using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace HealthDesk.Web.Identity;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
        _userManager = userManager;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        // Get default claims (Id, Name, Roles, etc.)
        var identity = await base.GenerateClaimsAsync(user);

        // ── Load ALL user claims from database ────────────────────
        var userClaims = await _userManager.GetClaimsAsync(user);

        // Add each claim to the identity
        identity.AddClaims(userClaims);

        // Debug logging
        var tenantClaim = userClaims.FirstOrDefault(c => c.Type == "TenantId");
        if (tenantClaim != null)
        {
            Console.WriteLine($"[ClaimsPrincipal] ✅ Loaded TenantId claim: {tenantClaim.Value} for user {user.Email}");
        }
        else
        {
            Console.WriteLine($"[ClaimsPrincipal] ❌ WARNING: No TenantId claim found for user {user.Email}");
        }

        return identity;
    }
}
