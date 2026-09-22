using Automations.Domain.Enums;
using Core.Domain;

namespace Automations.Application.Abstractions;

/// <summary>
/// Sends a rendered outbound message to the external channel (WhatsApp/SMS/e-mail).
/// </summary>
public interface IOutboundMessageSender
{
    /// <summary>
    /// Delivers the message; failures return <see cref="Result"/> without throwing for business errors.
    /// </summary>
    Task<Result> SendAsync(
        MessageChannel channel,
        string? subject,
        string body,
        string payloadJson,
        string? toPhone = null,
        string? toEmail = null,
        CancellationToken cancellationToken = default);
}
