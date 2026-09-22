using System.Text.Json;
using Commerce.Domain.Entities;
using Commerce.Domain.Enums;
using Commerce.Domain.Repositories;

namespace Commerce.Application.Services;

/// <summary>
/// Enqueues marketplace listing sync jobs after offer or stock changes.
/// </summary>
public static class MarketplaceSyncEnqueue
{
    /// <summary>Enqueues push listing job when Mercado Livre is enabled.</summary>
    public static async Task EnqueuePushListingAsync(
        IMarketplaceSyncJobRepository jobRepository,
        ProductOffer offer,
        decimal availableQuantity,
        CancellationToken cancellationToken)
    {
        if (!offer.MercadoLivreEnabled || !offer.IsPublished)
        {
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            offerId = offer.Id,
            sku = offer.Sku,
            title = offer.ProductName,
            price = offer.SalePrice.Amount,
            quantity = availableQuantity,
            listingId = offer.ExternalListingId
        });

        var key = $"push-listing:{offer.Id}:{offer.SalePrice.Amount}:{availableQuantity}";
        var existing = await jobRepository.GetByIdempotencyKeyAsync(key, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var job = MarketplaceSyncJob.Enqueue(
            MarketplaceSyncJobKind.PushListing,
            payload,
            key,
            offer.Id);
        if (job.IsSuccess)
        {
            jobRepository.Add(job.Value);
        }
    }
}
