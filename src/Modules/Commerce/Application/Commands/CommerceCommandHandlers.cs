using Commerce.Application.Dtos;
using Commerce.Application.Marketplace;
using Commerce.Application.Services;
using Commerce.Domain.Entities;
using CommerceErrorCodes = Commerce.Domain.ErrorCodes;
using Commerce.Domain.Enums;
using Commerce.Domain.Repositories;
using Commerce.Domain.ValueObjects;
using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;

namespace Commerce.Application.Commands;

/// <summary>Lists offers with available stock from Inventory.</summary>
public sealed class ListProductOffersQueryHandler : IRequestHandler<ListProductOffersQuery, Result<IReadOnlyList<ProductOfferDto>>>
{
    private readonly IProductOfferRepository _offerRepository;
    private readonly IMediator _mediator;

    public ListProductOffersQueryHandler(IProductOfferRepository offerRepository, IMediator mediator)
    {
        _offerRepository = offerRepository;
        _mediator = mediator;
    }

    public async Task<Result<IReadOnlyList<ProductOfferDto>>> Handle(ListProductOffersQuery request, CancellationToken cancellationToken)
    {
        var offers = await _offerRepository.ListAsync(cancellationToken);
        var snapshots = await LoadSnapshotsAsync(offers.Select(o => o.ProductId).ToList(), cancellationToken);
        var dtos = offers
            .Select(o => CommerceMapping.ToDto(o, snapshots.GetValueOrDefault(o.ProductId)))
            .ToList();
        return Result.Success<IReadOnlyList<ProductOfferDto>>(dtos);
    }

    private async Task<Dictionary<Guid, decimal>> LoadSnapshotsAsync(IReadOnlyList<Guid> productIds, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSellableProductSnapshotsRequest(productIds), cancellationToken);
        if (result.IsFailure)
        {
            return new Dictionary<Guid, decimal>();
        }

        return result.Value.ToDictionary(s => s.ProductId, s => s.AvailableQuantity);
    }
}

/// <summary>Upserts offer and refreshes catalog snapshots from Inventory.</summary>
public sealed class UpsertProductOfferCommandHandler : IRequestHandler<UpsertProductOfferCommand, Result<ProductOfferDto>>
{
    private readonly IProductOfferRepository _offerRepository;
    private readonly IMarketplaceSyncJobRepository _jobRepository;
    private readonly IMediator _mediator;

    public UpsertProductOfferCommandHandler(
        IProductOfferRepository offerRepository,
        IMarketplaceSyncJobRepository jobRepository,
        IMediator mediator)
    {
        _offerRepository = offerRepository;
        _jobRepository = jobRepository;
        _mediator = mediator;
    }

    public async Task<Result<ProductOfferDto>> Handle(UpsertProductOfferCommand request, CancellationToken cancellationToken)
    {
        var snapshotResult = await _mediator.Send(
            new GetSellableProductSnapshotsRequest([request.ProductId]),
            cancellationToken);
        if (snapshotResult.IsFailure || snapshotResult.Value.Count == 0)
        {
            return Result.Failure<ProductOfferDto>(CommerceErrorCodes.Offer.NotFound);
        }

        var snapshot = snapshotResult.Value[0];
        if (!snapshot.IsActive)
        {
            return Result.Failure<ProductOfferDto>(CommerceErrorCodes.Offer.NotFound);
        }

        var existing = await _offerRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
        ProductOffer offer;
        if (existing is null)
        {
            var create = ProductOffer.Create(request.ProductId, snapshot.Sku, snapshot.Name, request.SalePrice);
            if (create.IsFailure)
            {
                return Result.Failure<ProductOfferDto>(create.Error);
            }

            offer = create.Value;
            offer.SetPublished(request.IsPublished);
            offer.SetStoreEnabled(request.StoreEnabled);
            offer.SetMercadoLivreEnabled(request.MercadoLivreEnabled);
            _offerRepository.Add(offer);
        }
        else
        {
            offer = existing;
            var update = offer.UpdateCatalogSnapshot(snapshot.Sku, snapshot.Name, request.SalePrice);
            if (update.IsFailure)
            {
                return Result.Failure<ProductOfferDto>(update.Error);
            }

            offer.SetPublished(request.IsPublished);
            offer.SetStoreEnabled(request.StoreEnabled);
            offer.SetMercadoLivreEnabled(request.MercadoLivreEnabled);
            _offerRepository.Update(offer);
        }

        await MarketplaceSyncEnqueue.EnqueuePushListingAsync(
            _jobRepository,
            offer,
            snapshot.AvailableQuantity,
            cancellationToken);

        return Result.Success(CommerceMapping.ToDto(offer, snapshot.AvailableQuantity));
    }
}

