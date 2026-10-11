using FlashBar.Infrastructure;

namespace FlashBar.Tests.Infrastructure;

public class JsonApplicationRepositoryTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "flashbar-tests-" + Guid.NewGuid().ToString("N"));

    public JsonApplicationRepositoryTests() => Directory.CreateDirectory(_directory);

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private string WriteConfig(string json)
    {
        var path = Path.Combine(_directory, "apps.json");
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public async Task MissingFile_ReturnsEmptyList()
    {
        var repository = new JsonApplicationRepository(Path.Combine(_directory, "missing.json"));

        Assert.Empty(await repository.GetAllAsync());
    }

    [Fact]
    public async Task ParsesEntries_ResolvesIconRelativeToConfigDirectory()
    {
        var path = WriteConfig("""
        {
          // comments and trailing commas are allowed
          "applications": [
            { "name": "Notepad", "path": "C:\\Windows\\notepad.exe", "icon": "icons\\notepad.png", },
            { "name": "Calc", "path": "C:\\Windows\\calc.exe" }
          ]
        }
        """);

        var apps = await new JsonApplicationRepository(path).GetAllAsync();

        Assert.Equal(2, apps.Count);
        Assert.Equal("Notepad", apps[0].Name);
        Assert.Equal("C:\\Windows\\notepad.exe", apps[0].ExecutablePath);
        Assert.Equal(Path.Combine(_directory, "icons", "notepad.png"), apps[0].IconPath);
        Assert.Null(apps[1].IconPath);
    }

    [Fact]
    public async Task EntryWithoutPath_Throws()
    {
        var path = WriteConfig("""{ "applications": [ { "name": "Broken" } ] }""");

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() => new JsonApplicationRepository(path).GetAllAsync());

        Assert.Contains("#1", ex.Message);
    }

    [Fact]
    public async Task EmptyApplicationsArray_ReturnsEmptyList()
    {
        var path = WriteConfig("""{ "applications": [] }""");

        Assert.Empty(await new JsonApplicationRepository(path).GetAllAsync());
    }
}
