

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Persistence.Configurations
{
    public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
    {
        public void Configure(EntityTypeBuilder<PatientProfile> builder)
        {
            builder.HasOne(u => u.User)
                .WithOne(p => p.PatientProfile)
                .HasForeignKey<PatientProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
