using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;


namespace SmartHealthcare.Persistence.Contexts;
public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>,
      IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hospital> Hospitals { get; set; }

    public DbSet<DoctorProfile> DoctorProfiles { get; set; }

    public DbSet<PatientProfile> PatientProfiles { get; set; }

    public DbSet<Appointment> Appointments { get; set; }

    public DbSet<Prescription> Prescriptions { get; set; }

    public DbSet<MedicalRecord> MedicalRecords { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<AvailabilitySlot> AvailabilitySlots { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ConfigureIdentityTables(builder);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");

        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");

        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");

        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");

        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
    }
}