using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Inventory.Application.PurchaseImports.Dtos;

namespace Inventory.Application.PurchaseImports.Queries;

/// <summary>Returns a single import with conference lines.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.PurchaseImportsRead)]
public sealed record GetPurchaseImportByIdQuery(Guid ImportId) : IQuery<PurchaseImportDetailDto>;

/// <summary>Lists recent purchase imports.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.PurchaseImportsRead)]
public sealed record ListPurchaseImportsQuery(int Take = 50) : IQuery<IReadOnlyList<PurchaseImportListItemDto>>;
