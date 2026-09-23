using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Core.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Infrastructure.Configuration;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Infrastructure.Billing;

/// <summary>Asaas v3 adapter for SaaS tenant billing (9.4).</summary>
public sealed class AsaasBillingGateway : IBillingGateway
{
    private readonly HttpClient _httpClient;
    private readonly BillingOptions _options;
    private readonly ILogger<AsaasBillingGateway> _logger;

    /// <summary>Creates gateway client.</summary>
    public AsaasBillingGateway(
        HttpClient httpClient,
        IOptions<BillingOptions> options,
        ILogger<AsaasBillingGateway> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<string>> EnsureCustomerAsync(
        BillingGatewayCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return Result.Failure<string>(PlatformErrorCodes.Billing.GatewayFailed);
        }

        if (!string.IsNullOrWhiteSpace(request.ExistingGatewayCustomerId))
        {
            var updateUrl = $"{BaseUrl()}/v3/customers/{request.ExistingGatewayCustomerId}";
            using var updateRequest = CreateRequest(HttpMethod.Put, updateUrl, new AsaasCustomerPayload(
                request.Name,
                request.Email,
                request.CpfCnpj));
            var updateResponse = await _httpClient.SendAsync(updateRequest, cancellationToken);
            if (!updateResponse.IsSuccessStatusCode)
            {
                return await FailureFromResponse<string>(updateResponse, cancellationToken);
            }

            return Result.Success(request.ExistingGatewayCustomerId);
        }

        using var createRequest = CreateRequest(HttpMethod.Post, $"{BaseUrl()}/v3/customers", new AsaasCustomerPayload(
            request.Name,
            request.Email,
            request.CpfCnpj));
        var createResponse = await _httpClient.SendAsync(createRequest, cancellationToken);
        if (!createResponse.IsSuccessStatusCode)
        {
            return await FailureFromResponse<string>(createResponse, cancellationToken);
        }

        var body = await createResponse.Content.ReadFromJsonAsync<AsaasCustomerResponse>(cancellationToken);
        if (body?.Id is null)
        {
            return Result.Failure<string>(PlatformErrorCodes.Billing.GatewayFailed);
        }

        return Result.Success(body.Id);
    }

    /// <inheritdoc />
    public async Task<Result<BillingGatewayPaymentResult>> CreatePaymentAsync(
        BillingGatewayPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return Result.Failure<BillingGatewayPaymentResult>(PlatformErrorCodes.Billing.GatewayFailed);
        }

        var billingType = request.PaymentMethod switch
        {
            BillingPaymentMethodKind.CreditCard => "CREDIT_CARD",
            BillingPaymentMethodKind.Pix => "PIX",
            BillingPaymentMethodKind.Boleto => "BOLETO",
            _ => "UNDEFINED"
        };

        var payload = new AsaasPaymentPayload(
            request.GatewayCustomerId,
            billingType,
            request.Amount,
            request.DueDate.ToString("yyyy-MM-dd"),
            request.InvoiceId.ToString(),
            request.PaymentMethod == BillingPaymentMethodKind.CreditCard ? request.CreditCardToken : null);

        using var httpRequest = CreateRequest(HttpMethod.Post, $"{BaseUrl()}/v3/payments", payload);
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return await FailureFromResponse<BillingGatewayPaymentResult>(response, cancellationToken);
        }

        var body = await response.Content.ReadFromJsonAsync<AsaasPaymentResponse>(cancellationToken);
        if (body?.Id is null)
        {
            return Result.Failure<BillingGatewayPaymentResult>(PlatformErrorCodes.Billing.GatewayFailed);
        }

        return Result.Success(new BillingGatewayPaymentResult(
            body.Id,
            body.PixCopyPaste ?? body.EncodedImage,
            body.IdentificationField));
    }

    private string BaseUrl() => (_options.BaseUrl ?? "https://api.asaas.com").TrimEnd('/');

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, object body)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("access_token", _options.ApiKey);
        request.Content = JsonContent.Create(body);
        return request;
    }

    private async Task<Result<T>> FailureFromResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning("Asaas request failed: {Status} {Detail}", response.StatusCode, detail);
        return Result.Failure<T>(new Error(PlatformErrorCodes.Billing.GatewayFailed.Code, detail));
    }

    private sealed record AsaasCustomerPayload(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("cpfCnpj")] string CpfCnpj);

    private sealed record AsaasCustomerResponse([property: JsonPropertyName("id")] string? Id);

    private sealed record AsaasPaymentPayload(
        [property: JsonPropertyName("customer")] string Customer,
        [property: JsonPropertyName("billingType")] string BillingType,
        [property: JsonPropertyName("value")] decimal Value,
        [property: JsonPropertyName("dueDate")] string DueDate,
        [property: JsonPropertyName("externalReference")] string ExternalReference,
        [property: JsonPropertyName("creditCardToken")] string? CreditCardToken);

    private sealed record AsaasPaymentResponse(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("pixCopyPaste")] string? PixCopyPaste,
        [property: JsonPropertyName("encodedImage")] string? EncodedImage,
        [property: JsonPropertyName("identificationField")] string? IdentificationField);
}
