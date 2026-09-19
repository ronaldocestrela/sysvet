using Xunit;

namespace Clients.Tests.Maui;

public class MauiBrandingTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string MauiRoot => Path.Combine(RepoRoot, "src", "Clients", "MauiApp");

    [Fact]
    public void Csproj_Has_VetNexus_Branding_And_Windows_Tfm()
    {
        var csproj = File.ReadAllText(Path.Combine(MauiRoot, "MauiApp.csproj"));

        Assert.Contains("<ApplicationTitle>SysVet | VetNexus</ApplicationTitle>", csproj);
        Assert.Contains("<ApplicationId>com.vetnexus.sysvet</ApplicationId>", csproj);
        Assert.Contains("<RootNamespace>MauiApp</RootNamespace>", csproj);
        Assert.Contains("Color=\"#4A90E2\"", csproj);
        Assert.Contains("net10.0-windows10.0.19041.0", csproj);
    }

    [Fact]
    public void BlazorRootComponent_Namespace_Aligned_With_Xaml()
    {
        var mainPage = File.ReadAllText(Path.Combine(MauiRoot, "MainPage.xaml"));
        var mainRazor = File.ReadAllText(Path.Combine(MauiRoot, "Main.razor"));

        Assert.Contains("xmlns:local=\"clr-namespace:MauiApp\"", mainPage);
        Assert.Contains("ComponentType=\"{x:Type local:Main}\"", mainPage);
        Assert.Contains("@namespace MauiApp", mainRazor);
    }

    [Fact]
    public void IndexHtml_Uses_Local_Bootstrap_Icons()
    {
        var html = File.ReadAllText(Path.Combine(MauiRoot, "wwwroot", "index.html"));

        Assert.Contains("_content/SharedUI/lib/bootstrap-icons/bootstrap-icons.min.css", html);
        Assert.DoesNotContain("cdn.jsdelivr.net", html);
        Assert.Contains("SysVet | VetNexus", html);
    }

    [Fact]
    public void AndroidManifest_Has_Network_And_Camera_Permissions()
    {
        var manifest = File.ReadAllText(Path.Combine(MauiRoot, "Platforms", "Android", "AndroidManifest.xml"));

        Assert.Contains("android.permission.INTERNET", manifest);
        Assert.Contains("android.permission.CAMERA", manifest);
        Assert.Contains("usesCleartextTraffic=\"true\"", manifest);
    }

    [Fact]
    public void AppIcon_Svg_Uses_VetNexus_Primary_Color()
    {
        var svg = File.ReadAllText(Path.Combine(MauiRoot, "Resources", "appicon.svg"));
        Assert.Contains("#4A90E2", svg, StringComparison.OrdinalIgnoreCase);
    }
}
