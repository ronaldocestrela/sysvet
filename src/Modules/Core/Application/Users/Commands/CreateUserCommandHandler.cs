using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace Core.Application.Users.Commands;

/// <summary>
/// Validates profile and persists a new staff user via Identity.
/// </summary>
public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<string>>
{
    private readonly IIdentityService _identityService;
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IAccessProfileSeeder _accessProfileSeeder;
    private readonly ITenantContext _tenantContext;

    public CreateUserCommandHandler(
        IIdentityService identityService,
        IAccessProfileRepository accessProfileRepository,
        IAccessProfileSeeder accessProfileSeeder,
        ITenantContext tenantContext)
    {
        _identityService = identityService;
        _accessProfileRepository = accessProfileRepository;
        _accessProfileSeeder = accessProfileSeeder;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<Result<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<string>(ErrorCodes.Authorization.Forbidden);
        }

        await _accessProfileSeeder.EnsureTenantProfilesAsync(tenantId, cancellationToken);

        var profile = await _accessProfileRepository.GetByIdAsync(request.AccessProfileId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<string>(ErrorCodes.UserAccount.ProfileNotFound);
        }

        return await _identityService.CreateUserAsync(
            request.Email,
            request.Password,
            profile.BaseRole,
            tenantId,
            profile.Id,
            request.DisplayName,
            cancellationToken);
    }
}
