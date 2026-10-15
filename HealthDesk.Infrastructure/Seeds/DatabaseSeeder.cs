using HealthDesk.Domain.Constants;
using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace HealthDesk.Infrastructure.Seeds;

public static class DatabaseSeeder
{
    // ── Seed all roles ────────────────────────────────────────────
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        // SuperAdmin = platform level (YOU, the SaaS owner)
        // Admin      = tenant level  (each hospital's administrator)
        string[] roles = { "SuperAdmin", "Admin", "Doctor", "Nurse", "Receptionist", "Patient" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // ── Seed SuperAdmin (platform level, TenantId = 0) ────────────
    public static async Task SeedSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var superAdminEmail = configuration["SeedSuperAdmin:Email"]
            ?? "superadmin@healthdesk.com";
        var superAdminPassword = configuration["SeedSuperAdmin:Password"]
            ?? throw new InvalidOperationException(
        "SeedSuperAdmin:Password is missing from configuration.");

        var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdmin is null)
        {
            superAdmin = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FullName = "Super Administrator",
                EmailConfirmed = true,
                IsActive = true,
                TenantId = 0,   // Platform level — does NOT belong to any tenant
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(superAdmin, superAdminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                // SuperAdmin intentionally has NO TenantId claim
                // This is what lets the middleware/service know to skip the filter
            }
        }
    }

    // ── Seed default Admin user (tenant level, TenantId = 1) ─────
    public static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var email = configuration["SeedAdmin:Email"] ?? "admin@healthdesk.com";
        var password = configuration["SeedAdmin:Password"] ?? throw new InvalidOperationException(
        "SeedAdmin:Password is missing from configuration.");
        var name = configuration["SeedAdmin:FullName"] ?? "System Administrator";

        if (await userManager.FindByEmailAsync(email) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                EmailConfirmed = true,
                IsActive = true,
                TenantId = 1,   // Belongs to the default tenant (seeded first)
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");

                // ── TenantId claim is CRITICAL ─────────────────────
                // CustomUserClaimsPrincipalFactory loads this at login
                // so the middleware can resolve the correct tenant
                await userManager.AddClaimAsync(admin,
                    new Claim("TenantId", "1"));
            }
        }
    }

    // ── Seed role permissions ─────────────────────────────────────
    public static async Task SeedRolePermissionsAsync(RoleManager<IdentityRole> roleManager)
    {
        // ── Admin gets ALL permissions ────────────────────────────
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole is not null)
        {
            var allPermissions = PermissionConstants.GetAllPermissions();
            foreach (var permission in allPermissions)
            {
                var claims = await roleManager.GetClaimsAsync(adminRole);
                if (!claims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(adminRole,
                        new Claim("Permission", permission));
                }
            }
        }

        // ── Doctor permissions ────────────────────────────────────
        var doctorRole = await roleManager.FindByNameAsync("Doctor");
        if (doctorRole is not null)
        {
            var doctorPermissions = new[]
            {
                PermissionConstants.Patients.View,
                PermissionConstants.Appointments.View,
                PermissionConstants.Appointments.UpdateStatus,
                PermissionConstants.Prescriptions.View,
                PermissionConstants.Prescriptions.Create
            };

            foreach (var permission in doctorPermissions)
            {
                var claims = await roleManager.GetClaimsAsync(doctorRole);
                if (!claims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(doctorRole,
                        new Claim("Permission", permission));
                }
            }
        }

        // ── Receptionist permissions ──────────────────────────────
        var receptionistRole = await roleManager.FindByNameAsync("Receptionist");
        if (receptionistRole is not null)
        {
            var receptionistPermissions = new[]
            {
                PermissionConstants.Patients.View,
                PermissionConstants.Patients.Create,
                PermissionConstants.Patients.Edit,
                PermissionConstants.Appointments.View,
                PermissionConstants.Appointments.Create,
                PermissionConstants.Appointments.Edit,
                PermissionConstants.Doctors.View,
                PermissionConstants.Departments.View
            };

            foreach (var permission in receptionistPermissions)
            {
                var claims = await roleManager.GetClaimsAsync(receptionistRole);
                if (!claims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(receptionistRole,
                        new Claim("Permission", permission));
                }
            }
        }

        // ── Nurse permissions ─────────────────────────────────────
        var nurseRole = await roleManager.FindByNameAsync("Nurse");
        if (nurseRole is not null)
        {
            var nursePermissions = new[]
            {
                PermissionConstants.Patients.View,
                PermissionConstants.Appointments.View,
                PermissionConstants.Doctors.View,
                PermissionConstants.Departments.View
            };

            foreach (var permission in nursePermissions)
            {
                var claims = await roleManager.GetClaimsAsync(nurseRole);
                if (!claims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(nurseRole,
                        new Claim("Permission", permission));
                }
            }
        }

        // ── Patient permissions ───────────────────────────────────
        var patientRole = await roleManager.FindByNameAsync("Patient");
        if (patientRole is not null)
        {
            var patientPermissions = new[]
            {
                PermissionConstants.Appointments.View,
                PermissionConstants.Prescriptions.View
            };

            foreach (var permission in patientPermissions)
            {
                var claims = await roleManager.GetClaimsAsync(patientRole);
                if (!claims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(patientRole,
                        new Claim("Permission", permission));
                }
            }
        }
    }
}