/// <summary>Lists online orders.</summary>
public sealed class ListOnlineOrdersQueryHandler : IRequestHandler<ListOnlineOrdersQuery, Result<Core.Application.Common.PagedResult<OnlineOrderDto>>>
{
    private readonly IOnlineOrderRepository _orderRepository;

    public ListOnlineOrdersQueryHandler(IOnlineOrderRepository orderRepository) => _orderRepository = orderRepository;

    public async Task<Result<Core.Application.Common.PagedResult<OnlineOrderDto>>> Handle(ListOnlineOrdersQuery request, CancellationToken cancellationToken)
    {
        var pageRequest = Core.Application.Common.PageRequest.TryCreate(request.Page, request.PageSize);
        if (pageRequest.IsFailure)
        {
            return Result.Failure<Core.Application.Common.PagedResult<OnlineOrderDto>>(pageRequest.Error);
        }

        var (page, pageSize) = (pageRequest.Value.Page, pageRequest.Value.PageSize);
        var (orders, total) = await _orderRepository.ListPagedAsync(page, pageSize, cancellationToken);
        var items = orders.Select(CommerceMapping.ToDto).ToList();
        return Result.Success(new Core.Application.Common.PagedResult<OnlineOrderDto>(items, page, pageSize, total));
    }
}

/// <summary>Marks order ready for pickup.</summary>
public sealed class MarkOnlineOrderReadyCommandHandler : IRequestHandler<MarkOnlineOrderReadyCommand, Result>
{
    private readonly IOnlineOrderRepository _orderRepository;

    public MarkOnlineOrderReadyCommandHandler(IOnlineOrderRepository orderRepository) => _orderRepository = orderRepository;

    public async Task<Result> Handle(MarkOnlineOrderReadyCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(CommerceErrorCodes.Order.NotFound);
        }

        var result = order.MarkReadyForPickup();
        if (result.IsSuccess)
        {
            _orderRepository.Update(order);
        }

        return result;
    }
}

/// <summary>Completes online order.</summary>
public sealed class CompleteOnlineOrderCommandHandler : IRequestHandler<CompleteOnlineOrderCommand, Result>
{
    private readonly IOnlineOrderRepository _orderRepository;

    public CompleteOnlineOrderCommandHandler(IOnlineOrderRepository orderRepository) => _orderRepository = orderRepository;

    public async Task<Result> Handle(CompleteOnlineOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(CommerceErrorCodes.Order.NotFound);
        }

        var result = order.Complete();
        if (result.IsSuccess)
        {
            _orderRepository.Update(order);
        }

        return result;
    }
}

/// <summary>Cancels order and restores inventory when needed.</summary>
public sealed class CancelOnlineOrderCommandHandler : IRequestHandler<CancelOnlineOrderCommand, Result>
{
    private readonly IOnlineOrderRepository _orderRepository;
    private readonly IMediator _mediator;

    public CancelOnlineOrderCommandHandler(IOnlineOrderRepository orderRepository, IMediator mediator)
    {
        _orderRepository = orderRepository;
        _mediator = mediator;
    }

    public async Task<Result> Handle(CancelOnlineOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(CommerceErrorCodes.Order.NotFound);
        }

        var wasConfirmed = order.Status == OnlineOrderStatus.Confirmed || order.Status == OnlineOrderStatus.ReadyForPickup;
        var cancel = order.Cancel();
        if (cancel.IsFailure)
        {
            return cancel;
        }

        if (wasConfirmed)
        {
            var returnId = Guid.NewGuid();
            var lines = order.Lines
                .Select(l => new RestoreStockForSaleReturnLine(l.ProductId, l.Quantity))
                .ToList();
            var restore = await _mediator.Send(new RestoreStockForSaleReturnRequest(order.Id, returnId, lines), cancellationToken);
            if (restore.IsFailure)
            {
                return restore;
            }
        }

        _orderRepository.Update(order);
        return Result.Success();
    }
}

