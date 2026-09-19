using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public class SaleReturnConfiguration : IEntityTypeConfiguration<SaleReturn>
{
    public void Configure(EntityTypeBuilder<SaleReturn> builder)
    {
        builder.ToTable("SaleReturns");
        builder.HasKey(r => r.Id);
        builder.OwnsOne(r => r.RefundAmount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("RefundAmount").HasPrecision(18, 2);
            m.Property(x => x.Currency).HasColumnName("RefundCurrency").HasMaxLength(3);
        });
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.HasMany(r => r.Lines)
            .WithOne()
            .HasForeignKey(l => l.SaleReturnId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Lines).HasField("_lines");
    }
}
