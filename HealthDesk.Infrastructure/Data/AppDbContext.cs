
using HealthDesk.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthDesk.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ── DbSets (Tables) ──────────────────────────────────────────
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base — sets up Identity tables

        // ── Rename Identity tables to cleaner names ───────────────
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.HasDefaultSchema("dbo");

        // ── Department Configuration ──────────────────────────────
        builder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(d => d.Description)
                  .HasMaxLength(500);
            entity.HasQueryFilter(d => !d.IsDeleted); // Global soft-delete filter
        });

        // ── Doctor Configuration ──────────────────────────────────
        builder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FullName)
                  .IsRequired()
                  .HasMaxLength(150);
            entity.Property(d => d.Specialization)
                  .IsRequired()
                  .HasMaxLength(100);
            entity.Property(d => d.ConsultationFee)
                  .HasColumnType("decimal(10,2)");

            // Doctor → Department (Many-to-One)
            entity.HasOne(d => d.Department)
                  .WithMany(dep => dep.Doctors)
                  .HasForeignKey(d => d.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Doctor → ApplicationUser (One-to-One)
            entity.HasOne(d => d.User)
                  .WithOne(u => u.Doctor)
                  .HasForeignKey<Doctor>(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            //entity.HasOne<ApplicationUser>()
            //      .WithOne()
            //       .HasForeignKey<Doctor>(d => d.UserId)
            //       .OnDelete(DeleteBehavior.Cascade);


            entity.HasQueryFilter(d => !d.IsDeleted);
        });

        // ── Patient Configuration ─────────────────────────────────
        builder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FullName)
                  .IsRequired()
                  .HasMaxLength(150);
            entity.Property(p => p.PhoneNumber)
                  .HasMaxLength(20);
            entity.Property(p => p.Address)
                  .HasMaxLength(300);

            //Patient → ApplicationUser(One - to - One)
            entity.HasOne(p => p.User)
                  .WithOne(u => u.Patient)
                  .HasForeignKey<Patient>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            //entity.HasOne<ApplicationUser>()
            //      .WithOne()
            //      .HasForeignKey<Patient>(p => p.UserId)
            //      .OnDelete(DeleteBehavior.Cascade);


            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ── Appointment Configuration ─────────────────────────────
        builder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.ReasonForVisit)
                  .HasMaxLength(500);
            entity.Property(a => a.Notes)
                  .HasMaxLength(1000);
            entity.Property(a => a.Fee)
                  .HasColumnType("decimal(10,2)");

            // Appointment → Patient (Many-to-One)
            entity.HasOne(a => a.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Appointment → Doctor (Many-to-One)
            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        // ── Prescription Configuration ────────────────────────────
        builder.Entity<Prescription>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Diagnosis)
                  .IsRequired()
                  .HasMaxLength(500);
            entity.Property(p => p.Medicines)
                  .IsRequired();
            entity.Property(p => p.Instructions)
                  .HasMaxLength(1000);

            // Prescription → Appointment (One-to-One)
            entity.HasOne(p => p.Appointment)
                  .WithOne(a => a.Prescription)
                  .HasForeignKey<Prescription>(p => p.AppointmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Prescription → Patient
            entity.HasOne(p => p.Patient)
                  .WithMany(pat => pat.Prescriptions)
                  .HasForeignKey(p => p.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Prescription → Doctor
            entity.HasOne(p => p.Doctor)
                  .WithMany(d => d.Prescriptions)
                  .HasForeignKey(p => p.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ── AuditLog Configuration ────────────────────────────────
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Action).HasMaxLength(50);
            entity.Property(a => a.EntityName).HasMaxLength(100);
            entity.Property(a => a.UserName).HasMaxLength(150);
            entity.Property(a => a.IpAddress).HasMaxLength(45);
        });
    }

    // ── Auto-set UpdatedAt on SaveChanges ─────────────────────────
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}