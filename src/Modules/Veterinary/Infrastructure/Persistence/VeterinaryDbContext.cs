using Core.Domain;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence;

public class VeterinaryDbContext : DbContext, IVeterinaryUnitOfWork
{
    public ITenantContext TenantContext { get; set; } = null!;

    public VeterinaryDbContext(DbContextOptions<VeterinaryDbContext> options) : base(options)
    {
    }

    public string SchemaName => TenantContext?.SchemaName ?? "dbo";

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<EvolutionNote> EvolutionNotes => Set<EvolutionNote>();
    public DbSet<VaccineDose> VaccineDoses => Set<VaccineDose>();
    public DbSet<Hospitalization> Hospitalizations => Set<Hospitalization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (!string.IsNullOrWhiteSpace(SchemaName))
        {
            modelBuilder.HasDefaultSchema(SchemaName);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VeterinaryDbContext).Assembly);
    }

    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
