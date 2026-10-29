using FlashBar.Infrastructure;

namespace FlashBar.Tests.Infrastructure;

public class WindowsApplicationDiscoveryServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "flashbar-discovery-tests-" + Guid.NewGuid().ToString("N"));

    public WindowsApplicationDiscoveryServiceTests() => Directory.CreateDirectory(_directory);

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Fact]
    public async Task SearchAsync_FindsMatchingStartMenuShortcutsRecursively()
    {
        var nestedDirectory = Path.Combine(_directory, "Vendor", "Product");
        Directory.CreateDirectory(nestedDirectory);
        var shortcutPath = Path.Combine(nestedDirectory, "ExampleEditor.lnk");
        File.WriteAllText(shortcutPath, string.Empty);
        File.WriteAllText(Path.Combine(nestedDirectory, "ExampleEditor.exe"), string.Empty);
        File.WriteAllText(Path.Combine(nestedDirectory, "readme.txt"), string.Empty);

        var service = new WindowsApplicationDiscoveryService([_directory]);

        var results = await service.SearchAsync("editor");

        var application = Assert.Single(results);
        Assert.Equal("ExampleEditor", application.Name);
        Assert.Equal(shortcutPath, application.ExecutablePath);
        Assert.True(application.IconPath is null || File.Exists(application.IconPath));
    }
}
