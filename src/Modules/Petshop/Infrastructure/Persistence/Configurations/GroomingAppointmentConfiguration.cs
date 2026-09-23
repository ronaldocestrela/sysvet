using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Petshop.Domain.Entities;

namespace Petshop.Infrastructure.Persistence.Configurations;

internal sealed class GroomingAppointmentConfiguration : IEntityTypeConfiguration<GroomingAppointment>
{
    public void Configure(EntityTypeBuilder<GroomingAppointment> builder)
    {
        builder.ToTable("GroomingAppointments");
        builder.HasKey(a => a.Id);
        builder.Property<Guid>("TenantId").IsRequired();
        builder.Property(a => a.TutorId).IsRequired();
        builder.Property(a => a.PetId).IsRequired();
        builder.Property(a => a.GroomerId).IsRequired();
        builder.Property(a => a.GroomingServiceId).IsRequired();
        builder.Property(a => a.Date).IsRequired();
        builder.Property(a => a.DurationInMinutes).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(500);
        builder.Property(a => a.RowVersion).IsConcurrencyToken();

        builder.HasIndex("TenantId", nameof(GroomingAppointment.Status), nameof(GroomingAppointment.Date));
    }
}
