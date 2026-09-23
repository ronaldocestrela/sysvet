using Core.Application.Messaging;
using Core.Domain;

namespace Platform.Application.Billing;

/// <summary>Lists billing invoices for tenant.</summary>
/// <param name="TenantId">Target tenant.</param>
public sealed record ListTenantBillingInvoicesQuery(Guid TenantId) : IQuery<IReadOnlyList<BillingInvoiceDto>>;
