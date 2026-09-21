using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petshop.Domain.Entities;

namespace Petshop.Infrastructure.Persistence.Configurations;

internal sealed class GroomingServiceConfiguration : IEntityTypeConfiguration<GroomingService>
{
    public void Configure(EntityTypeBuilder<GroomingService> builder)
    {
        builder.ToTable("GroomingServices");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
        builder.Property(s => s.ServiceType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.DurationInMinutes).IsRequired();
        builder.Property(s => s.PrepaidServiceCode).HasMaxLength(20);
        builder.Property(s => s.IsActive).IsRequired();

        builder.HasMany(s => s.DefaultSupplies)
            .WithOne()
            .HasForeignKey(l => l.GroomingServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(GroomingService.DefaultSupplies))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class GroomingServiceSupplyLineConfiguration : IEntityTypeConfiguration<GroomingServiceSupplyLine>
{
    public void Configure(EntityTypeBuilder<GroomingServiceSupplyLine> builder)
    {
        builder.ToTable("GroomingServiceSupplyLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.GroomingServiceId).IsRequired();
        builder.Property(l => l.ProductId).IsRequired();
        builder.Property(l => l.Quantity).HasPrecision(18, 4).IsRequired();
    }
}
