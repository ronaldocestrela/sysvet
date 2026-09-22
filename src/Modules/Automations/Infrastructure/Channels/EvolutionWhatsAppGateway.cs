using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Automations.Infrastructure.Configuration;
using Core.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure.Channels;

/// <summary>
/// Sends WhatsApp text messages via Evolution API v2.
/// </summary>
public sealed class EvolutionWhatsAppGateway
{
    private readonly HttpClient _httpClient;
    private readonly EvolutionOptions _options;
    private readonly ILogger<EvolutionWhatsAppGateway> _logger;

    public EvolutionWhatsAppGateway(
        HttpClient httpClient,
        IOptions<AutomationsOptions> options,
        ILogger<EvolutionWhatsAppGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.Evolution;
        _logger = logger;
    }

    /// <summary>
    /// Posts a plain-text WhatsApp message to the normalized phone number.
    /// </summary>
    public async Task<Result> SendAsync(string body, string toPhoneDigits, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.Instance))
        {
            return Result.Failure(new Error("Evolution.NotConfigured", "Evolution API não configurada."));
        }

        var number = toPhoneDigits.StartsWith("55") ? toPhoneDigits : "55" + toPhoneDigits;
        var url = $"{_options.BaseUrl.TrimEnd('/')}/message/sendText/{_options.Instance}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.Add("apikey", _options.ApiKey);
        }

        request.Content = JsonContent.Create(new EvolutionTextPayload(number, body));
        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result.Failure(new Error("Evolution.SendFailed", detail));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Evolution send failed to {Phone}", number);
            return Result.Failure(new Error("Evolution.SendFailed", ex.Message));
        }
    }

    private sealed record EvolutionTextPayload(
        [property: JsonPropertyName("number")] string Number,
        [property: JsonPropertyName("text")] string Text);
}
