using Core.Application.IntegrationEvents;
using Finance.Domain.Enums;
using Finance.Domain.Repositories;
using MediatR;

namespace Finance.Application.Integration;

/// <summary>
/// Reverses receivable allocations when a payment is refunded.
/// </summary>
public sealed class OrderPaymentRefundedIntegrationHandler : INotificationHandler<OrderPaymentRefundedEvent>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public OrderPaymentRefundedIntegrationHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task Handle(OrderPaymentRefundedEvent notification, CancellationToken cancellationToken)
    {
        var title = await _titleRepository.GetBySourceAsync(
            TitleSourceType.Sale,
            notification.OrderId,
            "-",
            cancellationToken);

        if (title is null)
        {
            return;
        }

        var result = title.ReverseAllocation(
            notification.Amount,
            DateTimeOffset.UtcNow,
            notification.Method,
            notification.RefundId);

        if (result.IsSuccess)
        {
            _titleRepository.Update(title);
        }
    }
}
