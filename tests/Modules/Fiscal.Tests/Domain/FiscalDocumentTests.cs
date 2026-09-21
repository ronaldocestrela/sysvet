using Fiscal.Domain.Entities;
using Fiscal.Domain.Enums;
using FluentAssertions;

namespace Fiscal.Tests.Domain;

public class FiscalDocumentTests
{
    [Fact]
    public void CreateDraft_Nfe_WithProductLine_ShouldAuthorize()
    {
        var docResult = FiscalDocument.CreateDraft(
            FiscalDocumentType.Nfe,
            Guid.NewGuid(),
            "Maria",
            "52998224725",
            "SP");
        docResult.IsSuccess.Should().BeTrue();
        var doc = docResult.Value;
        doc.AddProductItem(Guid.NewGuid(), "Ração", 1, 50m, "23091000", "5102", "102", 0).IsSuccess.Should().BeTrue();
        doc.BeginTransmission().IsSuccess.Should().BeTrue();
        doc.MarkAuthorized("35250912345678901234567890123456789012345678", "PROT1", "xml/key", "pdf/key", 1, 1, null)
            .IsSuccess.Should().BeTrue();
        doc.Status.Should().Be(FiscalDocumentStatus.Authorized);
    }

    [Fact]
    public void Cancel_AfterWindow_ShouldFail()
    {
        var doc = FiscalDocument.CreateDraft(FiscalDocumentType.Nfe, Guid.NewGuid(), "A", null, "SP").Value;
        doc.AddProductItem(null, "Item", 1, 1m, "23091000", "5102", "102", 0);
        doc.BeginTransmission();
        doc.MarkAuthorized(null, "P", "x", null, 1, 1, null);
        typeof(FiscalDocument).GetProperty(nameof(FiscalDocument.AuthorizedAt))!
            .SetValue(doc, DateTimeOffset.UtcNow.AddHours(-48));
        doc.Cancel(TimeSpan.FromHours(24)).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CorrectionLetter_WithForbiddenWord_ShouldFail()
    {
        var doc = FiscalDocument.CreateDraft(FiscalDocumentType.Nfe, Guid.NewGuid(), "A", null, "SP").Value;
        doc.AddProductItem(null, "Item", 1, 1m, "23091000", "5102", "102", 0);
        doc.BeginTransmission();
        doc.MarkAuthorized("35250912345678901234567890123456789012345678", "P", "x", null, 1, 1, null);
        doc.AddCorrectionLetter("Alteração do valor total da nota fiscal").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void FiscalTaxResolver_ShouldPickInterstateCfop()
    {
        Fiscal.Domain.Services.FiscalTaxResolver.ResolveProductCfop("SP", "RJ").Should().Be("6102");
        Fiscal.Domain.Services.FiscalTaxResolver.ResolveProductCfop("SP", "SP").Should().Be("5102");
    }

    private const string ValidNfceAccessKey = "35250912345678901234656789012345678901234567";

    [Fact]
    public void Nfce_ContingencyIssued_ThenTransmit_ShouldAuthorize()
    {
        var doc = FiscalDocument.CreateDraft(FiscalDocumentType.Nfce, Guid.NewGuid(), "Consumidor", null, "SP").Value;
        doc.AddProductItem(Guid.NewGuid(), "Ração", 1, 50m, "23091000", "5102", "102", 0).IsSuccess.Should().BeTrue();
        doc.MarkContingencyIssued(
                ValidNfceAccessKey,
                "xml/local",
                "https://sefaz.example/qr",
                42,
                1,
                FiscalEmissionType.ContingencyOffline)
            .IsSuccess.Should().BeTrue();
        doc.Status.Should().Be(FiscalDocumentStatus.ContingencyIssued);
        doc.EmissionType.Should().Be(FiscalEmissionType.ContingencyOffline);
        doc.BeginTransmission().IsSuccess.Should().BeTrue();
        doc.MarkAuthorized(ValidNfceAccessKey, "PROT-NFC", "xml/key", "pdf/key", 42, 1, null)
            .IsSuccess.Should().BeTrue();
        doc.Status.Should().Be(FiscalDocumentStatus.Authorized);
    }

    [Fact]
    public void Nfce_MarkContingency_WithInvalidModelKey_ShouldFail()
    {
        var doc = FiscalDocument.CreateDraft(FiscalDocumentType.Nfce, Guid.NewGuid(), "A", null, "SP").Value;
        doc.AddProductItem(null, "Item", 1, 1m, "23091000", "5102", "102", 0);
        doc.MarkContingencyIssued(
                "35250912345678901234567890123456789012345678",
                "xml",
                null,
                1,
                1,
                FiscalEmissionType.ContingencyOffline)
            .IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Nfce_CorrectionLetter_ShouldFail()
    {
        var doc = FiscalDocument.CreateDraft(FiscalDocumentType.Nfce, Guid.NewGuid(), "A", null, "SP").Value;
        doc.AddProductItem(null, "Item", 1, 1m, "23091000", "5102", "102", 0);
        doc.MarkContingencyIssued(ValidNfceAccessKey, "x", null, 1, 1, FiscalEmissionType.ContingencyOffline);
        doc.BeginTransmission();
        doc.MarkAuthorized(ValidNfceAccessKey, "P", "x", null, 1, 1, null);
        doc.AddCorrectionLetter("Correção de texto livre").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void IssuerProfile_ShouldConsumeNfceNumber()
    {
        var issuer = IssuerProfile.Create(
            "Clinic",
            "Clinic",
            Fiscal.Domain.ValueObjects.FiscalCnpj.Create("12345678000195").Value,
            "IE",
            "IM",
            "1234567",
            "Rua",
            "1",
            null,
            "Centro",
            "City",
            "SP",
            "01310100",
            3550308,
            "11999999999",
            "0107",
            2m,
            FiscalEnvironment.Homologation).Value;
        issuer.ConsumeNextNfceNumber().Should().Be(1);
        issuer.NextNfceNumber.Should().Be(2);
    }
}
