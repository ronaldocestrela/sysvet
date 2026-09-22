using Core.Domain;

namespace TutorPortal.Domain.Entities;

/// <summary>
/// Stores a browser Web Push subscription for a tutor portal Identity user.
/// </summary>
public sealed class TutorPushSubscription : Entity
{
    /// <summary>Identity user identifier.</summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>Push service endpoint URL.</summary>
    public string Endpoint { get; private set; } = string.Empty;

    /// <summary>Client public key (base64).</summary>
    public string P256dh { get; private set; } = string.Empty;

    /// <summary>Auth secret (base64).</summary>
    public string Auth { get; private set; } = string.Empty;

    /// <summary>Optional browser user agent captured at subscribe time.</summary>
    public string UserAgent { get; private set; } = string.Empty;

#pragma warning disable CS8618
    private TutorPushSubscription() : base(Guid.Empty) { }
#pragma warning restore CS8618

    private TutorPushSubscription(Guid id, string userId, string endpoint, string p256dh, string auth, string userAgent)
        : base(id)
    {
        UserId = userId;
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
        UserAgent = userAgent;
    }

    /// <summary>
    /// Creates a new push subscription row for the tutor user.
    /// </summary>
    public static Result<TutorPushSubscription> Create(
        string userId,
        string endpoint,
        string p256dh,
        string auth,
        string? userAgent = null,
        Guid id = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure<TutorPushSubscription>(ErrorCodes.Push.InvalidUser);
        }

        if (string.IsNullOrWhiteSpace(endpoint) || endpoint.Length > 2000)
        {
            return Result.Failure<TutorPushSubscription>(ErrorCodes.Push.InvalidEndpoint);
        }

        if (string.IsNullOrWhiteSpace(p256dh) || string.IsNullOrWhiteSpace(auth))
        {
            return Result.Failure<TutorPushSubscription>(ErrorCodes.Push.InvalidKeys);
        }

        return Result.Success(new TutorPushSubscription(
            id == Guid.Empty ? Guid.NewGuid() : id,
            userId.Trim(),
            endpoint.Trim(),
            p256dh.Trim(),
            auth.Trim(),
            userAgent?.Trim() ?? string.Empty));
    }

    /// <summary>
    /// Updates keys when the browser refreshes the subscription for the same endpoint.
    /// </summary>
    public Result RefreshKeys(string p256dh, string auth, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(p256dh) || string.IsNullOrWhiteSpace(auth))
        {
            return Result.Failure(ErrorCodes.Push.InvalidKeys);
        }

        P256dh = p256dh.Trim();
        Auth = auth.Trim();
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            UserAgent = userAgent.Trim();
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
