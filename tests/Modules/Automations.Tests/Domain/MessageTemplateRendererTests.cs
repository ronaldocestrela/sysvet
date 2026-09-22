using Automations.Domain.Services;
using FluentAssertions;

namespace Automations.Tests.Domain;

public class MessageTemplateRendererTests
{
    [Fact]
    public void ReplacesKnownTokens()
    {
        var result = MessageTemplateRenderer.Render(
            "Olá {{TutorName}}, o pet {{PetName}} está pronto.",
            new Dictionary<string, string>
            {
                ["TutorName"] = "Maria",
                ["PetName"] = "Thor"
            });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Olá Maria, o pet Thor está pronto.");
    }

    [Fact]
    public void UnknownToken_Fails()
    {
        var result = MessageTemplateRenderer.Render(
            "Olá {{TutorName}}",
            new Dictionary<string, string>());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("UnknownToken");
    }
}
