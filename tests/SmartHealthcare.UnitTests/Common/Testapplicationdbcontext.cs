using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.UnitTests.Common
{
    /// <summary>
    /// A plain EF Core DbContext (backed by the InMemory provider) that implements
    /// IApplicationDbContext. Handlers under test are written against IApplicationDbContext,
    /// so this lets tests exercise real LINQ/EF query behavior (Where, Select, FirstOrDefaultAsync,
    /// navigation properties, etc.) instead of hand-mocking DbSet, which xUnit + Moq cannot do
    /// reliably for IQueryable async operations.
    /// </summary>
    public class TestApplicationDbContext : DbContext, IApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();

        public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();

        public DbSet<Hospital> Hospitals => Set<Hospital>();

        public DbSet<Appointment> Appointments => Set<Appointment>();

        public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();

        public DbSet<Prescription> Prescriptions => Set<Prescription>();

        public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ApplicationUser is IdentityUser<Guid>; we only need it mapped as a plain
            // entity reachable via navigations (PatientProfile.User, DoctorProfile.User, etc.),
            // not the full ASP.NET Identity store, so no Identity-specific configuration here.
            modelBuilder.Entity<ApplicationUser>(builder =>
            {
                builder.HasKey(u => u.Id);
            });
        }
    }
}