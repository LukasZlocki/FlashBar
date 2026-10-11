using FlashBar.Domain;
using FlashBar.Repository;

namespace FlashBar.Service;

public interface IApplicationLaunchService
{
    Task LaunchAsync(ApplicationEntry application, CancellationToken cancellationToken = default);
}

public sealed class ApplicationLaunchService(IApplicationLauncher launcher) : IApplicationLaunchService
{
    public Task LaunchAsync(ApplicationEntry application, CancellationToken cancellationToken = default)
        => launcher.LaunchAsync(application, cancellationToken);
}
