using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("MedicalRecords");

        builder.HasKey(m => m.Id);
        
        builder.Property(m => m.AppointmentId).IsRequired();
        builder.Property(m => m.VeterinarianId).IsRequired();
        builder.Property(m => m.TutorId).IsRequired();
        builder.Property(m => m.PetId).IsRequired();

        builder.Property(m => m.Diagnosis)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.Prescription)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>();
    }
}
