using Commerce.Application.Dtos;
using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Commerce.Application.Commands;

/// <summary>Lists product offers for staff backoffice.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.CommerceRead)]
public sealed record ListProductOffersQuery : IQuery<IReadOnlyList<ProductOfferDto>>;

/// <summary>Creates or updates a product offer and sale price.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.CommerceWrite)]
public sealed record UpsertProductOfferCommand(
    Guid ProductId,
    decimal SalePrice,
    bool IsPublished,
    bool StoreEnabled,
    bool MercadoLivreEnabled) : ICommand<ProductOfferDto>;

/// <summary>Lists online orders for staff fulfillment.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.CommerceRead)]
public sealed record ListOnlineOrdersQuery(
    int Page = 1,
    int PageSize = Core.Application.Common.PageRequest.DefaultPageSize) : IQuery<Core.Application.Common.PagedResult<OnlineOrderDto>>;

/// <summary>Marks order ready for pickup.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.CommerceWrite)]
public sealed record MarkOnlineOrderReadyCommand(Guid OrderId) : ICommand;

/// <summary>Completes order fulfillment.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.CommerceWrite)]
public sealed record CompleteOnlineOrderCommand(Guid OrderId) : ICommand;

/// <summary>Cancels order and restores stock when already confirmed.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.CommerceWrite)]
public sealed record CancelOnlineOrderCommand(Guid OrderId) : ICommand;

/// <summary>Public storefront catalog for a tenant resolved by slug filter.</summary>
public sealed record GetPublicStoreCatalogQuery : IQuery<IReadOnlyList<PublicStoreProductDto>>;

/// <summary>Public product detail.</summary>
public sealed record GetPublicStoreProductQuery(Guid OfferId) : IQuery<PublicStoreProductDto>;

/// <summary>Guest checkout with immediate confirm and stock debit.</summary>
public sealed record PlacePublicStoreOrderCommand(
    string BuyerName,
    string BuyerPhone,
    string? BuyerEmail,
    IReadOnlyList<PublicStoreOrderLineDto> Lines) : ICommand<OnlineOrderDto>;

/// <summary>Reads Mercado Livre integration settings.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.CommerceRead)]
public sealed record GetMercadoLivreSettingsQuery : IQuery<MercadoLivreSettingsDto>;

/// <summary>Updates Mercado Livre token and seller mapping.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.CommerceWrite)]
public sealed record UpdateMercadoLivreSettingsCommand(
    string? AccessToken,
    long? UserId,
    string SiteId,
    bool IsEnabled) : ICommand<MercadoLivreSettingsDto>;

/// <summary>Processes Mercado Livre webhook notification (tenant from seller index).</summary>
public sealed record ProcessMercadoLivreNotificationCommand(long UserId, string Topic, string Resource) : ICommand;
