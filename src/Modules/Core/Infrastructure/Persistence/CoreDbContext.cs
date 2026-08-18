using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.Auditing;
using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Core.Infrastructure.Persistence;

public class CoreDbContext : IdentityDbContext<AppUser>, IUnitOfWork
{
    public ITenantContext TenantContext { get; set; } = null!;

    public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
    {
    }

    public string SchemaName => TenantContext?.SchemaName ?? "dbo";

    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<Pet> Pets { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

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
