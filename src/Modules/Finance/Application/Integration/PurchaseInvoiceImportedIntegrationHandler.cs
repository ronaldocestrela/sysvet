using Core.Application.IntegrationEvents;
using Finance.Application.Common;
using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Repositories;
using MediatR;

namespace Finance.Application.Integration;

/// <summary>
/// Materializes payables when a purchase NF-e import is confirmed.
/// </summary>
public sealed class PurchaseInvoiceImportedIntegrationHandler : INotificationHandler<PurchaseInvoiceImportedEvent>
{
    private readonly IFinancialTitleRepository _titleRepository;
    private readonly IFinancialCategoryRepository _categoryRepository;
    private readonly IMediator _mediator;

    public PurchaseInvoiceImportedIntegrationHandler(
        IFinancialTitleRepository titleRepository,
        IFinancialCategoryRepository categoryRepository,
        IMediator mediator)
    {
        _titleRepository = titleRepository;
        _categoryRepository = categoryRepository;
        _mediator = mediator;
    }

    public async Task Handle(PurchaseInvoiceImportedEvent notification, CancellationToken cancellationToken)
    {
        var existing = await _titleRepository.ListBySourceIdAsync(
            TitleSourceType.Purchase,
            notification.ImportId,
            cancellationToken);

        if (existing.Count > 0)
        {
            await _mediator.Send(new MarkPurchaseInvoiceApLinkedRequest(notification.ImportId), cancellationToken);
            return;
        }

        var category = await FinanceCategoryBootstrap.EnsureSystemCategoryAsync(
            _categoryRepository,
            SystemCategoryCodes.Purchases,
            "Compras NF-e",
            CategoryDirection.Out,
            cancellationToken);

        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var duplicates = notification.Duplicates;

        if (duplicates.Count == 0)
        {
            var single = FinancialTitle.CreateFromPurchaseDuplicate(
                notification.ImportId,
                category.Id,
                notification.SupplierId,
                "-",
                notification.TotalAmount,
                issueDate,
                issueDate,
                $"NF-e {notification.AccessKey}");

            if (single.IsFailure)
            {
                throw new InvalidOperationException(single.Error.Message);
            }

            _titleRepository.Add(single.Value);
        }
        else
        {
            foreach (var dup in duplicates)
            {
                var due = dup.DueDate ?? issueDate;
                var title = FinancialTitle.CreateFromPurchaseDuplicate(
                    notification.ImportId,
                    category.Id,
                    notification.SupplierId,
                    dup.Number,
                    dup.Amount,
                    issueDate,
                    due,
                    $"NF-e {notification.AccessKey} dup {dup.Number}");

                if (title.IsFailure)
                {
                    throw new InvalidOperationException(title.Error.Message);
                }

                _titleRepository.Add(title.Value);
            }
        }

        var linkResult = await _mediator.Send(new MarkPurchaseInvoiceApLinkedRequest(notification.ImportId), cancellationToken);
        if (linkResult.IsFailure)
        {
            throw new InvalidOperationException(linkResult.Error.Message);
        }
    }
}
