using Automations.Application.Abstractions;
using Automations.Domain.Enums;
using Automations.Infrastructure.Configuration;
using Core.Domain;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Channels;

/// <summary>
/// Routes outbound jobs to Fake or Live channel gateways based on configuration.
/// </summary>
public sealed class ChannelOutboundMessageSender : IOutboundMessageSender
{
    private readonly FakeOutboundMessageSender _fake;
    private readonly SmtpEmailGateway _smtp;
    private readonly EvolutionWhatsAppGateway _evolution;
    private readonly AutomationsOptions _options;

    public ChannelOutboundMessageSender(
        FakeOutboundMessageSender fake,
        SmtpEmailGateway smtp,
        EvolutionWhatsAppGateway evolution,
        IOptions<AutomationsOptions> options)
    {
        _fake = fake;
        _smtp = smtp;
        _evolution = evolution;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<Result> SendAsync(
        MessageChannel channel,
        string? subject,
        string body,
        string payloadJson,
        string? toPhone = null,
        string? toEmail = null,
        CancellationToken cancellationToken = default)
    {
        if (channel == MessageChannel.Sms)
        {
            return Result.Failure(Domain.ErrorCodes.Channel.SmsNotSupported);
        }

        if (!string.Equals(_options.Provider, "Live", StringComparison.OrdinalIgnoreCase))
        {
            return await _fake.SendAsync(channel, subject, body, payloadJson, toPhone, toEmail, cancellationToken);
        }

        return channel switch
        {
            MessageChannel.Email => await _smtp.SendAsync(subject, body, toEmail ?? string.Empty, cancellationToken),
            MessageChannel.WhatsApp => await _evolution.SendAsync(body, toPhone ?? string.Empty, cancellationToken),
            _ => Result.Failure(Domain.ErrorCodes.Channel.SmsNotSupported)
        };
    }
}
