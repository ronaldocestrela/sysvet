using SharedUI.Services;
using Xunit;

namespace Clients.Tests.SharedUI.Services;

public class ToastServiceTests
{
    [Fact]
    public void Show_Adds_Toast_To_Collection()
    {
        var sut = new ToastService();
        sut.Show("Operação concluída", ToastType.Success);

        Assert.Single(sut.Toasts);
        Assert.Equal("Operação concluída", sut.Toasts[0].Message);
        Assert.Equal(ToastType.Success, sut.Toasts[0].Type);
    }

    [Fact]
    public void Dismiss_Removes_Toast()
    {
        var sut = new ToastService();
        sut.Show("Msg");
        var id = sut.Toasts[0].Id;

        sut.Dismiss(id);

        Assert.Empty(sut.Toasts);
    }

    [Fact]
    public void Changed_Raises_When_Show_Or_Dismiss()
    {
        var sut = new ToastService();
        var count = 0;
        sut.Changed += () => count++;

        sut.Show("a");
        sut.Dismiss(sut.Toasts[0].Id);

        Assert.Equal(2, count);
    }
}
