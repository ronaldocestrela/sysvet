namespace Finance.Domain.Models;

/// <summary>
/// Card settlement slice from AR allocations used for TEF reconciliation.
/// </summary>
public sealed record CardSettlementSnapshot(Guid AllocationId, string Nsu, decimal Amount, string Method);
