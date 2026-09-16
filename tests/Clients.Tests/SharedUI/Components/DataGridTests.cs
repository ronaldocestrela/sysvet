using Bunit;
using Microsoft.AspNetCore.Components;
using SharedUI.Components;
using Xunit;

namespace Clients.Tests.SharedUI.Components;

public class DataGridTests : BunitContext
{
    [Fact]
    public void Should_Show_LoadingState_When_Items_Is_Null()
    {
        var cut = Render<DataGrid<string>>(parameters => parameters
            .Add(p => p.RowTemplate, (string _) => builder => builder.AddContent(0, "cell")));

        Assert.Contains("Carregando dados", cut.Markup);
        cut.Find(".loading-overlay");
    }

    [Fact]
    public void Should_Show_Empty_State_When_Items_Is_Empty()
    {
        var cut = Render<DataGrid<string>>(parameters => parameters
            .Add(p => p.Items, Array.Empty<string>())
            .Add(p => p.RowTemplate, (string _) => builder => builder.AddContent(0, "cell")));

        Assert.Contains("Nenhum registro encontrado.", cut.Markup);
    }

    [Fact]
    public void Should_Render_Table_When_Items_Exist()
    {
        var cut = Render<DataGrid<string>>(parameters => parameters
            .Add(p => p.Items, new[] { "a", "b" })
            .Add(p => p.HeaderTemplate, builder => builder.AddMarkupContent(0, "<th>Name</th>"))
            .Add(p => p.RowTemplate, item => builder => builder.AddMarkupContent(0, $"<td>{item}</td>")));

        cut.Find("table.data-grid");
        cut.Find("thead");
        Assert.Equal(2, cut.FindAll("tbody tr").Count);
    }
}
