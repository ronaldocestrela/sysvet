using Core.Domain;

namespace Clients.Infrastructure.Fiscal;

/// <summary>
/// Online API client for fiscal issuer and document emission (Fase 7.5).
/// </summary>
public sealed class FiscalApiService
{
    private readonly Http.ApiClient _apiClient;
    private readonly HttpClient _httpClient;
    private readonly Sync.ISyncConnectivity _connectivity;

    public FiscalApiService(Http.ApiClient apiClient, HttpClient httpClient, Sync.ISyncConnectivity connectivity)
    {
        _apiClient = apiClient;
        _httpClient = httpClient;
        _connectivity = connectivity;
    }

    public Task<Result<IReadOnlyList<FiscalDocumentClientDto>>> ListDocumentsAsync(Guid? orderId = null, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<FiscalDocumentClientDto>>(OfflineError()));
        }

        var query = orderId is null ? string.Empty : $"?orderId={orderId}";
        return _apiClient.GetAsync<IReadOnlyList<FiscalDocumentClientDto>>($"/api/v1/fiscal-documents{query}", cancellationToken);
    }

    public Task<Result<FiscalDocumentDetailClientDto?>> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<FiscalDocumentDetailClientDto?>(OfflineError()));
        }

        return _apiClient.GetAsync<FiscalDocumentDetailClientDto?>($"/api/v1/fiscal-documents/{id}", cancellationToken);
    }

    public Task<Result<IReadOnlyList<Guid>>> IssueFromOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<Guid>>(OfflineError()));
        }

        return _apiClient.PostAsync<object, IReadOnlyList<Guid>>(
            "/api/v1/fiscal-documents/from-order",
            new { orderId },
            idempotencyKey: Guid.NewGuid(),
            cancellationToken: cancellationToken);
    }

    public Task<Result<IssuerProfileClientDto?>> GetIssuerAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<IssuerProfileClientDto?>(OfflineError()));
        }

        return _apiClient.GetAsync<IssuerProfileClientDto?>("/api/v1/fiscal/issuer", cancellationToken);
    }

    public Task<Result<Guid>> UpsertIssuerAsync(UpsertIssuerClientRequest request, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<Guid>(OfflineError()));
        }

        return _apiClient.PutAsync<UpsertIssuerClientRequest, Guid>(
            "/api/v1/fiscal/issuer",
            request,
            idempotencyKey: Guid.NewGuid(),
            cancellationToken: cancellationToken);
    }

    public async Task<Result<bool>> UploadCertificateAsync(byte[] pfxBytes, string password, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Failure<bool>(OfflineError());
        }

        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(pfxBytes), "file", "certificate.pfx");
        form.Add(new StringContent(password), "password");
        var response = await _httpClient.PostAsync("/api/v1/fiscal/issuer/certificate", form, cancellationToken);
        return response.IsSuccessStatusCode
            ? Result.Success(true)
            : Result.Failure<bool>(new Error("Fiscal.CertificateUploadFailed", "Falha ao enviar certificado."));
    }

    public Task<Result<Http.DownloadedFile>> DownloadXmlAsync(Guid documentId, CancellationToken cancellationToken = default) =>
        _connectivity.IsOnline
            ? _apiClient.DownloadGetAsync($"/api/v1/fiscal-documents/{documentId}/xml", cancellationToken)
            : Task.FromResult(Result.Failure<Http.DownloadedFile>(OfflineError()));

    public Task<Result<Http.DownloadedFile>> DownloadDanfeAsync(Guid documentId, CancellationToken cancellationToken = default) =>
        _connectivity.IsOnline
            ? _apiClient.DownloadGetAsync($"/api/v1/fiscal-documents/{documentId}/danfe", cancellationToken)
            : Task.FromResult(Result.Failure<Http.DownloadedFile>(OfflineError()));

    private static Error OfflineError() =>
        new("Client.Offline", "Operação fiscal requer conexão com a API.");
}

public sealed record FiscalDocumentClientDto(
    Guid Id,
    string DocumentType,
    string Status,
    Guid SourceOrderId,
    string? AccessKey,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AuthorizedAt);

public sealed record FiscalDocumentDetailClientDto(
    Guid Id,
    string DocumentType,
    string Status,
    Guid SourceOrderId,
    string? AccessKey,
    string? Protocol,
    string? RejectionReason,
    string RecipientName,
    decimal TotalAmount);

public sealed record IssuerProfileClientDto(
    Guid Id,
    string LegalName,
    string TradeName,
    string Cnpj,
    bool HasCertificate);

public sealed record UpsertIssuerClientRequest(
    string LegalName,
    string TradeName,
    string Cnpj,
    string StateRegistration,
    string MunicipalRegistration,
    string Cnae,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string PostalCode,
    int IbgeCityCode,
    string Phone,
    string NationalServiceTaxCode,
    decimal DefaultIssRate,
    string Environment);
