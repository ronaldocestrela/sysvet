using Intelligence.Domain.Reports;

namespace Intelligence.Application.Reports.Dtos;

/// <summary>ABC customer report payload.</summary>
public sealed record AbcCustomerReportDto(DateOnly From, DateOnly To, IReadOnlyList<AbcCustomerRowDto> Rows);

/// <summary>Single ABC customer row.</summary>
public sealed record AbcCustomerRowDto(
    Guid TutorId,
    string DisplayName,
    decimal NetAmount,
    decimal SharePercent,
    decimal CumulativeSharePercent,
    AbcClass Class,
    int Rank);

/// <summary>ABC product report payload.</summary>
public sealed record AbcProductReportDto(DateOnly From, DateOnly To, IReadOnlyList<AbcProductRowDto> Rows);

/// <summary>Single ABC product row.</summary>
public sealed record AbcProductRowDto(
    Guid ProductId,
    string ProductName,
    decimal NetAmount,
    decimal NetQuantity,
    decimal SharePercent,
    decimal CumulativeSharePercent,
    AbcClass Class,
    int Rank);

/// <summary>Productivity report payload.</summary>
public sealed record ProductivityReportDto(DateOnly From, DateOnly To, IReadOnlyList<ProductivityRowDto> Rows);

/// <summary>Productivity row with resolved staff name.</summary>
public sealed record ProductivityRowDto(
    Guid UserId,
    string DisplayName,
    decimal SalesNetAmount,
    decimal SalesQuantity,
    int ClinicalCompleted,
    int GroomingCompleted);

/// <summary>CSV export file for intelligence reports.</summary>
public sealed record IntelligenceReportFileDto(byte[] Content, string FileName, string ContentType);
