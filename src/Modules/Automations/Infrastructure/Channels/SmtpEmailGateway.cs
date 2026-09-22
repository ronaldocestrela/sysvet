using System.Net;
using System.Net.Mail;
using Automations.Infrastructure.Configuration;
using Core.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Channels;

/// <summary>
/// Sends rendered e-mail bodies through SMTP.
/// </summary>
public sealed class SmtpEmailGateway
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailGateway> _logger;

    public SmtpEmailGateway(IOptions<AutomationsOptions> options, ILogger<SmtpEmailGateway> logger)
    {
        _options = options.Value.Smtp;
        _logger = logger;
    }

    /// <summary>
    /// Delivers an e-mail to the given recipient address.
    /// </summary>
    public Task<Result> SendAsync(string? subject, string body, string toEmail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(toEmail))
        {
            return Task.FromResult(Result.Failure(new Error("Smtp.NotConfigured", "SMTP ou destinatário não configurado.")));
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(string.IsNullOrWhiteSpace(_options.From) ? _options.User : _options.From),
                Subject = subject ?? "VetNexus",
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = string.IsNullOrWhiteSpace(_options.User)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_options.User, _options.Password)
            };

            client.Send(message);
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP send failed to {Email}", toEmail);
            return Task.FromResult(Result.Failure(new Error("Smtp.SendFailed", ex.Message)));
        }
    }
}