/// <summary>Public catalog of published store offers.</summary>
public sealed class GetPublicStoreCatalogQueryHandler : IRequestHandler<GetPublicStoreCatalogQuery, Result<IReadOnlyList<PublicStoreProductDto>>>
{
    private readonly IProductOfferRepository _offerRepository;
    private readonly IMediator _mediator;

    public GetPublicStoreCatalogQueryHandler(IProductOfferRepository offerRepository, IMediator mediator)
    {
        _offerRepository = offerRepository;
        _mediator = mediator;
    }

    public async Task<Result<IReadOnlyList<PublicStoreProductDto>>> Handle(GetPublicStoreCatalogQuery request, CancellationToken cancellationToken)
    {
        var offers = await _offerRepository.ListPublishedForStoreAsync(cancellationToken);
        if (offers.Count == 0)
        {
            return Result.Success<IReadOnlyList<PublicStoreProductDto>>(Array.Empty<PublicStoreProductDto>());
        }

        var snapshots = await _mediator.Send(
            new GetSellableProductSnapshotsRequest(offers.Select(o => o.ProductId).ToList()),
            cancellationToken);
        if (snapshots.IsFailure)
        {
            return Result.Failure<IReadOnlyList<PublicStoreProductDto>>(snapshots.Error);
        }

        var qtyByProduct = snapshots.Value.ToDictionary(s => s.ProductId, s => s.AvailableQuantity);
        var dtos = offers
            .Where(o => qtyByProduct.ContainsKey(o.ProductId))
            .Select(o => CommerceMapping.ToPublicDto(o, qtyByProduct[o.ProductId]))
            .ToList();
        return Result.Success<IReadOnlyList<PublicStoreProductDto>>(dtos);
    }
}

/// <summary>Public product detail.</summary>
public sealed class GetPublicStoreProductQueryHandler : IRequestHandler<GetPublicStoreProductQuery, Result<PublicStoreProductDto>>
{
    private readonly IProductOfferRepository _offerRepository;
    private readonly IMediator _mediator;

    public GetPublicStoreProductQueryHandler(IProductOfferRepository offerRepository, IMediator mediator)
    {
        _offerRepository = offerRepository;
        _mediator = mediator;
    }

    public async Task<Result<PublicStoreProductDto>> Handle(GetPublicStoreProductQuery request, CancellationToken cancellationToken)
    {
        var offer = await _offerRepository.GetByIdAsync(request.OfferId, cancellationToken);
        if (offer is null || !offer.IsPublished || !offer.StoreEnabled)
        {
            return Result.Failure<PublicStoreProductDto>(CommerceErrorCodes.Offer.NotPublished);
        }

        var snapshots = await _mediator.Send(new GetSellableProductSnapshotsRequest([offer.ProductId]), cancellationToken);
        if (snapshots.IsFailure || snapshots.Value.Count == 0)
        {
            return Result.Failure<PublicStoreProductDto>(CommerceErrorCodes.Offer.NotFound);
        }

        return Result.Success(CommerceMapping.ToPublicDto(offer, snapshots.Value[0].AvailableQuantity));
    }
}

/// <summary>Guest store checkout: confirm and debit stock atomically.</summary>
public sealed class PlacePublicStoreOrderCommandHandler : IRequestHandler<PlacePublicStoreOrderCommand, Result<OnlineOrderDto>>
{
    private readonly IProductOfferRepository _offerRepository;
    private readonly IOnlineOrderRepository _orderRepository;
    private readonly IMarketplaceSyncJobRepository _jobRepository;
    private readonly IMediator _mediator;

    public PlacePublicStoreOrderCommandHandler(
        IProductOfferRepository offerRepository,
        IOnlineOrderRepository orderRepository,
        IMarketplaceSyncJobRepository jobRepository,
        IMediator mediator)
    {
        _offerRepository = offerRepository;
        _orderRepository = orderRepository;
        _jobRepository = jobRepository;
        _mediator = mediator;
    }

