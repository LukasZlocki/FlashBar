using FlashBar.Domain;

namespace FlashBar.Tests.Domain;

public class ApplicationSearchTests
{
    private static readonly ApplicationEntry[] Apps =
    [
        new("Notepad", "notepad.exe"),
        new("Calculator", "calc.exe"),
        new("Paint", "mspaint.exe"),
        new("Notepad++", "notepad++.exe"),
    ];

    [Fact]
    public void EmptyQuery_ReturnsAllInConfigOrder()
    {
        var result = ApplicationSearch.Match(Apps, "   ");

        Assert.Equal(["Notepad", "Calculator", "Paint", "Notepad++"], result.Select(a => a.Name));
    }

    [Fact]
    public void NullQuery_ReturnsAll()
    {
        Assert.Equal(Apps.Length, ApplicationSearch.Match(Apps, null).Count);
    }

    [Fact]
    public void Match_IsCaseInsensitive()
    {
        var result = ApplicationSearch.Match(Apps, "PAINT");

        Assert.Equal(["Paint"], result.Select(a => a.Name));
    }

    [Fact]
    public void Match_PrefixMatchesRankFirst()
    {
        var result = ApplicationSearch.Match(Apps, "note");

        Assert.Equal(["Notepad", "Notepad++"], result.Select(a => a.Name));
    }

    [Fact]
    public void Match_ContainsMatchesAfterPrefixMatches()
    {
        var result = ApplicationSearch.Match(Apps, "pad");

        Assert.Equal(["Notepad", "Notepad++"], result.Select(a => a.Name));
    }

    [Fact]
    public void Match_NoHits_ReturnsEmpty()
    {
        Assert.Empty(ApplicationSearch.Match(Apps, "zzz"));
    }
}
