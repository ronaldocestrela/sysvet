using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class IssuedPrescriptionConfiguration : IEntityTypeConfiguration<IssuedPrescription>
{
    public void Configure(EntityTypeBuilder<IssuedPrescription> builder)
    {
        builder.ToTable("IssuedPrescriptions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Status).IsRequired().HasConversion<int>();
        builder.HasIndex(p => p.AppointmentId);
        builder.HasIndex(p => p.PetId);
        builder.Property(p => p.RowVersion).IsConcurrencyToken();
        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.IssuedPrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(IssuedPrescription.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.MedicationName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Concentration).HasMaxLength(200);
        builder.Property(i => i.Dose).HasMaxLength(200);
        builder.Property(i => i.Route).HasMaxLength(100);
        builder.Property(i => i.Frequency).HasMaxLength(200);
        builder.Property(i => i.Duration).HasMaxLength(200);
        builder.Property(i => i.Instructions).HasMaxLength(1000);
        builder.HasIndex(i => i.IssuedPrescriptionId);
    }
}
