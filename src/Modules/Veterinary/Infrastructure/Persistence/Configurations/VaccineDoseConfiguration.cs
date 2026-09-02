using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence.Configurations;

internal sealed class VaccineDoseConfiguration : IEntityTypeConfiguration<VaccineDose>
{
    public void Configure(EntityTypeBuilder<VaccineDose> builder)
    {
        builder.ToTable("VaccineDoses");

        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.PetId).IsRequired();

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.BatchNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.AppliedAt).IsRequired();
        builder.Property(v => v.NextDueDate);
    }
}
