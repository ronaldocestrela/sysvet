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
        builder.HasIndex(m => m.AppointmentId).IsUnique();
        builder.HasIndex(m => m.PetId);

        builder.Property(m => m.VeterinarianId).IsRequired();
        builder.Property(m => m.TutorId).IsRequired();
        builder.Property(m => m.PetId).IsRequired();

        builder.Property(m => m.Anamnesis)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(m => m.Diagnosis)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.Prescription)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.FollowUpOn);

        builder.OwnsOne(m => m.VitalSigns, vitals =>
        {
            vitals.Property(v => v.WeightKg).HasColumnName("VitalWeightKg");
            vitals.Property(v => v.TemperatureC).HasColumnName("VitalTemperatureC");
            vitals.Property(v => v.HeartRateBpm).HasColumnName("VitalHeartRateBpm");
            vitals.Property(v => v.RespiratoryRateBpm).HasColumnName("VitalRespiratoryRateBpm");
            vitals.Property(v => v.MucousMembranes).HasColumnName("VitalMucousMembranes").HasMaxLength(200);
            vitals.Property(v => v.CapillaryRefillTime).HasColumnName("VitalCapillaryRefillTime").HasMaxLength(200);
            vitals.Property(v => v.MeasuredAt).HasColumnName("VitalMeasuredAt");
        });

        builder.HasMany(m => m.EvolutionNotes)
            .WithOne()
            .HasForeignKey(n => n.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(MedicalRecord.EvolutionNotes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
