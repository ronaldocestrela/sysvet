using Fiscal.Domain.Enums;

namespace Fiscal.Application.Issuer;

/// <summary>Issuer profile API shape.</summary>
public sealed record IssuerProfileDto(
    Guid Id,
    string LegalName,
    string TradeName,
    string Cnpj,
    string StateRegistration,
    string MunicipalRegistration,
    string Cnae,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string PostalCode,
    int IbgeCityCode,
    string Phone,
    string NationalServiceTaxCode,
    decimal DefaultIssRate,
    int NfeSeries,
    long NextNfeNumber,
    int DpsSeries,
    long NextDpsNumber,
    FiscalEnvironment Environment,
    bool HasCertificate);
