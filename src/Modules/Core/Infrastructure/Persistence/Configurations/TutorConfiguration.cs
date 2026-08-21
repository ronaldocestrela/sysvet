using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Persistence.Configurations;

public class TutorConfiguration : IEntityTypeConfiguration<Tutor>
{
    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        builder.ToTable("Tutors");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.RowVersion)
            .IsConcurrencyToken();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.OwnsOne(t => t.Email, e =>
        {
            e.Property(x => x.Address)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(250);
            e.HasIndex(x => x.Address).IsUnique();
        });

        builder.OwnsOne(t => t.Cpf, c =>
        {
            c.Property(x => x.Number)
                .HasColumnName("Cpf")
                .IsRequired()
                .HasMaxLength(11);
            c.HasIndex(x => x.Number).IsUnique();
        });

        builder.OwnsOne(t => t.Phone, p =>
        {
            p.Property(x => x.Number)
                .HasColumnName("Phone")
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.HasMany(t => t.Pets)
            .WithOne()
            .HasForeignKey(p => p.TutorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
