using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;
using Microsoft.Extensions.Hosting;
using Environments = Microsoft.Extensions.Hosting.Environments;

namespace Core.Application.Auth.Commands;

/// <summary>
/// Registers a user in Development only (defense in depth with API route mapping).
/// </summary>
public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IIdentityService _identityService;
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly IAccessProfileSeeder _accessProfileSeeder;
    private readonly IHostEnvironment _hostEnvironment;

    public RegisterUserCommandHandler(
        IIdentityService identityService,
        IAccessProfileRepository accessProfileRepository,
        IAccessProfileSeeder accessProfileSeeder,
        IHostEnvironment hostEnvironment)
    {
        _identityService = identityService;
        _accessProfileRepository = accessProfileRepository;
        _accessProfileSeeder = accessProfileSeeder;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(_hostEnvironment.EnvironmentName, Environments.Development, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<Guid>(ErrorCodes.Auth.RegistrationNotAllowed);
        }

        if (!ApplicationRoles.All.Contains(request.Role))
        {
            return Result.Failure<Guid>(ErrorCodes.Auth.InvalidRole);
        }

        var tenantId = request.TenantId ?? Guid.NewGuid();
        await _accessProfileSeeder.EnsureTenantProfilesAsync(tenantId, cancellationToken);

        var profile = await _accessProfileRepository.GetSystemProfileByBaseRoleAsync(request.Role, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<Guid>(ErrorCodes.UserAccount.ProfileNotFound);
        }

        var createResult = await _identityService.CreateUserAsync(
            request.Email,
            request.Password,
            request.Role,
            tenantId,
            profile.Id,
            cancellationToken: cancellationToken);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        return Result.Success(Guid.Parse(createResult.Value));
    }
}
