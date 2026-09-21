using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petshop.Domain.Entities;

namespace Petshop.Infrastructure.Persistence.Configurations;

internal sealed class GroomingSlotConfiguration : IEntityTypeConfiguration<GroomingSlot>
{
    public void Configure(EntityTypeBuilder<GroomingSlot> builder)
    {
        builder.ToTable("GroomingSlots");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.GroomerId).IsRequired();
        builder.Property(s => s.Date).IsRequired();
        builder.Property(s => s.StartTime).IsRequired();
        builder.Property(s => s.EndTime).IsRequired();
        builder.Property(s => s.IsAvailable).IsRequired();
    }
}
