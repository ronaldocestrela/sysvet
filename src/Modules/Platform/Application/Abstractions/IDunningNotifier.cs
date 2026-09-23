using Core.Domain;
using Platform.Domain.Entities;

namespace Platform.Application.Abstractions;

/// <summary>Sends SaaS dunning notices (9.5).</summary>
public interface IDunningNotifier
{
    /// <summary>Delivers or records a dunning notice.</summary>
    Task<Result> SendAsync(DunningNotice notice, string recipientEmail, string? recipientPhone, CancellationToken cancellationToken);
}
