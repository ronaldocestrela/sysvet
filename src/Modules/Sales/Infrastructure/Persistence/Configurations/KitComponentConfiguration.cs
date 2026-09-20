using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public sealed class KitComponentConfiguration : IEntityTypeConfiguration<KitComponent>
{
    public void Configure(EntityTypeBuilder<KitComponent> builder)
    {
        builder.ToTable("KitComponents");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.QuantityPerKit).HasPrecision(18, 2);
    }
}
