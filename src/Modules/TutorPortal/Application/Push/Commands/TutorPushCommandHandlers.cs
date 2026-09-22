using Core.Application.Common.Interfaces;
using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using TutorPortal.Domain.Entities;
using TutorPortal.Domain.Repositories;

namespace TutorPortal.Application.Push.Commands;

/// <summary>Persists or updates a tutor Web Push subscription.</summary>
public sealed class SubscribeTutorPushCommandHandler : IRequestHandler<SubscribeTutorPushCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly ITutorPushSubscriptionRepository _repository;
    private readonly ITutorPortalUnitOfWork _unitOfWork;

    /// <summary>Creates the handler with user context and persistence.</summary>
    public SubscribeTutorPushCommandHandler(
        ICurrentUser currentUser,
        ITutorPushSubscriptionRepository repository,
        ITutorPortalUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(SubscribeTutorPushCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
        {
            return Result.Failure(ErrorCodes.Authorization.Unauthorized);
        }

        var existing = await _repository.GetByUserAndEndpointAsync(_currentUser.UserId, request.Endpoint, cancellationToken);
        if (existing is not null)
        {
            var refresh = existing.RefreshKeys(request.P256dh, request.Auth, request.UserAgent);
            if (refresh.IsFailure)
            {
                return refresh;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        var created = TutorPushSubscription.Create(
            _currentUser.UserId,
            request.Endpoint,
            request.P256dh,
            request.Auth,
            request.UserAgent);
        if (created.IsFailure)
        {
            return Result.Failure(created.Error);
        }

        await _repository.AddAsync(created.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

/// <summary>Deletes a tutor Web Push subscription by endpoint.</summary>
public sealed class UnsubscribeTutorPushCommandHandler : IRequestHandler<UnsubscribeTutorPushCommand, Result>
{
    private readonly ICurrentUser _currentUser;
    private readonly ITutorPushSubscriptionRepository _repository;
    private readonly ITutorPortalUnitOfWork _unitOfWork;

    /// <summary>Creates the handler with user context and persistence.</summary>
    public UnsubscribeTutorPushCommandHandler(
        ICurrentUser currentUser,
        ITutorPushSubscriptionRepository repository,
        ITutorPortalUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(UnsubscribeTutorPushCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
        {
            return Result.Failure(ErrorCodes.Authorization.Unauthorized);
        }

        var existing = await _repository.GetByUserAndEndpointAsync(_currentUser.UserId, request.Endpoint, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(TutorPortal.Domain.ErrorCodes.Push.NotFound);
        }

        _repository.Remove(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
