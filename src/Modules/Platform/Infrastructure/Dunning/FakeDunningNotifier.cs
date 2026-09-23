using Core.Domain;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Dunning;

/// <summary>Records dunning delivery without external providers (9.5 CI/dev).</summary>
public sealed class FakeDunningNotifier : IDunningNotifier
{
    /// <inheritdoc />
    public Task<Result> SendAsync(
        DunningNotice notice,
        string recipientEmail,
        string? recipientPhone,
        CancellationToken cancellationToken)
    {
        _ = recipientEmail;
        _ = recipientPhone;
        return Task.FromResult(notice.MarkSent(DateTimeOffset.UtcNow));
    }
}
