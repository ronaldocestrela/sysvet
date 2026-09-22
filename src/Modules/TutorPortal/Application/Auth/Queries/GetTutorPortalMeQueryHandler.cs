using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Auth.Dtos;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Application.Auth.Queries;

/// <summary>
/// Builds the tutor portal profile from JWT claims and CRM data.
/// </summary>
public sealed class GetTutorPortalMeQueryHandler : IRequestHandler<GetTutorPortalMeQuery, Result<TutorPortalMeDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly ITutorPortalAccountRepository _accountRepository;
    private readonly ICrmTutorLookup _crmTutorLookup;

    public GetTutorPortalMeQueryHandler(
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        ITutorPortalAccountRepository accountRepository,
        ICrmTutorLookup crmTutorLookup)
    {
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _accountRepository = accountRepository;
        _crmTutorLookup = crmTutorLookup;
    }

    /// <inheritdoc />
    public async Task<Result<TutorPortalMeDto>> Handle(GetTutorPortalMeQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
        {
            return Result.Failure<TutorPortalMeDto>(ErrorCodes.Authorization.Unauthorized);
        }

        var tutorId = _currentUser.TutorId;
        if (tutorId is null || tutorId == Guid.Empty)
        {
            var account = await _accountRepository.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
            tutorId = account?.TutorId;
        }

        if (tutorId is null || tutorId == Guid.Empty)
        {
            return Result.Failure<TutorPortalMeDto>(TutorPortal.Domain.ErrorCodes.Account.NotFound);
        }

        var tenantId = _currentUser.TenantId != Guid.Empty ? _currentUser.TenantId : _tenantContext.TenantId;
        var tutor = await _crmTutorLookup.GetActiveTutorByIdAsync(tutorId.Value, cancellationToken);
        if (tutor is null)
        {
            return Result.Failure<TutorPortalMeDto>(TutorPortal.Domain.ErrorCodes.Account.NotFound);
        }

        var pets = await _crmTutorLookup.GetPetsForTutorAsync(tutorId.Value, cancellationToken);

        return Result.Success(new TutorPortalMeDto(
            _currentUser.UserId,
            _currentUser.Email ?? string.Empty,
            tenantId,
            tutorId.Value,
            tutor.Name,
            pets.Select(p => new TutorPortalPetDto(p.Id, p.Name)).ToList()));
    }
}
