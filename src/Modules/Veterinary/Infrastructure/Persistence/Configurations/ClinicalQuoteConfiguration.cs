using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class ClinicalQuoteConfiguration : IEntityTypeConfiguration<ClinicalQuote>
{
    public void Configure(EntityTypeBuilder<ClinicalQuote> builder)
    {
        builder.ToTable("ClinicalQuotes");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Status).IsRequired().HasConversion<int>();
        builder.Property(q => q.ConversionStatus).IsRequired().HasConversion<int>();
        builder.Property(q => q.Notes).HasMaxLength(2000);
        builder.HasIndex(q => q.AppointmentId);
        builder.HasIndex(q => q.PetId);
        builder.HasIndex(q => q.ConversionStatus);
        builder.Property(q => q.RowVersion).IsConcurrencyToken();
        builder.HasMany(q => q.Items)
            .WithOne()
            .HasForeignKey(i => i.ClinicalQuoteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(ClinicalQuote.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ClinicalQuoteItemConfiguration : IEntityTypeConfiguration<ClinicalQuoteItem>
{
    public void Configure(EntityTypeBuilder<ClinicalQuoteItem> builder)
    {
        builder.ToTable("ClinicalQuoteItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Description).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Quantity).HasPrecision(18, 4);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.Kind).IsRequired().HasConversion<int>();
        builder.HasIndex(i => i.ClinicalQuoteId);
    }
}
