using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Authorization;
using Core.Domain.Entities;

namespace Core.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotently seeds system access profiles per tenant.
/// </summary>
public sealed class AccessProfileSeeder : IAccessProfileSeeder
{
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly CoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public AccessProfileSeeder(
        IAccessProfileRepository accessProfileRepository,
        CoreDbContext dbContext,
        ITenantContext tenantContext)
    {
        _accessProfileRepository = accessProfileRepository;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task EnsureTenantProfilesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        _tenantContext.TenantId = tenantId;

        await EnsureOneAsync(ApplicationRoles.Admin, Permissions.AdminDefaults(), 100m, cancellationToken);
        await EnsureOneAsync(ApplicationRoles.Veterinarian, Permissions.VeterinarianDefaults(), 0m, cancellationToken);
        await EnsureOneAsync(ApplicationRoles.Receptionist, Permissions.ReceptionistDefaults(), 0m, cancellationToken);
        await EnsureOneAsync(ApplicationRoles.Cashier, Permissions.CashierDefaults(), 0m, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureOneAsync(string baseRole, IReadOnlyList<string> defaults, decimal maxDiscountPercent, CancellationToken cancellationToken)
    {
        var existing = await _accessProfileRepository.GetSystemProfileByBaseRoleAsync(baseRole, cancellationToken);
        if (existing is not null)
        {
            var changed = false;
            if (baseRole == ApplicationRoles.Admin && !existing.PermissionCodes.Contains(Permissions.AuditRead))
            {
                existing.Grant(Permissions.AuditRead);
                changed = true;
            }

            if (baseRole == ApplicationRoles.Admin && existing.MaxDiscountPercent < 100m)
            {
                existing.SetMaxDiscountPercent(100m);
                changed = true;
            }

            foreach (var permission in GetMissingPermissions(baseRole, existing.PermissionCodes))
            {
                existing.Grant(permission);
                changed = true;
            }

            if (changed)
            {
                _accessProfileRepository.Update(existing);
            }

            return;
        }

        var created = AccessProfile.CreateSystem(baseRole, baseRole, defaults, maxDiscountPercent);
        if (created.IsSuccess)
        {
            _accessProfileRepository.Add(created.Value);
        }
    }

    private static IEnumerable<string> GetMissingPermissions(string baseRole, IReadOnlyCollection<string> granted)
    {
        var required = baseRole switch
        {
            ApplicationRoles.Veterinarian => Permissions.VeterinarianDefaults(),
            ApplicationRoles.Receptionist => Permissions.ReceptionistDefaults(),
            ApplicationRoles.Cashier => Permissions.CashierDefaults(),
            ApplicationRoles.Admin => Permissions.AdminDefaults(),
            _ => Array.Empty<string>()
        };

        return required.Where(p => !granted.Contains(p));
    }
}
