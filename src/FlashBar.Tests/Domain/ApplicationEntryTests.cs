using FlashBar.Domain;

namespace FlashBar.Tests.Domain;

public class ApplicationEntryTests
{
    [Fact]
    public void Constructor_TrimsValuesAndKeepsIcon()
    {
        var entry = new ApplicationEntry("  Notepad  ", "  C:\\Windows\\notepad.exe ", " icon.png ");

        Assert.Equal("Notepad", entry.Name);
        Assert.Equal("C:\\Windows\\notepad.exe", entry.ExecutablePath);
        Assert.Equal("icon.png", entry.IconPath);
    }

    [Fact]
    public void Constructor_BlankIconBecomesNull()
    {
        var entry = new ApplicationEntry("Calc", "calc.exe", "   ");

        Assert.Null(entry.IconPath);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsBlankName(string name)
    {
        Assert.Throws<ArgumentException>(() => new ApplicationEntry(name, "calc.exe"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsBlankPath(string path)
    {
        Assert.Throws<ArgumentException>(() => new ApplicationEntry("Calc", path));
    }
}
