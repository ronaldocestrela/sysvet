namespace Clients.Infrastructure.Intelligence;

/// <summary>ABC customers report from API.</summary>
public sealed class AbcCustomerReportClientDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public IReadOnlyList<AbcCustomerRowClientDto> Rows { get; init; } = Array.Empty<AbcCustomerRowClientDto>();
}

/// <summary>ABC customer row.</summary>
public sealed class AbcCustomerRowClientDto
{
    public Guid TutorId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public decimal NetAmount { get; init; }
    public decimal SharePercent { get; init; }
    public decimal CumulativeSharePercent { get; init; }
    public string Class { get; init; } = string.Empty;
    public int Rank { get; init; }
}

/// <summary>ABC products report from API.</summary>
public sealed class AbcProductReportClientDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public IReadOnlyList<AbcProductRowClientDto> Rows { get; init; } = Array.Empty<AbcProductRowClientDto>();
}

/// <summary>ABC product row.</summary>
public sealed class AbcProductRowClientDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal NetAmount { get; init; }
    public decimal NetQuantity { get; init; }
    public decimal SharePercent { get; init; }
    public decimal CumulativeSharePercent { get; init; }
    public string Class { get; init; } = string.Empty;
    public int Rank { get; init; }
}

/// <summary>Productivity report from API.</summary>
public sealed class ProductivityReportClientDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public IReadOnlyList<ProductivityRowClientDto> Rows { get; init; } = Array.Empty<ProductivityRowClientDto>();
}

/// <summary>Productivity row.</summary>
public sealed class ProductivityRowClientDto
{
    public Guid UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public decimal SalesNetAmount { get; init; }
    public decimal SalesQuantity { get; init; }
    public int ClinicalCompleted { get; init; }
    public int GroomingCompleted { get; init; }
}
