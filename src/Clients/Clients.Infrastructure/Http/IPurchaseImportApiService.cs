using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>
/// Online API for NF-e purchase import (parse/confirm); requires connectivity.
/// </summary>
public interface IPurchaseImportApiService
{
    Task<Result<PurchaseImportPreviewClientDto>> ParseAsync(Stream xml, string fileName, CancellationToken cancellationToken = default);
    Task<Result<Guid>> ConfirmAsync(Guid importId, ConfirmPurchaseImportClientRequest body, CancellationToken cancellationToken = default);
    Task<Result<PurchaseImportDetailClientDto>> GetByIdAsync(Guid importId, CancellationToken cancellationToken = default);
}
