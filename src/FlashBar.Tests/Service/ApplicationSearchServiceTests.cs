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

        public Task AddAsync(ApplicationEntry application, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class FakeDiscoveryService(IReadOnlyList<ApplicationEntry> apps) : IApplicationDiscoveryService
    {
        public int SearchCount { get; private set; }

        public Task<IReadOnlyList<ApplicationEntry>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            SearchCount++;
            return Task.FromResult(apps);
        }
    }

    [Fact]
    public async Task SearchAsync_FiltersRepositoryResults()
    {
        var discovery = new FakeDiscoveryService([]);
        var service = new ApplicationSearchService(new FakeRepository(
        [
            new ApplicationEntry("Notepad", "notepad.exe"),
            new ApplicationEntry("Calculator", "calc.exe"),
        ]), discovery);

        var result = await service.SearchAsync("calc");

        Assert.Equal(["Calculator"], result.Select(a => a.Name));
        Assert.Equal(0, discovery.SearchCount);
    }

    [Fact]
    public async Task SearchAsync_UsesDiscoveryWhenConfiguredAppsHaveNoMatch()
    {
        var discovery = new FakeDiscoveryService([new ApplicationEntry("Visual Studio Code", "code.exe")]);
        var service = new ApplicationSearchService(new FakeRepository(
        [
            new ApplicationEntry("Notepad", "notepad.exe"),
        ]), discovery);
        var discoveryStarted = false;

        var result = await service.SearchAsync("code", onDiscoveryStarted: () => discoveryStarted = true);

        Assert.True(discoveryStarted);
        Assert.Equal(1, discovery.SearchCount);
        Assert.Equal(["Visual Studio Code"], result.Select(application => application.Name));
    }

    [Fact]
    public async Task SearchAsync_DoesNotDiscoverForEmptyQuery()
    {
        var discovery = new FakeDiscoveryService([new ApplicationEntry("Code", "code.exe")]);
        var service = new ApplicationSearchService(new FakeRepository([]), discovery);

        var result = await service.SearchAsync(string.Empty);

        Assert.Empty(result);
        Assert.Equal(0, discovery.SearchCount);
    }
}
