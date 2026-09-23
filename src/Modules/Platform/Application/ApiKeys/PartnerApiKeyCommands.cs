using Core.Application.Messaging;

namespace Platform.Application.ApiKeys;

/// <summary>Issues a partner API key for a tenant (9.7).</summary>
public sealed record CreatePartnerApiKeyCommand(Guid TenantId, string PartnerName) : ICommand<CreatePartnerApiKeyResultDto>;

/// <summary>Lists partner API keys for a tenant.</summary>
public sealed record ListPartnerApiKeysQuery(Guid TenantId) : IQuery<IReadOnlyList<PartnerApiKeySummaryDto>>;

/// <summary>Revokes a partner API key.</summary>
public sealed record RevokePartnerApiKeyCommand(Guid TenantId, Guid KeyId) : ICommand;
