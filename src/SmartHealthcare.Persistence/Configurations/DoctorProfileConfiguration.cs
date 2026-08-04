using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Persistence.Configurations
{
    public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
    {
        public void Configure(EntityTypeBuilder<DoctorProfile> builder)
        {
            builder.HasOne(u => u.User)
                .WithOne(d => d.DoctorProfile)
                .HasForeignKey<DoctorProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
