using Fiscal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiscal.Infrastructure.Persistence.Configurations;

internal sealed class FiscalDocumentItemConfiguration : IEntityTypeConfiguration<FiscalDocumentItem>
{
    public void Configure(EntityTypeBuilder<FiscalDocumentItem> builder)
    {
        builder.ToTable("FiscalDocumentItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.Ncm).HasMaxLength(8);
        builder.Property(x => x.Cfop).HasMaxLength(4);
        builder.Property(x => x.Csosn).HasMaxLength(3);
        builder.Ignore(x => x.Total);
    }
}
