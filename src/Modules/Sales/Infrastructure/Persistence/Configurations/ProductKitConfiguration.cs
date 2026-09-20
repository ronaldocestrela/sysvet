using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public sealed class ProductKitConfiguration : IEntityTypeConfiguration<ProductKit>
{
    public void Configure(EntityTypeBuilder<ProductKit> builder)
    {
        builder.ToTable("ProductKits");
        builder.HasKey(k => k.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.Property(k => k.Name).HasMaxLength(150).IsRequired();
        builder.Property(k => k.IsActive).IsRequired();
        builder.Property(k => k.RowVersion).IsConcurrencyToken();
        builder.HasMany(k => k.Components)
            .WithOne()
            .HasForeignKey(c => c.ProductKitId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(k => k.Components).HasField("_components");
    }
}
