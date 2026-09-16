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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AccessProfileSeeder(
        IAccessProfileRepository accessProfileRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _accessProfileRepository = accessProfileRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task EnsureTenantProfilesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        _tenantContext.TenantId = tenantId;

        await EnsureOneAsync(ApplicationRoles.Admin, Permissions.AdminDefaults(), cancellationToken);
        await EnsureOneAsync(ApplicationRoles.Veterinarian, Permissions.VeterinarianDefaults(), cancellationToken);
        await EnsureOneAsync(ApplicationRoles.Receptionist, Permissions.ReceptionistDefaults(), cancellationToken);
        await EnsureOneAsync(ApplicationRoles.Cashier, Permissions.CashierDefaults(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureOneAsync(string baseRole, IReadOnlyList<string> defaults, CancellationToken cancellationToken)
    {
        var existing = await _accessProfileRepository.GetSystemProfileByBaseRoleAsync(baseRole, cancellationToken);
        if (existing is not null)
        {
            if (baseRole == ApplicationRoles.Admin && !existing.PermissionCodes.Contains(Permissions.AuditRead))
            {
                existing.Grant(Permissions.AuditRead);
                _accessProfileRepository.Update(existing);
            }

            return;
        }

        var created = AccessProfile.CreateSystem(baseRole, baseRole, defaults);
        if (created.IsSuccess)
        {
            _accessProfileRepository.Add(created.Value);
        }
    }
}
