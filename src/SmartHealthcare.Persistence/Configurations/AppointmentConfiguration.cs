using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Persistence.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasOne(p => p.Patient)
                .WithMany(a => a.Appointments)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Doctor)
                .WithMany(a => a.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Hospital)
                .WithMany(a => a.Appointments)
                .HasForeignKey(h => h.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AvailabilitySlot)
                .WithMany(a => a.Appointments)
                .HasForeignKey(x => x.AvailabilitySlotId)
                .OnDelete(DeleteBehavior.Restrict);
                
        }
    }
}
