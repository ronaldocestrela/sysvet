using API.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace API.IntegrationTests;

public class OpenApiModuleDocumentTransformerTests
{
    [Fact]
    public async Task TransformAsync_SetsApiVersionMetadata()
    {
        var transformer = new OpenApiModuleDocumentTransformer();
        var document = new OpenApiDocument();

        await transformer.TransformAsync(document, null!, CancellationToken.None);

        document.Info.Should().NotBeNull();
        document.Info!.Version.Should().Be("1.0.0");
        document.Info.Title.Should().Be("SysVet API");
    }
}
