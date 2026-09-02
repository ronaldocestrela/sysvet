using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(s => s.Id);

        builder.Property<Guid>("TenantId").IsRequired();

        builder.Property(s => s.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Quantity).HasPrecision(18, 4);
        builder.Property(s => s.BatchNumber).HasMaxLength(50);
        builder.Property(s => s.Reason).IsRequired().HasMaxLength(200);

        builder.HasOne<Product>()
               .WithMany()
               .HasForeignKey(s => s.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.RowVersion).IsConcurrencyToken();
    }
}
