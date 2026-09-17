using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clients.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF mapping for tutors in the client-local SQLite database (subset of Core CRM schema).
/// </summary>
public sealed class OfflineTutorConfiguration : IEntityTypeConfiguration<Tutor>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        builder.ToTable("Tutors");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.RowVersion).IsConcurrencyToken();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(t => t.Name);

        builder.Property(t => t.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(t => t.DeletedAt);

        builder.OwnsOne(t => t.Email, e =>
        {
            e.Property(x => x.Address).HasColumnName("Email").IsRequired().HasMaxLength(250);
            e.HasIndex(x => x.Address).IsUnique();
        });

        builder.OwnsOne(t => t.Cpf, c =>
        {
            c.Property(x => x.Number).HasColumnName("Cpf").IsRequired().HasMaxLength(11);
            c.HasIndex(x => x.Number).IsUnique();
        });

        builder.OwnsOne(t => t.Phone, p =>
        {
            p.Property(x => x.Number).HasColumnName("Phone").IsRequired().HasMaxLength(20);
        });

        builder.HasMany(t => t.Pets)
            .WithOne()
            .HasForeignKey(p => p.TutorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata.FindNavigation(nameof(Tutor.Pets))?.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
