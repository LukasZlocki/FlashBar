using FlashBar.Domain;
using FlashBar.Repository;
using FlashBar.Service;

namespace FlashBar.Tests.Service;

public class ApplicationLaunchServiceTests
{
    private sealed class FakeRepository : IApplicationRepository
    {
        public List<ApplicationEntry> Applications { get; } = [];

        public Task<IReadOnlyList<ApplicationEntry>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ApplicationEntry>>(Applications);

        public Task AddAsync(ApplicationEntry application, CancellationToken cancellationToken = default)
        {
            Applications.Add(application);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeLauncher(Exception? error = null) : IApplicationLauncher
    {
        public bool WasLaunched { get; private set; }

        public Task LaunchAsync(ApplicationEntry application, CancellationToken cancellationToken = default)
        {
            if (error is not null)
            {
                throw error;
            }

            WasLaunched = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task LaunchAsync_RemembersApplicationAfterSuccessfulLaunch()
    {
        var application = new ApplicationEntry("Code", "code.exe");
        var launcher = new FakeLauncher();
        var repository = new FakeRepository();
        var service = new ApplicationLaunchService(launcher, repository);

        await service.LaunchAsync(application);

        Assert.True(launcher.WasLaunched);
        Assert.Same(application, Assert.Single(repository.Applications));
    }

    [Fact]
    public async Task LaunchAsync_DoesNotRememberApplicationWhenLaunchFails()
    {
        var repository = new FakeRepository();
        var service = new ApplicationLaunchService(new FakeLauncher(new InvalidOperationException("launch failed")), repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.LaunchAsync(new ApplicationEntry("Code", "code.exe")));

        Assert.Empty(repository.Applications);
    }
}
