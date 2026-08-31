using Bunit;
using SharedUI.Components;
using Xunit;

namespace Clients.Tests.SharedUI.Components;

public class FormFieldTests : BunitContext
{
    [Fact]
    public void Should_Render_Label_When_Label_Provided()
    {
        // Arrange & Act
        var cut = Render<FormField>(parameters => parameters
            .Add(p => p.Label, "Nome")
        );

        // Assert
        cut.Find("label").TextContent.MarkupMatches("Nome");
    }

    [Fact]
    public void Should_Render_ChildContent_Inside_FieldControl()
    {
        // Arrange & Act
        var cut = Render<FormField>(parameters => parameters
            .AddChildContent("<input type='text' id='name' />")
        );

        // Assert
        cut.Find(".field-control").MarkupMatches(@"
            <div class=""field-control"">
                <input type='text' id='name' />
            </div>
        ");
    }

    [Fact]
    public void Should_Render_Error_When_Error_Provided()
    {
        // Arrange & Act
        var cut = Render<FormField>(parameters => parameters
            .Add(p => p.Error, "Campo obrigatório")
        );

        // Assert
        cut.Find(".error-msg").TextContent.MarkupMatches("Campo obrigatório");
    }
}
