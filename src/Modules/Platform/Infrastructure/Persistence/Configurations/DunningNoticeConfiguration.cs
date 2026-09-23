using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence.Configurations;

internal sealed class DunningNoticeConfiguration : IEntityTypeConfiguration<DunningNotice>
{
    public void Configure(EntityTypeBuilder<DunningNotice> builder)
    {
        builder.ToTable("PlatformDunningNotices");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Channel).HasConversion<int>();
        builder.HasIndex(n => new { n.TenantId, n.InvoiceId, n.StepDay, n.Channel }).IsUnique();
        builder.Property(n => n.RowVersion).IsConcurrencyToken();
    }
}
