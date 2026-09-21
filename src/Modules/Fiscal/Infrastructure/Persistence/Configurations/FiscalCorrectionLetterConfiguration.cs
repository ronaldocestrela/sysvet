using Fiscal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiscal.Infrastructure.Persistence.Configurations;

internal sealed class FiscalCorrectionLetterConfiguration : IEntityTypeConfiguration<FiscalCorrectionLetter>
{
    public void Configure(EntityTypeBuilder<FiscalCorrectionLetter> builder)
    {
        builder.ToTable("FiscalCorrectionLetters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CorrectionText).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Protocol).HasMaxLength(60);
    }
}
