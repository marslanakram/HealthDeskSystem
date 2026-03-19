//using HealthDesk.Application.Interfaces;
//using HealthDesk.Application.Mappings;
//using HealthDesk.Application.Services;
//using HealthDesk.Domain.Entities;
//using HealthDesk.Infrastructure.Authorization;
//using HealthDesk.Infrastructure.Data;
//using HealthDesk.Infrastructure.Repositories;
//using HealthDesk.Infrastructure.Seeds;
//using HealthDesk.Infrastructure.TenantService;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Serilog;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

//var builder = WebApplication.CreateBuilder(args);

//// ── Serilog ──────────────────────────────────────────────────────
//builder.Host.UseSerilog((context, config) =>
//{
//    config.ReadFrom.Configuration(context.Configuration)
//          .WriteTo.Console()
//          .WriteTo.File("Logs/healthdesk-.log", rollingInterval: RollingInterval.Day);
//});

//// ── Database ─────────────────────────────────────────────────────
////builder.Services.AddDbContext<AppDbContext>(options =>
////    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
//{
//    var interceptor = serviceProvider.GetRequiredService<TenantSaveChangesInterceptor>();

//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        sqlOptions =>
//        {
//            sqlOptions.MigrationsAssembly("HealthDesk.Infrastructure");
//            sqlOptions.EnableRetryOnFailure(
//                maxRetryCount: 5,
//                maxRetryDelay: TimeSpan.FromSeconds(30),
//                errorNumbersToAdd: null);
//        })
//        .AddInterceptors(interceptor);  // ← Auto-set TenantId

//    if (builder.Environment.IsDevelopment())
//    {
//        options.EnableSensitiveDataLogging();
//        options.EnableDetailedErrors();
//    }
//});

//// ── Identity ─────────────────────────────────────────────────────
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
//{
//    options.Password.RequiredLength = 8;
//    options.Password.RequireDigit = true;
//    options.Password.RequireUppercase = true;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Lockout.MaxFailedAccessAttempts = 5;
//    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
//    options.User.RequireUniqueEmail = true;
//})
//.AddEntityFrameworkStores<AppDbContext>()
//.AddDefaultTokenProviders();

//// ── Cookie settings ───────────────────────────────────────────────
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/Account/Login";
//    options.LogoutPath = "/Account/Logout";
//    options.AccessDeniedPath = "/Account/AccessDenied";
//    options.ExpireTimeSpan = TimeSpan.FromHours(8);
//    options.SlidingExpiration = true;
//});

//// ── Authorization Policies ────────────────────────────────────────
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
//    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin", "SuperAdmin"));
//    options.AddPolicy("DoctorOrAdmin", p => p.RequireRole("Doctor", "Admin", "SuperAdmin"));
//    options.AddPolicy("ReceptionistOrAdmin", p => p.RequireRole("Receptionist", "Admin", "SuperAdmin"));
//    options.AddPolicy("MedicalStaff", p => p.RequireRole("Doctor", "Nurse", "Admin", "SuperAdmin"));
//    options.AddPolicy("AllStaff", p => p.RequireRole("Admin", "Doctor", "Nurse", "Receptionist", "SuperAdmin"));
//});

//// ── Permission-Based Authorization ────────────────────────────────
//builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
//builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

//// ── Repository & Unit of Work ─────────────────────────────────────
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped<IPatientRepository, PatientRepository>();
//builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
//builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
//builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
//builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
//builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

//// ── Application Services ──────────────────────────────────────────
//builder.Services.AddScoped<IPatientService, PatientService>();
//builder.Services.AddScoped<IDoctorService, DoctorService>();
//builder.Services.AddScoped<IAppointmentService, AppointmentService>();
//builder.Services.AddScoped<IDepartmentService, DepartmentService>();
//builder.Services.AddScoped<IDashboardService, DashboardService>();
//builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
//builder.Services.AddScoped<IAuditService, AuditService>();
//builder.Services.AddScoped<ITenantService, TenantService>();
//builder.Services.AddScoped<TenantSaveChangesInterceptor>();

//// ── AutoMapper ────────────────────────────────────────────────────
//builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

//// ── MVC ───────────────────────────────────────────────────────────
//builder.Services.AddControllersWithViews();
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddMemoryCache();
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    //options.IdleTimeout = TimeSpan.FromMinutes(30);
//    options.IdleTimeout = TimeSpan.FromHours(2);
//    options.Cookie.HttpOnly = true;
//    options.Cookie.IsEssential = true;
//});

//var app = builder.Build();

