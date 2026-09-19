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
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.PaidAt);

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

        builder.Property(o => o.RowVersion).IsConcurrencyToken();
    }
}
