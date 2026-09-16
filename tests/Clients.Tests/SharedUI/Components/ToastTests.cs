using Bunit;
using Microsoft.Extensions.DependencyInjection;
using SharedUI.Components;
using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI.Components;

public class ToastTests : BunitContext
{
    [Fact]
    public void Should_Render_Toast_From_Service()
    {
        var toastService = new ToastService();
        Services.AddSingleton<IToastService>(toastService);

        var cut = Render<Toast>();
        toastService.Show("Salvo com sucesso", ToastType.Success);
        cut.Render();

        Assert.Contains("Salvo com sucesso", cut.Markup);
        cut.Find(".toast.success");
    }

    [Fact]
    public void Dismiss_Button_Removes_Toast()
    {
        var toastService = new ToastService();
        Services.AddSingleton<IToastService>(toastService);

        var cut = Render<Toast>();
        toastService.Show("Msg");
        cut.Render();

        cut.Find("button.toast-close").Click();
        cut.Render();

        Assert.Empty(toastService.Toasts);
    }
}
