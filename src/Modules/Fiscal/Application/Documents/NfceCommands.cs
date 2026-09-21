using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Fiscal.Domain.Enums;

namespace Fiscal.Application.Documents;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.FiscalWrite)]
public sealed record TransmitNfceCommand : ICommand<bool>
{
    public Guid DocumentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid IdempotencyKey { get; set; }
    public string AccessKey { get; init; } = string.Empty;
    public int Number { get; init; }
    public int Series { get; init; }
    public FiscalEmissionType EmissionType { get; init; }
    public string SignedXml { get; init; } = string.Empty;
    public string? QrCodeUrl { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string? RecipientCpf { get; init; }
    public string? RecipientUf { get; init; }
    public IReadOnlyList<TransmitNfceLineCommand> Lines { get; init; } = Array.Empty<TransmitNfceLineCommand>();
}

public sealed record TransmitNfceLineCommand(
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    string Ncm,
    string Cfop,
    string Csosn,
    int MerchandiseOrigin);

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.FiscalWrite)]
public sealed record ReconcileNfceStatusCommand(Guid DocumentId) : ICommand<bool>;

[AuthorizeRequest(AuthorizationPolicies.Cashier, Permissions.FiscalRead)]
public sealed record GetFiscalPosBundleQuery : IQuery<FiscalPosBundleDto?>;

/// <summary>Encrypted certificate bundle for PDV offline signing.</summary>
public sealed record FiscalPosBundleDto(
    Guid IssuerId,
    string LegalName,
    string TradeName,
    string Cnpj,
    string State,
    int IbgeCityCode,
    int NfceSeries,
    int NfeSeries,
    FiscalEnvironment Environment,
    bool HasCertificate,
    string? EncryptedPfxBase64,
    string? EncryptedCertificatePassword);
