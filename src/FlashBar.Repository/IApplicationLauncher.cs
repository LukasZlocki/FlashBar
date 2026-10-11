using FlashBar.Domain;

namespace FlashBar.Repository;

public interface IApplicationLauncher
{
    Task LaunchAsync(ApplicationEntry application, CancellationToken cancellationToken = default);
}
