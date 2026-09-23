using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class AddOnConfiguration : IEntityTypeConfiguration<AddOn>
{
    public void Configure(EntityTypeBuilder<AddOn> builder)
    {
        builder.ToTable("PlatformAddOns");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code).HasMaxLength(64).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(256).IsRequired();
        builder.Property(a => a.MonthlyPrice).HasPrecision(18, 2);
        builder.Property(a => a.Module).HasConversion<int>();
        builder.HasIndex(a => a.Code).IsUnique();
        builder.Property(a => a.RowVersion).IsConcurrencyToken();
    }
}
