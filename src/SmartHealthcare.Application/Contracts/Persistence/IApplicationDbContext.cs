using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Domain.Entities;


namespace SmartHealthcare.Application.Contracts.Persistence
{
    public interface IApplicationDbContext
    {
        DbSet<RefreshToken> RefreshTokens { get; }

        DbSet<PatientProfile> PatientProfiles { get; }

        DbSet<DoctorProfile> DoctorProfiles { get; }

        DbSet<Hospital> Hospitals { get; }

        DbSet<Appointment> Appointments { get; }

        DbSet<AvailabilitySlot> AvailabilitySlots { get; }



        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