    public async Task<Result<OnlineOrderDto>> Handle(PlacePublicStoreOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
        {
            return Result.Failure<OnlineOrderDto>(CommerceErrorCodes.Order.Empty);
        }

        var lineData = new List<(Guid ProductId, Guid OfferId, string Name, string Sku, decimal Qty, Money UnitPrice)>();
        foreach (var line in request.Lines)
        {
            var offer = await _offerRepository.GetByIdAsync(line.OfferId, cancellationToken);
            if (offer is null || !offer.IsPublished || !offer.StoreEnabled)
            {
                return Result.Failure<OnlineOrderDto>(CommerceErrorCodes.Offer.NotPublished);
            }

            var snapshots = await _mediator.Send(new GetSellableProductSnapshotsRequest([offer.ProductId]), cancellationToken);
            if (snapshots.IsFailure || snapshots.Value.Count == 0)
            {
                return Result.Failure<OnlineOrderDto>(CommerceErrorCodes.Offer.NotFound);
            }

            var snapshot = snapshots.Value[0];
            if (snapshot.AvailableQuantity < line.Quantity)
            {
                return Result.Failure<OnlineOrderDto>(CommerceErrorCodes.Order.InsufficientStock);
            }

            lineData.Add((
                offer.ProductId,
                offer.Id,
                offer.ProductName,
                offer.Sku,
                line.Quantity,
                offer.SalePrice));
        }

        var orderResult = OnlineOrder.CreateStorePickup(
            request.BuyerName,
            request.BuyerPhone,
            request.BuyerEmail,
            lineData);
        if (orderResult.IsFailure)
        {
            return Result.Failure<OnlineOrderDto>(orderResult.Error);
        }

        var order = orderResult.Value;
        var confirm = order.Confirm();
        if (confirm.IsFailure)
        {
            return Result.Failure<OnlineOrderDto>(confirm.Error);
        }

        var stockLines = order.Lines
            .Select(l => new ConsumeStockForSaleLine(l.ProductId, l.Quantity))
            .ToList();
        var stock = await _mediator.Send(new ConsumeStockForSaleRequest(order.Id, stockLines), cancellationToken);
        if (stock.IsFailure)
        {
            return Result.Failure<OnlineOrderDto>(stock.Error);
        }

        _orderRepository.Add(order);

        foreach (var offerId in order.Lines.Select(l => l.ProductOfferId).Distinct())
        {
            var offer = await _offerRepository.GetByIdAsync(offerId, cancellationToken);
            if (offer is null)
            {
                continue;
            }

            var snapshots = await _mediator.Send(new GetSellableProductSnapshotsRequest([offer.ProductId]), cancellationToken);
            var qty = snapshots.IsSuccess && snapshots.Value.Count > 0 ? snapshots.Value[0].AvailableQuantity : 0m;
            await MarketplaceSyncEnqueue.EnqueuePushListingAsync(_jobRepository, offer, qty, cancellationToken);
        }

        return Result.Success(CommerceMapping.ToDto(order));
    }
}

/// <summary>Reads Mercado Livre settings.</summary>
public sealed class GetMercadoLivreSettingsQueryHandler : IRequestHandler<GetMercadoLivreSettingsQuery, Result<MercadoLivreSettingsDto>>
{
    private readonly IMercadoLivreSettingsRepository _settingsRepository;

    public GetMercadoLivreSettingsQueryHandler(IMercadoLivreSettingsRepository settingsRepository) =>
        _settingsRepository = settingsRepository;

    public async Task<Result<MercadoLivreSettingsDto>> Handle(GetMercadoLivreSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetAsync(cancellationToken) ?? MercadoLivreSettings.CreateDefault();
        return Result.Success(new MercadoLivreSettingsDto(
            settings.IsEnabled,
            settings.UserId,
            settings.SiteId,
            !string.IsNullOrWhiteSpace(settings.AccessToken)));
    }
}

/// <summary>Updates Mercado Livre settings and global seller index.</summary>
public sealed class UpdateMercadoLivreSettingsCommandHandler : IRequestHandler<UpdateMercadoLivreSettingsCommand, Result<MercadoLivreSettingsDto>>
{
    private readonly IMercadoLivreSettingsRepository _settingsRepository;
    private readonly IMarketplaceSellerIndexRepository _sellerIndexRepository;
    private readonly ITenantContext _tenantContext;

