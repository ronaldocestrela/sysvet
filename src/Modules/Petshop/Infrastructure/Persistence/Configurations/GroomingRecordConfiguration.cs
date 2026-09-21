using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petshop.Domain.Entities;

namespace Petshop.Infrastructure.Persistence.Configurations;

internal sealed class GroomingRecordConfiguration : IEntityTypeConfiguration<GroomingRecord>
{
    public void Configure(EntityTypeBuilder<GroomingRecord> builder)
    {
        builder.ToTable("GroomingRecords");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.GroomingAppointmentId).IsRequired();
        builder.HasIndex(r => r.GroomingAppointmentId).IsUnique();
        builder.HasIndex(r => r.PetId);
        builder.Property(r => r.GroomerId).IsRequired();
        builder.Property(r => r.TutorId).IsRequired();
        builder.Property(r => r.PetId).IsRequired();
        builder.Property(r => r.CoatNotes).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.Status).HasConversion<int>().IsRequired();

        builder.HasMany(r => r.SupplyLines)
            .WithOne()
            .HasForeignKey(l => l.GroomingRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(GroomingRecord.SupplyLines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class GroomingRecordSupplyLineConfiguration : IEntityTypeConfiguration<GroomingRecordSupplyLine>
{
    public void Configure(EntityTypeBuilder<GroomingRecordSupplyLine> builder)
    {
        builder.ToTable("GroomingRecordSupplyLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.GroomingRecordId).IsRequired();
        builder.Property(l => l.ProductId).IsRequired();
        builder.Property(l => l.Quantity).HasPrecision(18, 4).IsRequired();
    }
}
