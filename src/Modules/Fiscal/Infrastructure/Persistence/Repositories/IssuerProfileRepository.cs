using Fiscal.Domain.Entities;
using Fiscal.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fiscal.Infrastructure.Persistence.Repositories;

/// <summary>EF issuer profile repository.</summary>
public sealed class IssuerProfileRepository : IIssuerProfileRepository
{
    private readonly FiscalDbContext _dbContext;

    public IssuerProfileRepository(FiscalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IssuerProfile?> GetAsync(CancellationToken cancellationToken = default) =>
        _dbContext.IssuerProfiles.FirstOrDefaultAsync(cancellationToken);

    public void Add(IssuerProfile profile) => _dbContext.IssuerProfiles.Add(profile);

    public void Update(IssuerProfile profile) => _dbContext.IssuerProfiles.Update(profile);
}
