using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petshop.Domain.Entities;

namespace Clients.Infrastructure.Persistence.Configurations;

internal sealed class OfflineGroomingAppointmentConfiguration : IEntityTypeConfiguration<GroomingAppointment>
{
    public void Configure(EntityTypeBuilder<GroomingAppointment> builder)
    {
        builder.ToTable("GroomingAppointments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Notes).HasMaxLength(500);
    }
}

internal sealed class OfflineGroomingSlotConfiguration : IEntityTypeConfiguration<GroomingSlot>
{
    public void Configure(EntityTypeBuilder<GroomingSlot> builder)
    {
        builder.ToTable("GroomingSlots");
        builder.HasKey(s => s.Id);
    }
}

internal sealed class OfflineGroomingRecordConfiguration : IEntityTypeConfiguration<GroomingRecord>
{
    public void Configure(EntityTypeBuilder<GroomingRecord> builder)
    {
        builder.ToTable("GroomingRecords");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.CoatNotes).HasMaxLength(2000);
        builder.Property(r => r.Status).HasConversion<int>();
        builder.HasMany(r => r.SupplyLines).WithOne().HasForeignKey(l => l.GroomingRecordId).OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(GroomingRecord.SupplyLines))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class OfflineGroomingServiceConfiguration : IEntityTypeConfiguration<GroomingService>
{
    public void Configure(EntityTypeBuilder<GroomingService> builder)
    {
        builder.ToTable("GroomingServices");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(150);
        builder.Property(s => s.ServiceType).HasConversion<string>().HasMaxLength(20);
        builder.HasMany(s => s.DefaultSupplies).WithOne().HasForeignKey(l => l.GroomingServiceId).OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(GroomingService.DefaultSupplies))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
