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
        string[] roles = { "Admin", "Doctor", "Nurse", "Receptionist", "Patient" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // ── Seed default Admin user ───────────────────────────────────
    public static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var email = configuration["SeedAdmin:Email"] ?? "admin@healthdesk.com";
        var password = configuration["SeedAdmin:Password"] ?? "***REMOVED***";
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
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);

            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

public static async Task SeedRolePermissionsAsync(RoleManager<IdentityRole> roleManager)
    {
        // ── Admin gets ALL permissions ────────────────────────────────
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

        // ── Doctor permissions ────────────────────────────────────────
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

        // ── Receptionist permissions ──────────────────────────────────
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

        // ── Nurse permissions ─────────────────────────────────────────
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

        // ── Patient permissions ───────────────────────────────────────
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

