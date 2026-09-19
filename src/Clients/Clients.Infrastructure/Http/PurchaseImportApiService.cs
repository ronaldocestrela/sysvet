using System.Net.Http.Json;
using Clients.Infrastructure.Sync;
using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>REST client for purchase NF-e import endpoints.</summary>
public sealed class PurchaseImportApiService : IPurchaseImportApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISyncConnectivity _connectivity;

    public PurchaseImportApiService(HttpClient httpClient, ISyncConnectivity connectivity)
    {
        _httpClient = httpClient;
        _connectivity = connectivity;
    }

    public async Task<Result<PurchaseImportPreviewClientDto>> ParseAsync(Stream xml, string fileName, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Failure<PurchaseImportPreviewClientDto>(new Error("PurchaseImport.Offline", "Conecte-se para importar XML de compra."));
        }

        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(xml), "file", fileName);
        var response = await _httpClient.PostAsync("/api/v1/inventory/purchase-imports/parse", form, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<PurchaseImportPreviewClientDto>(new Error("PurchaseImport.ParseFailed", "Falha ao processar XML."));
        }

        var dto = await response.Content.ReadFromJsonAsync<PurchaseImportPreviewClientDto>(cancellationToken);
        return dto is null
            ? Result.Failure<PurchaseImportPreviewClientDto>(new Error("PurchaseImport.ParseFailed", "Resposta inválida."))
            : Result.Success(dto);
    }

    public async Task<Result<Guid>> ConfirmAsync(Guid importId, ConfirmPurchaseImportClientRequest body, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Failure<Guid>(new Error("PurchaseImport.Offline", "Conecte-se para confirmar a importação."));
        }

        var response = await _httpClient.PostAsJsonAsync($"/api/v1/inventory/purchase-imports/{importId}/confirm", body, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<Guid>(new Error("PurchaseImport.ConfirmFailed", "Falha ao confirmar importação."));
        }

        var id = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken);
        return id == Guid.Empty
            ? Result.Failure<Guid>(new Error("PurchaseImport.ConfirmFailed", "Resposta inválida."))
            : Result.Success(id);
    }

    public async Task<Result<PurchaseImportDetailClientDto>> GetByIdAsync(Guid importId, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Failure<PurchaseImportDetailClientDto>(new Error("PurchaseImport.Offline", "Conecte-se para consultar a importação."));
        }

        var response = await _httpClient.GetAsync($"/api/v1/inventory/purchase-imports/{importId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<PurchaseImportDetailClientDto>(new Error("PurchaseImport.NotFound", "Importação não encontrada."));
        }

        var dto = await response.Content.ReadFromJsonAsync<PurchaseImportDetailClientDto>(cancellationToken);
        return dto is null
            ? Result.Failure<PurchaseImportDetailClientDto>(new Error("PurchaseImport.NotFound", "Resposta inválida."))
            : Result.Success(dto);
    }
}
