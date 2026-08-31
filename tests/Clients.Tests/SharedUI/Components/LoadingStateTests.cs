using Bunit;
using SharedUI.Components;
using Xunit;

namespace Clients.Tests.SharedUI.Components;

public class LoadingStateTests : BunitContext
{
    [Fact]
    public void Should_Render_LoadingSpinner_When_IsLoading_Is_True()
    {
        // Arrange & Act
        var cut = Render<LoadingState>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .Add(p => p.Message, "Carregando...")
        );

        // Assert
        cut.MarkupMatches(@"
            <div class=""loading-overlay"">
                <div class=""spinner""></div>
                <p>Carregando...</p>
            </div>
        ");
    }

    [Fact]
    public void Should_Render_ChildContent_When_IsLoading_Is_False()
    {
        // Arrange & Act
        var cut = Render<LoadingState>(parameters => parameters
            .Add(p => p.IsLoading, false)
            .AddChildContent("<div>Conteudo Filho</div>")
        );

        // Assert
        cut.MarkupMatches("<div>Conteudo Filho</div>");
    }
}
