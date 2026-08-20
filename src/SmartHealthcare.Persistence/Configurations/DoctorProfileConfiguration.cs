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

            builder.Property(x => x.Specialization)
            .HasMaxLength(150)
            .IsRequired();

            builder.Property(x => x.Qualification)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.ConsultationFee)
                .HasPrecision(18, 2);

            builder.Property(x => x.ExperienceYears)
                .IsRequired();

            builder.Property(x => x.ApprovalStatus)
                .IsRequired();
        }
    }
}
