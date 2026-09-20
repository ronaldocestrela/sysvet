using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public sealed class PrepaidCreditConfiguration : IEntityTypeConfiguration<PrepaidCredit>
{
    public void Configure(EntityTypeBuilder<PrepaidCredit> builder)
    {
        builder.ToTable("PrepaidCredits");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.OrderItemId).IsUnique();
        builder.Ignore(c => c.RowVersion);
        builder.Ignore(c => c.UpdatedAt);
    }
}
