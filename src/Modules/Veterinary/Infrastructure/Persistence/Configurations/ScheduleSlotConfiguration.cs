using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

public class ScheduleSlotConfiguration : IEntityTypeConfiguration<ScheduleSlot>
{
    public void Configure(EntityTypeBuilder<ScheduleSlot> builder)
    {
        builder.ToTable("ScheduleSlots");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.VeterinarianId).IsRequired();
        builder.Property(s => s.Date).IsRequired();
        builder.Property(s => s.StartTime).IsRequired();
        builder.Property(s => s.EndTime).IsRequired();
        builder.Property(s => s.IsAvailable).IsRequired();

        // Evitar dois slots com o mesmo horário para o mesmo veterinário
        builder.HasIndex(s => new { s.VeterinarianId, s.Date, s.StartTime })
            .IsUnique();

        builder.Property(s => s.RowVersion)
            .IsConcurrencyToken();
    }
}
