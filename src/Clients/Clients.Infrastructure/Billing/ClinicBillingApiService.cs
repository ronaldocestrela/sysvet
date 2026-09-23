using System.Net.Http.Json;
using Core.Domain;

namespace Clients.Infrastructure.Billing;

/// <summary>Clinic admin calls for SaaS billing standing and payment (9.5).</summary>
public interface IClinicBillingApi
{
    /// <summary>Loads billing standing for the authenticated tenant admin.</summary>
    Task<Result<ClinicBillingStandingResponse>> GetStandingAsync(CancellationToken cancellationToken = default);

    /// <summary>Retries payment for the outstanding SaaS invoice.</summary>
    Task<Result<PayClinicBillingResponse>> PayOutstandingAsync(CancellationToken cancellationToken = default);
}

/// <summary>HTTP implementation of <see cref="IClinicBillingApi"/>.</summary>
public sealed class ClinicBillingApiService : IClinicBillingApi
{
    private readonly HttpClient _httpClient;

    /// <summary>Creates the service.</summary>
    public ClinicBillingApiService(HttpClient httpClient) => _httpClient = httpClient;

    /// <summary>Loads billing standing for the authenticated tenant admin.</summary>
    public async Task<Result<ClinicBillingStandingResponse>> GetStandingAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/api/v1/billing/standing", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<ClinicBillingStandingResponse>(
                new Error("Billing.StandingFailed", "Não foi possível carregar a situação de cobrança."));
        }

        var body = await response.Content.ReadFromJsonAsync<ClinicBillingStandingResponse>(cancellationToken);
        return body is null
            ? Result.Failure<ClinicBillingStandingResponse>(new Error("Billing.StandingEmpty", "Resposta vazia."))
            : Result.Success(body);
    }

    /// <summary>Retries payment for the outstanding SaaS invoice.</summary>
    public async Task<Result<PayClinicBillingResponse>> PayOutstandingAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync("/api/v1/billing/pay", null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<PayClinicBillingResponse>(
                new Error("Billing.PayFailed", "Falha ao iniciar pagamento."));
        }

        var body = await response.Content.ReadFromJsonAsync<PayClinicBillingResponse>(cancellationToken);
        return body is null
            ? Result.Failure<PayClinicBillingResponse>(new Error("Billing.PayEmpty", "Resposta vazia."))
            : Result.Success(body);
    }
}

/// <summary>API DTO mirror for billing standing.</summary>
public sealed record ClinicBillingStandingResponse(
    int Standing,
    bool IsOperationallyLocked,
    Guid? OutstandingInvoiceId,
    decimal? OutstandingAmount,
    int? OutstandingStatus,
    string? PixCopyPaste,
    string? BoletoLine,
    string? LatestGatewayPaymentId);

/// <summary>API DTO mirror for pay result.</summary>
public sealed record PayClinicBillingResponse(Guid InvoiceId, decimal Amount, string? GatewayPaymentId);
