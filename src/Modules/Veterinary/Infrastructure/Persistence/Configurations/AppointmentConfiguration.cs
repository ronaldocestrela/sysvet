using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TutorId).IsRequired();
        builder.Property(a => a.PetId).IsRequired();
        builder.Property(a => a.VeterinarianId).IsRequired();
        builder.Property(a => a.Date).IsRequired();
        builder.Property(a => a.DurationInMinutes).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
        builder.Property(a => a.Notes).HasMaxLength(500);

        builder.Property(a => a.RowVersion)
            .IsConcurrencyToken();
    }
}
