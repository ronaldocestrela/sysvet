using Automations.Application.Abstractions;
using Automations.Domain.Enums;
using Core.Domain;
using Microsoft.Extensions.Logging;

namespace Automations.Infrastructure.Channels;

/// <summary>
/// PoC sender that logs rendered messages until external providers ship in 8.2.
/// </summary>
public sealed class LoggingOutboundMessageSender : IOutboundMessageSender
{
    private readonly ILogger<LoggingOutboundMessageSender> _logger;

    public LoggingOutboundMessageSender(ILogger<LoggingOutboundMessageSender> logger) => _logger = logger;

    /// <inheritdoc />
    public Task<Result> SendAsync(
        MessageChannel channel,
        string? subject,
        string body,
        string payloadJson,
        string? toPhone = null,
        string? toEmail = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Automations outbound {Channel} subject={Subject} body={Body} payload={Payload}",
            channel,
            subject,
            body,
            payloadJson);

        return Task.FromResult(Result.Success());
    }
}
