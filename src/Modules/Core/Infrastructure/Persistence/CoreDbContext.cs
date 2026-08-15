using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Core.Infrastructure.Persistence;

public class CoreDbContext : DbContext, IUnitOfWork
{
    public ITenantContext TenantContext { get; set; } = null!;

    public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
    {
    }

    public string SchemaName => TenantContext?.SchemaName ?? "dbo";

    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<Pet> Pets => Set<Pet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        if (!string.IsNullOrWhiteSpace(SchemaName))
        {
            modelBuilder.HasDefaultSchema(SchemaName);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
    }



    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
