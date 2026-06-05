

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Persistence.Configurations
{
    public class HospitalConfiguration:IEntityTypeConfiguration<Hospital>
    {
        public void Configure(EntityTypeBuilder<Hospital> builder)
        {
            builder.HasIndex(x => x.RohiniCode).IsUnique();
            builder.Property(x => x.RohiniCode).HasMaxLength(50);
            builder.Property(x => x.Name).HasMaxLength(250);
        }
    }
}
