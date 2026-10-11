using FlashBar.Domain;
using FlashBar.Repository;
using FlashBar.Service;

namespace FlashBar.Tests.Service;

public class ApplicationSearchServiceTests
{
    private sealed class FakeRepository(IReadOnlyList<ApplicationEntry> apps) : IApplicationRepository
    {
        public Task<IReadOnlyList<ApplicationEntry>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(apps);
    }

    [Fact]
    public async Task SearchAsync_FiltersRepositoryResults()
    {
        var service = new ApplicationSearchService(new FakeRepository(
        [
            new ApplicationEntry("Notepad", "notepad.exe"),
            new ApplicationEntry("Calculator", "calc.exe"),
        ]));

        var result = await service.SearchAsync("calc");

        Assert.Equal(["Calculator"], result.Select(a => a.Name));
    }
}