    public UpdateMercadoLivreSettingsCommandHandler(
        IMercadoLivreSettingsRepository settingsRepository,
        IMarketplaceSellerIndexRepository sellerIndexRepository,
        ITenantContext tenantContext)
    {
        _settingsRepository = settingsRepository;
        _sellerIndexRepository = sellerIndexRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<MercadoLivreSettingsDto>> Handle(UpdateMercadoLivreSettingsCommand request, CancellationToken cancellationToken)
    {
        var existing = await _settingsRepository.GetAsync(cancellationToken);
        var settings = existing ?? MercadoLivreSettings.CreateDefault();
        settings.Update(request.AccessToken, request.UserId, request.SiteId, request.IsEnabled);

        if (existing is null)
        {
            _settingsRepository.Add(settings);
        }
        else
        {
            _settingsRepository.Update(settings);
        }

        if (settings.IsEnabled && settings.UserId.HasValue && _tenantContext.TenantId != Guid.Empty)
        {
            await _sellerIndexRepository.UpsertAsync(settings.UserId.Value, _tenantContext.TenantId, cancellationToken);
        }

        return Result.Success(new MercadoLivreSettingsDto(
            settings.IsEnabled,
            settings.UserId,
            settings.SiteId,
            !string.IsNullOrWhiteSpace(settings.AccessToken)));
    }
}

/// <summary>Handles Mercado Livre webhook by importing paid orders.</summary>
public sealed class ProcessMercadoLivreNotificationCommandHandler : IRequestHandler<ProcessMercadoLivreNotificationCommand, Result>
{
    private readonly IOnlineOrderRepository _orderRepository;
    private readonly IProductOfferRepository _offerRepository;
    private readonly IMarketplaceSyncJobRepository _jobRepository;
    private readonly IEnumerable<IMarketplaceChannel> _channels;
    private readonly IMediator _mediator;

    public ProcessMercadoLivreNotificationCommandHandler(
        IOnlineOrderRepository orderRepository,
        IProductOfferRepository offerRepository,
        IMarketplaceSyncJobRepository jobRepository,
        IEnumerable<IMarketplaceChannel> channels,
        IMediator mediator)
    {
        _orderRepository = orderRepository;
        _offerRepository = offerRepository;
        _jobRepository = jobRepository;
        _channels = channels;
        _mediator = mediator;
    }

    public async Task<Result> Handle(ProcessMercadoLivreNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!request.Topic.Contains("orders", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success();
        }

        var channel = _channels.FirstOrDefault(c => c.ChannelName == "MercadoLivre");
        if (channel is null)
        {
            return Result.Failure(CommerceErrorCodes.Marketplace.NotConfigured);
        }

        var externalId = request.Resource.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? request.Resource;
        var existing = await _orderRepository.GetByExternalOrderIdAsync(externalId, cancellationToken);
        if (existing is not null)
        {
            return Result.Success();
        }

        var remote = await channel.GetOrderAsync(externalId, cancellationToken);
        if (remote is null)
        {
            return Result.Success();
        }

        var lineData = new List<(Guid ProductId, Guid OfferId, string Name, string Sku, decimal Qty, Money UnitPrice)>();
        foreach (var remoteLine in remote.Lines)
        {
            var offers = await _offerRepository.ListAsync(cancellationToken);
            var offer = offers.FirstOrDefault(o => o.Sku.Equals(remoteLine.Sku, StringComparison.OrdinalIgnoreCase));
            if (offer is null)
            {
                continue;
            }

            var price = Money.Create(remoteLine.UnitPrice);
            if (price.IsFailure)
            {
                return Result.Failure(price.Error);
            }

            lineData.Add((offer.ProductId, offer.Id, offer.ProductName, offer.Sku, remoteLine.Quantity, price.Value));
        }

        if (lineData.Count == 0)
        {
            return Result.Failure(CommerceErrorCodes.Order.Empty);
        }

        var orderResult = OnlineOrder.CreateMercadoLivre(
            remote.BuyerName,
            remote.BuyerPhone,
            remote.BuyerEmail,
            remote.ExternalOrderId,
            lineData);
        if (orderResult.IsFailure)
        {
            return orderResult;
        }

        var order = orderResult.Value;
        order.Confirm();
        var stockLines = order.Lines.Select(l => new ConsumeStockForSaleLine(l.ProductId, l.Quantity)).ToList();
        var stock = await _mediator.Send(new ConsumeStockForSaleRequest(order.Id, stockLines), cancellationToken);
        if (stock.IsFailure)
        {
            return stock;
        }

        _orderRepository.Add(order);
        return Result.Success();
    }
}
