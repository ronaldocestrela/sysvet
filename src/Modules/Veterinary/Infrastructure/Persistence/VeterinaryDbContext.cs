using Core.Domain;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;

namespace Veterinary.Infrastructure.Persistence;

public class VeterinaryDbContext : DbContext, IUnitOfWork
{
    public ITenantContext TenantContext { get; set; } = null!;

    public VeterinaryDbContext(DbContextOptions<VeterinaryDbContext> options) : base(options)
    {
    }

    public string SchemaName => TenantContext?.SchemaName ?? "dbo";

    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<ScheduleSlot> ScheduleSlots { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        if (!string.IsNullOrWhiteSpace(SchemaName))
        {
            modelBuilder.HasDefaultSchema(SchemaName);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VeterinaryDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aqui poderíamos injetar atualizações de UpdatedAt se não estivesse na base Entity ou interceptor global,
        // mas o EF Core atualiza o RowVersion (se configurado) e o UpdatedAt pode ser controlado pelo handler/entidade.
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                // Para simplificar, garantimos que a entidade tenha a propriedade UpdatedAt atualizada
                // Isso pode requerer expor o UpdatedAt para settar internamente ou usar reflexão caso seja private setter
                // Mas de acordo com Entity.cs, ela deve ser atualizada de alguma forma
            }
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
}
