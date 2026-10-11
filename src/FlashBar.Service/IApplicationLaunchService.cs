using FlashBar.Domain;
using FlashBar.Repository;
using System.Text.Json;

namespace FlashBar.Service;

public interface IApplicationLaunchService
{
    Task LaunchAsync(ApplicationEntry application, CancellationToken cancellationToken = default);
}

public sealed class ApplicationPersistenceException(string applicationName, Exception innerException)
    : Exception($"Application '{applicationName}' was started but could not be saved.", innerException);

public sealed class ApplicationLaunchService(
    IApplicationLauncher launcher,
    IApplicationRepository repository) : IApplicationLaunchService
{
    public async Task LaunchAsync(ApplicationEntry application, CancellationToken cancellationToken = default)
    {
        await launcher.LaunchAsync(application, cancellationToken);
        try
        {
            await repository.AddAsync(application);
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or JsonException
            or InvalidDataException
            or ArgumentException
            or NotSupportedException)
        {
            throw new ApplicationPersistenceException(application.Name, exception);
        }
    }
}