//// ── Seed database on startup ──────────────────────────────────────
//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//    await context.Database.MigrateAsync();

//    // Create default tenant if none exist
//    if (!await context.Tenants.AnyAsync())
//    {
//        var defaultTenant = new Tenant
//        {
//            Name = "Default Hospital",
//            Identifier = "default",
//            IsActive = true,
//            CreatedAt = DateTime.UtcNow,
//            SubscriptionTier = "Enterprise",
//            MaxUsers = 1000,
//            MaxPatients = 10000,
//            ContactEmail = "admin@healthdesk.com",
//            PrimaryColor = "#0d6efd"
//        };

//        context.Tenants.Add(defaultTenant);
//        await context.SaveChangesAsync();
//    }

//    var services = scope.ServiceProvider;
//    //var context = services.GetRequiredService<AppDbContext>();
//    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
//    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

//    await context.Database.MigrateAsync();
//    await DatabaseSeeder.SeedRolesAsync(roleManager);
//    await DatabaseSeeder.SeedSuperAdminAsync(userManager, builder.Configuration);
//    await DatabaseSeeder.SeedAdminUserAsync(userManager, builder.Configuration);
//    await DatabaseSeeder.SeedRolePermissionsAsync(roleManager);
//}

//// ── Middleware Pipeline ───────────────────────────────────────────
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseSerilogRequestLogging();
//app.UseSession();
//app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "areas",
//    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//await app.RunAsync();

using HealthDesk.Application.Interfaces;
using HealthDesk.Application.Mappings;
using HealthDesk.Application.Services;
using HealthDesk.Domain.Entities;
using HealthDesk.Infrastructure.Authorization;
using HealthDesk.Infrastructure.Data;
using HealthDesk.Infrastructure.Repositories;
using HealthDesk.Infrastructure.Seeds;
using HealthDesk.Infrastructure.TenantServices;
using HealthDesk.Web.Identity;
using HealthDesk.Web.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Logging (Serilog) ─────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/healthdesk-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Session ───────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Services ──────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<TenantSaveChangesInterceptor>();

// ── Database Context ──────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<TenantSaveChangesInterceptor>();

    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("HealthDesk.Infrastructure");
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        })
        .AddInterceptors(interceptor);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ── Identity ──────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

// ── Authorization Policies ────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));

    options.AddPolicy("DoctorOrAdmin", policy =>
        policy.RequireRole("Doctor", "Admin", "SuperAdmin"));

    options.AddPolicy("ReceptionistOrAdmin", policy =>
        policy.RequireRole("Receptionist", "Admin", "SuperAdmin"));

    options.AddPolicy("MedicalStaff", policy =>
        policy.RequireRole("Doctor", "Nurse", "Admin", "SuperAdmin"));

    options.AddPolicy("AllStaff", policy =>
        policy.RequireRole("Doctor", "Nurse", "Receptionist", "Admin", "SuperAdmin"));
});

//── Permission-Based Authorization ────────────────────────────────
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();


// ── AutoMapper ────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// ── Repository & UnitOfWork ───────────────────────────────────
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── Application Services ──────────────────────────────────────
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITenantPermissionService, TenantPermissionService>();

// ── MVC ───────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseTenantResolution();
app.UseAuthentication();
app.UseAuthorization();

// ── Seed Database ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await context.Database.MigrateAsync();

    // Create default tenant if none exist
    if (!await context.Tenants.AnyAsync())
    {
        var defaultTenant = new Tenant
        {
            Name = "Default Hospital",
            Identifier = "default",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            SubscriptionTier = "Enterprise",
            MaxUsers = 1000,
            MaxPatients = 10000,
            ContactEmail = "admin@healthdesk.com",
            PrimaryColor = "#0d6efd"
        };

        context.Tenants.Add(defaultTenant);
        await context.SaveChangesAsync();
    }

    await DatabaseSeeder.SeedRolesAsync(roleManager);
    await DatabaseSeeder.SeedSuperAdminAsync(userManager, builder.Configuration);
    await DatabaseSeeder.SeedAdminUserAsync(userManager, builder.Configuration);
    //await DatabaseSeeder.SeedStaffUsersAsync(userManager);
    await DatabaseSeeder.SeedRolePermissionsAsync(roleManager);

    var permService = services
        .GetRequiredService<ITenantPermissionService>();

    var allTenants = await context.Tenants
        .Where(t => t.IsActive)
        .ToListAsync();

    foreach (var tenant in allTenants)
        await permService.SeedDefaultPermissionsAsync(tenant.Id);
}

// ── Routes ────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
