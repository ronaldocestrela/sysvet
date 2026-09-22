using Core.Application.Auth.Dtos;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.ValueObjects;
using MediatR;
using TutorPortal.Application.Abstractions;
using TutorPortal.Domain.Entities;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Application.Auth.Commands;

/// <summary>
/// Validates CRM tutor data and creates a tutor portal Identity account.
/// </summary>
public sealed class RegisterTutorCommandHandler : IRequestHandler<RegisterTutorCommand, Result<AuthTokensDto>>
{
    private readonly ITenantContext _tenantContext;
    private readonly ICrmTutorLookup _crmTutorLookup;
    private readonly IIdentityService _identityService;
    private readonly ITutorPortalAccountRepository _accountRepository;
    private readonly ITutorPortalUnitOfWork _unitOfWork;
    private readonly TutorPortalAuthService _authService;

    public RegisterTutorCommandHandler(
        ITenantContext tenantContext,
        ICrmTutorLookup crmTutorLookup,
        IIdentityService identityService,
        ITutorPortalAccountRepository accountRepository,
        ITutorPortalUnitOfWork unitOfWork,
        TutorPortalAuthService authService)
    {
        _tenantContext = tenantContext;
        _crmTutorLookup = crmTutorLookup;
        _identityService = identityService;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    /// <inheritdoc />
    public async Task<Result<AuthTokensDto>> Handle(RegisterTutorCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<AuthTokensDto>(ErrorCodes.Authorization.Forbidden);
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.RegistrationDenied);
        }

        var cpfResult = Cpf.Create(request.Cpf);
        if (cpfResult.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.RegistrationDenied);
        }

        var match = await _crmTutorLookup.FindActiveTutorByEmailAndCpfAsync(
            emailResult.Value.Address,
            cpfResult.Value.Number,
            cancellationToken);
        if (match is null)
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.RegistrationDenied);
        }

        var existingByTutor = await _accountRepository.GetByTutorIdAsync(match.TutorId, cancellationToken);
        if (existingByTutor is not null)
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.RegistrationDenied);
        }

        var userIdResult = await _identityService.CreateTutorUserAsync(
            emailResult.Value.Address,
            request.Password,
            tenantId,
            cancellationToken);
        if (userIdResult.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(TutorPortal.Domain.ErrorCodes.Account.RegistrationDenied);
        }

        var accountResult = TutorPortalAccount.Create(userIdResult.Value, match.TutorId);
        if (accountResult.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(accountResult.Error);
        }

        _accountRepository.Add(accountResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var credentials = await _identityService.ValidateCredentialsAsync(emailResult.Value.Address, request.Password, cancellationToken);
        if (credentials.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(credentials.Error);
        }

        return await _authService.IssueTokensAsync(credentials.Value, cancellationToken);
    }
}
