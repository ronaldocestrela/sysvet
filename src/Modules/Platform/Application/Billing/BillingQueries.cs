using Core.Application.Messaging;
using Core.Domain;

namespace Platform.Application.Billing;

/// <summary>Lists billing invoices for tenant.</summary>
/// <param name="TenantId">Target tenant.</param>
public sealed record ListTenantBillingInvoicesQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = Core.Application.Common.PageRequest.DefaultPageSize) : IQuery<Core.Application.Common.PagedResult<BillingInvoiceDto>>;
