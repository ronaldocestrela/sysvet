using Core.Application.IntegrationEvents;
using Finance.Application.Common;
using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Models;
using Finance.Domain.Repositories;
using MediatR;

namespace Finance.Application.Integration;

/// <summary>
/// Materializes settled receivable when a sale is paid.
/// </summary>
public sealed class OrderPaidIntegrationHandler : INotificationHandler<OrderPaidEvent>
{
    private readonly IFinancialTitleRepository _titleRepository;
    private readonly IFinancialCategoryRepository _categoryRepository;
    private readonly IMediator _mediator;

    public OrderPaidIntegrationHandler(
        IFinancialTitleRepository titleRepository,
        IFinancialCategoryRepository categoryRepository,
        IMediator mediator)
    {
        _titleRepository = titleRepository;
        _categoryRepository = categoryRepository;
        _mediator = mediator;
    }

    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        var existing = await _titleRepository.GetBySourceAsync(
            TitleSourceType.Sale,
            notification.OrderId,
            "-",
            cancellationToken);

        if (existing is not null)
        {
            await _mediator.Send(new MarkOrderFinanceLinkedRequest(notification.OrderId), cancellationToken);
            return;
        }

        var category = await FinanceCategoryBootstrap.EnsureSystemCategoryAsync(
            _categoryRepository,
            SystemCategoryCodes.Sales,
            "Vendas PDV",
            CategoryDirection.In,
            cancellationToken);

        var payments = notification.Payments
            .Select((p, index) => new SalePaymentSlice(
                p.Method,
                p.Amount,
                p.Nsu,
                PaymentCorrelation.ForOrderPayment(notification.OrderId, index, p.Method, p.Amount, p.Nsu)))
            .ToList();

        var titleResult = FinancialTitle.CreateFromSale(
            notification.OrderId,
            category.Id,
            notification.TotalAmount,
            notification.TutorId,
            $"Pedido {notification.OrderId:N}",
            payments,
            DateOnly.FromDateTime(DateTime.UtcNow));

        if (titleResult.IsFailure)
        {
            throw new InvalidOperationException(titleResult.Error.Message);
        }

        _titleRepository.Add(titleResult.Value);

        var linkResult = await _mediator.Send(new MarkOrderFinanceLinkedRequest(notification.OrderId), cancellationToken);
        if (linkResult.IsFailure)
        {
            throw new InvalidOperationException(linkResult.Error.Message);
        }
    }
}
