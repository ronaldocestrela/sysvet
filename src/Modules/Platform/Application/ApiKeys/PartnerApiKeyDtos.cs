namespace Platform.Application.ApiKeys;

/// <summary>One-time secret returned when creating an API key.</summary>
public sealed record CreatePartnerApiKeyResultDto(
    Guid KeyId,
    string PartnerName,
    string KeyPrefix,
    string Secret,
    string Scope,
    DateTimeOffset CreatedAt);

/// <summary>API key metadata without secret.</summary>
public sealed record PartnerApiKeySummaryDto(
    Guid KeyId,
    string PartnerName,
    string KeyPrefix,
    string Scope,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? RevokedAt);
