using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using Core.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Application.Configuration;

namespace Platform.Infrastructure.Dunning;

/// <summary>SMTP e-mail and optional HTTP SMS for dunning (9.5).</summary>
public sealed class LiveDunningNotifier : IDunningNotifier
{
    private readonly DunningOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LiveDunningNotifier> _logger;

    /// <summary>Creates the notifier.</summary>
    public LiveDunningNotifier(
        IOptions<DunningOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<LiveDunningNotifier> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> SendAsync(
        DunningNotice notice,
        string recipientEmail,
        string? recipientPhone,
        CancellationToken cancellationToken)
    {
        Result delivery = notice.Channel switch
        {
            DunningChannel.Email => await SendEmailAsync(notice, recipientEmail, cancellationToken),
            DunningChannel.Sms => await SendSmsAsync(notice, recipientPhone, cancellationToken),
            _ => Result.Success()
        };

        if (delivery.IsFailure)
        {
            return delivery;
        }

        return notice.MarkSent(DateTimeOffset.UtcNow);
    }

    private Task<Result> SendEmailAsync(DunningNotice notice, string recipientEmail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) || string.IsNullOrWhiteSpace(recipientEmail))
        {
            return Task.FromResult(notice.MarkSent(DateTimeOffset.UtcNow));
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_options.SmtpFrom ?? _options.SmtpUser ?? "billing@vetnexus.app"),
                Subject = "VetNexus — pagamento pendente",
                Body = $"Sua fatura {notice.InvoiceId} está em atraso (passo {notice.StepDay}). Regularize em billing/payment.",
                IsBodyHtml = false
            };
            message.To.Add(recipientEmail);

            using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                EnableSsl = true,
                Credentials = string.IsNullOrWhiteSpace(_options.SmtpUser)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_options.SmtpUser, _options.SmtpPassword)
            };

            client.Send(message);
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dunning SMTP failed for tenant {TenantId}", notice.TenantId);
            return Task.FromResult(Result.Failure(Platform.Domain.ErrorCodes.Billing.GatewayFailed));
        }
    }

    private async Task<Result> SendSmsAsync(DunningNotice notice, string? recipientPhone, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.SmsBaseUrl) || string.IsNullOrWhiteSpace(recipientPhone))
        {
            return notice.MarkSent(DateTimeOffset.UtcNow);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("PlatformDunningSms");
            var response = await client.PostAsJsonAsync(
                "send",
                new { to = recipientPhone, text = $"VetNexus: fatura em atraso (passo {notice.StepDay})." },
                cancellationToken);
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(Platform.Domain.ErrorCodes.Billing.GatewayFailed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dunning SMS failed for tenant {TenantId}", notice.TenantId);
            return Result.Failure(Platform.Domain.ErrorCodes.Billing.GatewayFailed);
        }
    }
}
