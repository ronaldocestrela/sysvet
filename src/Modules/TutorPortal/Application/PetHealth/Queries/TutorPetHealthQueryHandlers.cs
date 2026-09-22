using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Auth;
using TutorPortal.Application.PetHealth.Dtos;

namespace TutorPortal.Application.PetHealth.Queries;

/// <summary>Loads vaccination card after tutor-pet ownership check.</summary>
public sealed class GetTutorVaccinationCardQueryHandler : IRequestHandler<GetTutorVaccinationCardQuery, Result<TutorVaccinationCardDto>>
{
    private readonly TutorPortalUserResolver _userResolver;
    private readonly TutorPetAccessGuard _accessGuard;
    private readonly ITutorPetHealthReadPort _readPort;

    /// <summary>Creates the handler with resolver, guard, and read port.</summary>
    public GetTutorVaccinationCardQueryHandler(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        ITutorPetHealthReadPort readPort)
    {
        _userResolver = userResolver;
        _accessGuard = accessGuard;
        _readPort = readPort;
    }

    /// <inheritdoc />
    public async Task<Result<TutorVaccinationCardDto>> Handle(GetTutorVaccinationCardQuery request, CancellationToken cancellationToken)
    {
        var tutorResult = await _userResolver.ResolveTutorIdAsync(cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<TutorVaccinationCardDto>(tutorResult.Error);
        }

        var access = await _accessGuard.EnsureOwnedAsync(request.PetId, tutorResult.Value, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<TutorVaccinationCardDto>(access.Error);
        }

        var card = await _readPort.GetVaccinationCardAsync(request.PetId, cancellationToken);
        if (card is null)
        {
            return Result.Failure<TutorVaccinationCardDto>(TutorPortal.Domain.ErrorCodes.Pet.NotFound);
        }

        return Result.Success(card);
    }
}

/// <summary>Lists exams after tutor-pet ownership check.</summary>
public sealed class ListTutorPetExamsQueryHandler : IRequestHandler<ListTutorPetExamsQuery, Result<IReadOnlyList<TutorPetExamDto>>>
{
    private readonly TutorPortalUserResolver _userResolver;
    private readonly TutorPetAccessGuard _accessGuard;
    private readonly ITutorPetHealthReadPort _readPort;

    /// <summary>Creates the handler with resolver, guard, and read port.</summary>
    public ListTutorPetExamsQueryHandler(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        ITutorPetHealthReadPort readPort)
    {
        _userResolver = userResolver;
        _accessGuard = accessGuard;
        _readPort = readPort;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<TutorPetExamDto>>> Handle(ListTutorPetExamsQuery request, CancellationToken cancellationToken)
    {
        var tutorResult = await _userResolver.ResolveTutorIdAsync(cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<TutorPetExamDto>>(tutorResult.Error);
        }

        var access = await _accessGuard.EnsureOwnedAsync(request.PetId, tutorResult.Value, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<IReadOnlyList<TutorPetExamDto>>(access.Error);
        }

        var exams = await _readPort.ListExamsAsync(request.PetId, cancellationToken);
        return Result.Success(exams);
    }
}

/// <summary>Lists timeline after tutor-pet ownership check.</summary>
public sealed class ListTutorPetTimelineQueryHandler : IRequestHandler<ListTutorPetTimelineQuery, Result<IReadOnlyList<TutorPetTimelineItemDto>>>
{
    private readonly TutorPortalUserResolver _userResolver;
    private readonly TutorPetAccessGuard _accessGuard;
    private readonly ITutorPetHealthReadPort _readPort;

    /// <summary>Creates the handler with resolver, guard, and read port.</summary>
    public ListTutorPetTimelineQueryHandler(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        ITutorPetHealthReadPort readPort)
    {
        _userResolver = userResolver;
        _accessGuard = accessGuard;
        _readPort = readPort;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<TutorPetTimelineItemDto>>> Handle(ListTutorPetTimelineQuery request, CancellationToken cancellationToken)
    {
        var tutorResult = await _userResolver.ResolveTutorIdAsync(cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<TutorPetTimelineItemDto>>(tutorResult.Error);
        }

        var access = await _accessGuard.EnsureOwnedAsync(request.PetId, tutorResult.Value, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<IReadOnlyList<TutorPetTimelineItemDto>>(access.Error);
        }

        var timeline = await _readPort.ListTimelineAsync(request.PetId, cancellationToken);
        return Result.Success(timeline);
    }
}
