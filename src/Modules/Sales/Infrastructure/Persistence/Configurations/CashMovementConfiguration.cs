using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

internal sealed class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.ToTable("CashMovements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Reason).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.OwnsOne(m => m.Amount, money =>
        {
            money.Property(x => x.Amount).HasColumnName("Amount").HasPrecision(18, 2);
            money.Property(x => x.Currency).HasColumnName("AmountCurrency").HasMaxLength(3);
        });

        builder.HasIndex(m => m.CashRegisterId);
    }
}
