using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class WardUnitConfiguration : IEntityTypeConfiguration<WardUnit>
{
    public void Configure(EntityTypeBuilder<WardUnit> builder)
    {
        builder.ToTable("WardUnits");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
        builder.Property(u => u.IsActive).IsRequired();
        builder.HasMany(u => u.Beds)
            .WithOne()
            .HasForeignKey(b => b.WardUnitId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(WardUnit.Beds))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class BedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.ToTable("Beds");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(20);
        builder.Property(b => b.SortOrder).IsRequired();
        builder.Property(b => b.IsActive).IsRequired();
        builder.HasIndex(b => b.WardUnitId);
    }
}
