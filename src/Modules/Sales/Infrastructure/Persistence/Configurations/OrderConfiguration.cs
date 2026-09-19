using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property<Guid>("TenantId").IsRequired();

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.FinanceIntegrationStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.TutorId);
        builder.Property(o => o.PetId);
        builder.Property(o => o.SourceQuoteId);
        builder.Property(o => o.SellerUserId).IsRequired();
        builder.Property(o => o.DiscountPercent).HasPrecision(5, 2);
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.PaidAt);

        builder.Ignore(o => o.SubtotalAmount);
        builder.Ignore(o => o.DiscountAmount);
        builder.Ignore(o => o.TotalAmount);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Payments)
            .WithOne()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Payments).HasField("_payments");

        builder.HasMany(o => o.Commissions)
            .WithOne()
            .HasForeignKey(c => c.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Commissions).HasField("_commissions");

        builder.HasMany(o => o.Returns)
            .WithOne()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Returns).HasField("_returns");

        builder.Property(o => o.RowVersion).IsConcurrencyToken();
    }
}
