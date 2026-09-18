using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clients.Infrastructure.Persistence.Configurations;

/// <summary>SQLite mapping for local stock ledger rows.</summary>
public sealed class OfflineStockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).HasConversion<string>();
        builder.Property(m => m.AdjustmentDirection).HasConversion<string>();
        builder.Property(m => m.Quantity).HasPrecision(18, 4);
        builder.Property(m => m.Reason).HasMaxLength(200);
        builder.HasIndex(m => m.ProductId);
    }
}
