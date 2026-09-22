using System.Text.Json;
using Commerce.Application.Marketplace;
using Commerce.Domain.Entities;
using Commerce.Domain.Enums;
using Commerce.Domain.Repositories;

namespace Commerce.Infrastructure.Workers;

/// <summary>Processes due marketplace sync outbox jobs.</summary>
public sealed class MarketplaceSyncProcessor
{
    private readonly IMarketplaceSyncJobRepository _jobRepository;
    private readonly IProductOfferRepository _offerRepository;
    private readonly IEnumerable<IMarketplaceChannel> _channels;

    public MarketplaceSyncProcessor(
        IMarketplaceSyncJobRepository jobRepository,
        IProductOfferRepository offerRepository,
        IEnumerable<IMarketplaceChannel> channels)
    {
        _jobRepository = jobRepository;
        _offerRepository = offerRepository;
        _channels = channels;
    }

    /// <summary>Runs one batch of due jobs.</summary>
    public async Task<int> ProcessDueAsync(int batchSize, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.ListDueAsync(batchSize, now, cancellationToken);
        foreach (var job in jobs)
        {
            job.MarkProcessing();
            _jobRepository.Update(job);

            var success = job.Kind switch
            {
                MarketplaceSyncJobKind.PushListing => await ProcessPushListingAsync(job, cancellationToken),
                _ => true
            };

            if (success)
            {
                job.MarkSucceeded();
            }
            else
            {
                job.MarkFailed("Marketplace push failed", now);
            }

            _jobRepository.Update(job);
        }

        return jobs.Count;
    }

    private async Task<bool> ProcessPushListingAsync(MarketplaceSyncJob job, CancellationToken cancellationToken)
    {
        using var doc = JsonDocument.Parse(job.PayloadJson);
        var root = doc.RootElement;
        var offerId = root.GetProperty("offerId").GetGuid();
        var offer = await _offerRepository.GetByIdAsync(offerId, cancellationToken);
        if (offer is null)
        {
            return true;
        }

        var channel = _channels.FirstOrDefault(c => c.ChannelName == "MercadoLivre");
        if (channel is null)
        {
            return false;
        }

        var push = new MarketplaceListingPush(
            root.GetProperty("sku").GetString() ?? offer.Sku,
            root.GetProperty("title").GetString() ?? offer.ProductName,
            root.GetProperty("price").GetDecimal(),
            root.GetProperty("quantity").GetDecimal(),
            root.TryGetProperty("listingId", out var lid) ? lid.GetString() : offer.ExternalListingId);

        var result = await channel.PushListingAsync(push, cancellationToken);
        if (result.Success && !string.IsNullOrWhiteSpace(result.ExternalListingId))
        {
            offer.SetExternalListingId(result.ExternalListingId);
            _offerRepository.Update(offer);
        }

        return result.Success;
    }
}
