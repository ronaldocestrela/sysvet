using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using TutorPortal.Application.Abstractions;

namespace TutorPortal.Application.Push.Queries;

/// <summary>Exposes configured VAPID public key to authenticated tutors.</summary>
public sealed class GetTutorPushVapidPublicKeyQueryHandler : IRequestHandler<GetTutorPushVapidPublicKeyQuery, Result<string?>>
{
    private readonly ITutorPushSettings _settings;

    /// <summary>Creates the handler with push settings.</summary>
    public GetTutorPushVapidPublicKeyQueryHandler(ITutorPushSettings settings) => _settings = settings;

    /// <inheritdoc />
    public Task<Result<string?>> Handle(GetTutorPushVapidPublicKeyQuery request, CancellationToken cancellationToken)
    {
        var key = string.IsNullOrWhiteSpace(_settings.VapidPublicKey) ? null : _settings.VapidPublicKey.Trim();
        return Task.FromResult(Result.Success<string?>(key));
    }
}
