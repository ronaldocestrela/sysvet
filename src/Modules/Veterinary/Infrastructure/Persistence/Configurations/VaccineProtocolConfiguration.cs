using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class VaccineProtocolConfiguration : IEntityTypeConfiguration<VaccineProtocol>
{
    public void Configure(EntityTypeBuilder<VaccineProtocol> builder)
    {
        builder.ToTable("VaccineProtocols");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Species).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.IsActive).IsRequired();
        builder.HasMany(p => p.Doses)
            .WithOne()
            .HasForeignKey(d => d.VaccineProtocolId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata.FindNavigation(nameof(VaccineProtocol.Doses))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class VaccineProtocolDoseConfiguration : IEntityTypeConfiguration<VaccineProtocolDose>
{
    public void Configure(EntityTypeBuilder<VaccineProtocolDose> builder)
    {
        builder.ToTable("VaccineProtocolDoses");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Label).IsRequired().HasMaxLength(100);
        builder.HasIndex(d => d.VaccineProtocolId);
    }
}
