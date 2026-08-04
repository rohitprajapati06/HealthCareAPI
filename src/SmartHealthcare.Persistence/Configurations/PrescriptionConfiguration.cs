

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Persistence.Configurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.HasOne(x => x.Appointment)
                .WithOne(p => p.Prescription)
                .HasForeignKey<Prescription>(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
