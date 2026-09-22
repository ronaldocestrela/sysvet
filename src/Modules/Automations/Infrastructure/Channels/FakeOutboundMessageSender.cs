using Automations.Application.Abstractions;
using Automations.Domain.Enums;
using Core.Domain;

namespace Automations.Infrastructure.Channels;

/// <summary>
/// No-op sender used in CI and local development (Provider=Fake).
/// </summary>
public sealed class FakeOutboundMessageSender : IOutboundMessageSender
{
    /// <inheritdoc />
    public Task<Result> SendAsync(
        MessageChannel channel,
        string? subject,
        string body,
        string payloadJson,
        string? toPhone = null,
        string? toEmail = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success());
}
