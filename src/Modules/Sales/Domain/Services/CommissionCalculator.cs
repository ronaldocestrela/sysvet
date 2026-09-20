using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Sales.Domain.Services;

/// <summary>
/// Pure commission accrual engine from order lines and tenant rules.
/// </summary>
public static class CommissionCalculator
{
    /// <summary>
    /// Builds accrual snapshots for a paid order (does not mutate the aggregate).
    /// </summary>
    public static IReadOnlyList<CommissionAccrual> Calculate(
        Order order,
        Guid sellerUserId,
        IReadOnlyList<CommissionRule> rules)
    {
        if (order.Status is not (OrderStatus.Paid or OrderStatus.PartiallyRefunded or OrderStatus.Refunded
            or OrderStatus.PartiallyReturned or OrderStatus.Returned))
        {
            return Array.Empty<CommissionAccrual>();
        }

        var subtotal = order.SubtotalAmount;
        var discountAmount = order.DiscountAmount;
        var accruals = new List<CommissionAccrual>();

        foreach (var item in order.Items)
        {
            var lineGross = item.TotalPrice.Amount;
            var lineNet = OrderPricing.ComputeLineNet(lineGross, subtotal, discountAmount);

            TryAddSellerAccrual(order, item, sellerUserId, lineNet, rules, accruals);

            if (item.PerformerUserId is Guid performerId && performerId != Guid.Empty && item.PerformerRole is CommissionRole performerRole)
            {
                TryAddPerformerAccrual(order, item, performerId, performerRole, lineNet, rules, accruals);
            }
        }

        return accruals;
    }

    private static void TryAddSellerAccrual(
        Order order,
        OrderItem item,
        Guid sellerUserId,
        decimal lineNet,
        IReadOnlyList<CommissionRule> rules,
        List<CommissionAccrual> accruals)
    {
        var rate = ResolveRate(CommissionRole.Seller, item.Kind, rules);
        if (rate <= 0 || sellerUserId == Guid.Empty)
        {
            return;
        }

        var amount = Math.Round(lineNet * rate / 100m, 2, MidpointRounding.AwayFromZero);
        if (amount <= 0)
        {
            return;
        }

        accruals.Add(new CommissionAccrual(
            Guid.NewGuid(),
            order.Id,
            item.Id,
            sellerUserId,
            CommissionRole.Seller,
            rate,
            lineNet,
            amount));
    }

    private static void TryAddPerformerAccrual(
        Order order,
        OrderItem item,
        Guid performerId,
        CommissionRole performerRole,
        decimal lineNet,
        IReadOnlyList<CommissionRule> rules,
        List<CommissionAccrual> accruals)
    {
        var rate = ResolveRate(performerRole, item.Kind, rules);
        if (rate <= 0)
        {
            return;
        }

        var amount = Math.Round(lineNet * rate / 100m, 2, MidpointRounding.AwayFromZero);
        if (amount <= 0)
        {
            return;
        }

        accruals.Add(new CommissionAccrual(
            Guid.NewGuid(),
            order.Id,
            item.Id,
            performerId,
            performerRole,
            rate,
            lineNet,
            amount));
    }

    private static decimal ResolveRate(CommissionRole role, OrderItemKind kind, IReadOnlyList<CommissionRule> rules)
    {
        var appliesTo = kind is OrderItemKind.Product or OrderItemKind.Kit
            ? CommissionAppliesTo.Product
            : CommissionAppliesTo.Service;
        var specific = rules.FirstOrDefault(r =>
            r.Role == role && r.AppliesTo == appliesTo);
        if (specific is not null)
        {
            return specific.RatePercent;
        }

        var all = rules.FirstOrDefault(r => r.Role == role && r.AppliesTo == CommissionAppliesTo.All);
        return all?.RatePercent ?? 0m;
    }
}
