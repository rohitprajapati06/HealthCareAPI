

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Persistence.Configurations
{
    public class MedicalRecordConfiguration :IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.HasOne(p => p.Patient)
                .WithMany(m => m.MedicalRecords)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Hospital)
                .WithMany()
                .HasForeignKey(h => h.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PatientId);

        }
    }
}
