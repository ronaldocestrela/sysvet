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
}
