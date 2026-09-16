using System.Text.Json;
using Xunit;

namespace Clients.Tests.BlazorWeb;

public class PwaManifestTests
{
    [Fact]
    public void Manifest_Has_Required_Install_Fields()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var path = Path.Combine(repoRoot, "src", "Clients", "BlazorWeb", "wwwroot", "manifest.json");

        var json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("name", out _));
        Assert.True(root.TryGetProperty("start_url", out _));
        Assert.True(root.GetProperty("icons").GetArrayLength() >= 2);

        var wwwroot = Path.GetDirectoryName(path)!;
        Assert.True(File.Exists(Path.Combine(wwwroot, "icon-192.png")));
        Assert.True(File.Exists(Path.Combine(wwwroot, "icon-512.png")));
    }
}
