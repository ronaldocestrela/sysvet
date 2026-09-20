using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Sales.Domain.Enums;

namespace Sales.Application.Catalog;

[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record UpsertProductKitCommand(
    Guid? Id,
    string Name,
    bool IsActive,
    IReadOnlyList<ProductKitComponentDto> Components) : ICommand<Guid>;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.SalesRead)]
public sealed record ListProductKitsQuery : IQuery<IReadOnlyList<ProductKitDto>>;

[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record UpsertServicePackageCommand(
    Guid? Id,
    string Name,
    ServiceCode ServiceCode,
    int UsesPerUnit,
    bool IsActive) : ICommand<Guid>;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.SalesRead)]
public sealed record ListServicePackagesQuery : IQuery<IReadOnlyList<ServicePackageDto>>;

public sealed record ProductKitComponentDto(Guid ProductId, decimal QuantityPerKit);

public sealed record ProductKitDto(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyList<ProductKitComponentDto> Components);

public sealed record ServicePackageDto(
    Guid Id,
    string Name,
    ServiceCode ServiceCode,
    int UsesPerUnit,
    bool IsActive);
