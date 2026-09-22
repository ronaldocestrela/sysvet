namespace Automations.Application.Abstractions;

/// <summary>
/// Resolves whether outbound delivery is allowed at the given instant (business hours).
/// </summary>
public interface IAutomationsDeliveryPolicy
{
    /// <summary>
    /// When <see cref="DeliveryPolicyResult.CanDeliver"/> is false, use <see cref="DeliveryPolicyResult.DeferUntil"/>.
    /// </summary>
    Task<DeliveryPolicyResult> EvaluateAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
}

/// <summary>
/// Outcome of a business-hours check for outbound delivery.
/// </summary>
public sealed record DeliveryPolicyResult(bool CanDeliver, DateTimeOffset DeferUntil);
