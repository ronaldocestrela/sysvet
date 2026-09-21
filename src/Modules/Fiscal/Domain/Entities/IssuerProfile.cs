using Core.Domain;
using Fiscal.Domain.Enums;
using Fiscal.Domain.ValueObjects;

namespace Fiscal.Domain.Entities;

/// <summary>
/// Tenant issuer (clinic CNPJ) configuration for NF-e and NFS-e Nacional.
/// </summary>
public sealed class IssuerProfile : AggregateRoot
{
    public string LegalName { get; private set; } = string.Empty;
    public string TradeName { get; private set; } = string.Empty;
    public FiscalCnpj Cnpj { get; private set; } = null!;
    public string StateRegistration { get; private set; } = string.Empty;
    public string MunicipalRegistration { get; private set; } = string.Empty;
    public int TaxRegimeCode { get; private set; } = 1;
    public string Cnae { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string? Complement { get; private set; }
    public string District { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public int IbgeCityCode { get; private set; }
    public string Phone { get; private set; } = string.Empty;
    public string NationalServiceTaxCode { get; private set; } = string.Empty;
    public decimal DefaultIssRate { get; private set; }
    public int NfeSeries { get; private set; } = 1;
    public long NextNfeNumber { get; private set; } = 1;
    public int DpsSeries { get; private set; } = 1;
    public long NextDpsNumber { get; private set; } = 1;
    public FiscalEnvironment Environment { get; private set; } = FiscalEnvironment.Homologation;
    public string? CertificateBlobKey { get; private set; }
    public string? EncryptedCertificatePassword { get; private set; }

    private IssuerProfile() { }

    /// <summary>Creates the single issuer profile for a tenant.</summary>
    public static Result<IssuerProfile> Create(
        string legalName,
        string tradeName,
        FiscalCnpj cnpj,
        string stateRegistration,
        string municipalRegistration,
        string cnae,
        string street,
        string number,
        string? complement,
        string district,
        string city,
        string state,
        string postalCode,
        int ibgeCityCode,
        string phone,
        string nationalServiceTaxCode,
        decimal defaultIssRate,
        FiscalEnvironment environment,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result.Failure<IssuerProfile>(new Error("Fiscal.Issuer.InvalidName", "Legal name is required."));
        }

        var profile = new IssuerProfile
        {
            Id = id ?? Guid.NewGuid(),
            LegalName = legalName.Trim(),
            TradeName = string.IsNullOrWhiteSpace(tradeName) ? legalName.Trim() : tradeName.Trim(),
            Cnpj = cnpj,
            StateRegistration = stateRegistration?.Trim() ?? string.Empty,
            MunicipalRegistration = municipalRegistration?.Trim() ?? string.Empty,
            Cnae = cnae?.Trim() ?? string.Empty,
            Street = street.Trim(),
            Number = number.Trim(),
            Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim(),
            District = district?.Trim() ?? string.Empty,
            City = city.Trim(),
            State = state.Trim().ToUpperInvariant(),
            PostalCode = new string(postalCode.Where(char.IsDigit).ToArray()),
            IbgeCityCode = ibgeCityCode,
            Phone = phone?.Trim() ?? string.Empty,
            NationalServiceTaxCode = nationalServiceTaxCode?.Trim() ?? string.Empty,
            DefaultIssRate = defaultIssRate,
            Environment = environment
        };

        return Result.Success(profile);
    }

    /// <summary>Updates mutable issuer fields (not numbering or certificate).</summary>
    public Result Update(
        string legalName,
        string tradeName,
        string stateRegistration,
        string municipalRegistration,
        string cnae,
        string street,
        string number,
        string? complement,
        string district,
        string city,
        string state,
        string postalCode,
        int ibgeCityCode,
        string phone,
        string nationalServiceTaxCode,
        decimal defaultIssRate,
        FiscalEnvironment environment)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            return Result.Failure(new Error("Fiscal.Issuer.InvalidName", "Legal name is required."));
        }

        LegalName = legalName.Trim();
        TradeName = string.IsNullOrWhiteSpace(tradeName) ? legalName.Trim() : tradeName.Trim();
        StateRegistration = stateRegistration?.Trim() ?? string.Empty;
        MunicipalRegistration = municipalRegistration?.Trim() ?? string.Empty;
        Cnae = cnae?.Trim() ?? string.Empty;
        Street = street.Trim();
        Number = number.Trim();
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
        District = district?.Trim() ?? string.Empty;
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
        PostalCode = new string(postalCode.Where(char.IsDigit).ToArray());
        IbgeCityCode = ibgeCityCode;
        Phone = phone?.Trim() ?? string.Empty;
        NationalServiceTaxCode = nationalServiceTaxCode?.Trim() ?? string.Empty;
        DefaultIssRate = defaultIssRate;
        Environment = environment;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Stores encrypted certificate metadata after upload.</summary>
    public void SetCertificate(string blobKey, string encryptedPassword)
    {
        CertificateBlobKey = blobKey;
        EncryptedCertificatePassword = encryptedPassword;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Whether transmission can proceed.</summary>
    public bool HasCertificate =>
        !string.IsNullOrWhiteSpace(CertificateBlobKey)
        && !string.IsNullOrWhiteSpace(EncryptedCertificatePassword);

    /// <summary>Reserves the next NF-e number atomically at persistence layer.</summary>
    public long ConsumeNextNfeNumber() => NextNfeNumber++;

    /// <summary>Reserves the next DPS number.</summary>
    public long ConsumeNextDpsNumber() => NextDpsNumber++;
}
