using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;

namespace Clients.Infrastructure;

public class OfflineDbContext : DbContext
{
    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<Pet> Pets => Set<Pet>();

    public OfflineDbContext(DbContextOptions<OfflineDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map ValueObjects properly
        modelBuilder.Entity<Tutor>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.OwnsOne(t => t.Cpf, cpf =>
            {
                cpf.Property(c => c.Number).HasColumnName("Cpf");
            });
            builder.OwnsOne(t => t.Email, email =>
            {
                email.Property(e => e.Address).HasColumnName("Email");
            });
            builder.OwnsOne(t => t.Phone, phone =>
            {
                phone.Property(p => p.Number).HasColumnName("Phone");
            });
            builder.Metadata.FindNavigation(nameof(Tutor.Pets))?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Pet>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.Property(p => p.Species).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Breed).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Sex).IsRequired().HasConversion<string>();
            builder.HasOne<Tutor>().WithMany(t => t.Pets).HasForeignKey(p => p.TutorId);
        });
    }
}
