using FluentAssertions;
using Inventory.Application.PurchaseImports;
using Inventory.Domain;
using Inventory.Tests.Fixtures;

namespace Inventory.Tests.Application;

public class NfePurchaseXmlParserTests
{
    [Fact]
    public void Parse_SampleNfeProc_ExtractsHeaderAndLine()
    {
        using var stream = File.OpenRead(NfeFixturePaths.Resolve("sample-nfeProc.xml"));
        var result = NfePurchaseXmlParser.Parse(stream);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessKey.Value.Should().Be("35250900000000000000550010000000011000000001");
        result.Value.Lines.Should().HaveCount(1);
        result.Value.Lines[0].Barcode.Should().Be("7891234567890");
        result.Value.Duplicates.Should().ContainSingle(d => d.Number == "001");
    }

    [Fact]
    public void Parse_Rastro_SplitsIntoTwoLines()
    {
        using var stream = File.OpenRead(NfeFixturePaths.Resolve("sample-rastro.xml"));
        var result = NfePurchaseXmlParser.Parse(stream);

        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Should().HaveCount(2);
        result.Value.Lines[0].LotNumber.Should().Be("LOTE-A");
        result.Value.Lines[0].Barcode.Should().BeNull();
    }

    [Fact]
    public void Parse_InvalidXml_Fails()
    {
        using var stream = new MemoryStream("<not-xml"u8.ToArray());
        var result = NfePurchaseXmlParser.Parse(stream);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.PurchaseImport.InvalidXml.Code);
    }

    [Fact]
    public void Parse_SemGtin_BarcodeIsNull()
    {
        using var stream = File.OpenRead(NfeFixturePaths.Resolve("sample-rastro.xml"));
        var result = NfePurchaseXmlParser.Parse(stream);
        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Should().AllSatisfy(l => l.Barcode.Should().BeNull());
    }
}
