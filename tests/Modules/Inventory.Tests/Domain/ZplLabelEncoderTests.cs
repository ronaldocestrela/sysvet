using FluentAssertions;
using Inventory.Domain.Services;
using Xunit;

namespace Inventory.Tests.Domain;

public class ZplLabelEncoderTests
{
    [Fact]
    public void EncodeSingleCopy_ContainsZplMarkersAndBarcode()
    {
        var zpl = ZplLabelEncoder.Encode("Produto Teste", "SKU-1", "7891234567890", copies: 1);

        zpl.Should().Contain("^XA");
        zpl.Should().Contain("^XZ");
        zpl.Should().Contain("^BC");
        zpl.Should().Contain("7891234567890");
        zpl.Should().Contain("SKU-1");
    }

    [Fact]
    public void EncodeMultipleCopies_ProducesMultipleBlocks()
    {
        var zpl = ZplLabelEncoder.Encode("Item", "A", "1234567890123", copies: 3);

        zpl.Split("^XA", StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(3);
    }
}
