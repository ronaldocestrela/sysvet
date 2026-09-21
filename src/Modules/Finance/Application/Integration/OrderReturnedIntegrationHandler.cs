using Core.Application.IntegrationEvents;
using Finance.Domain.Enums;
using Finance.Domain.Repositories;
using MediatR;

namespace Finance.Application.Integration;

/// <summary>
/// Reverses receivable allocations when items are returned with refund.
/// </summary>
public sealed class OrderReturnedIntegrationHandler : INotificationHandler<OrderReturnedEvent>
{
    private readonly IFinancialTitleRepository _titleRepository;

    public OrderReturnedIntegrationHandler(IFinancialTitleRepository titleRepository)
    {
        _titleRepository = titleRepository;
    }

    public async Task Handle(OrderReturnedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.RefundAmount <= 0)
        {
            return;
        }

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
            notification.RefundAmount,
            DateTimeOffset.UtcNow,
            "Return",
            notification.ReturnId);

        if (result.IsSuccess)
        {
            _titleRepository.Update(title);
        }
    }
}
