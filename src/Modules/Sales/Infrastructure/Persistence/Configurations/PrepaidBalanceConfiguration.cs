using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sales.Domain.Entities;

namespace Sales.Infrastructure.Persistence.Configurations;

public sealed class PrepaidBalanceConfiguration : IEntityTypeConfiguration<PrepaidBalance>
{
    public void Configure(EntityTypeBuilder<PrepaidBalance> builder)
    {
        builder.ToTable("PrepaidBalances");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.ServiceCode).HasConversion<string>().HasMaxLength(20);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.HasIndex("TenantId", nameof(PrepaidBalance.PetId), nameof(PrepaidBalance.ServiceCode)).IsUnique();
        builder.Ignore(b => b.RowVersion);
        builder.HasMany(b => b.Credits)
            .WithOne()
            .HasForeignKey(c => c.PrepaidBalanceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(b => b.Credits).HasField("_credits");
    }
}
