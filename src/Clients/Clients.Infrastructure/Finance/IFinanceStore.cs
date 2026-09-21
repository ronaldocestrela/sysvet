using Core.Domain;

namespace Clients.Infrastructure.Finance;

/// <summary>
/// Client port for AP/AR titles — offline mirror with sync outbox for manual operations.
/// </summary>
public interface IFinanceStore
{
    Task<Result<IReadOnlyList<FinanceTitleListItemDto>>> ListTitlesAsync(CancellationToken cancellationToken = default);

    Task<Result<BalanceProjectionClientDto>> GetProjectionAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<Result> SettleTitleAsync(Guid titleId, decimal amount, string method, CancellationToken cancellationToken = default);
}

public sealed class FinanceTitleListItemDto
{
    public Guid Id { get; init; }
    public string Direction { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateOnly DueDate { get; init; }
    public decimal OriginalAmount { get; init; }
    public decimal OpenAmount { get; init; }
    public string Description { get; init; } = string.Empty;
}

public sealed class BalanceProjectionClientDto
{
    public decimal ExpectedReceivable { get; init; }
    public decimal ExpectedPayable { get; init; }
    public decimal RealizedReceivable { get; init; }
    public decimal RealizedPayable { get; init; }
}
