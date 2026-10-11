using FlashBar.Domain;
using FlashBar.Repository;

namespace FlashBar.Service;

public interface IApplicationSearchService
{
    Task<IReadOnlyList<ApplicationEntry>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default,
        Action? onDiscoveryStarted = null);
}

public interface IApplicationDiscoveryService
{
    Task<IReadOnlyList<ApplicationEntry>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

public sealed class ApplicationSearchService(
    IApplicationRepository repository,
    IApplicationDiscoveryService discoveryService) : IApplicationSearchService
{
    public async Task<IReadOnlyList<ApplicationEntry>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default,
        Action? onDiscoveryStarted = null)
    {
        var applications = await repository.GetAllAsync(cancellationToken);
        var results = ApplicationSearch.Match(applications, query);
        if (results.Count > 0 || string.IsNullOrWhiteSpace(query))
        {
            return results;
        }

        cancellationToken.ThrowIfCancellationRequested();
        onDiscoveryStarted?.Invoke();
        var discoveredApplications = await discoveryService.SearchAsync(query, cancellationToken);
        return ApplicationSearch.Match(discoveredApplications, query);
    }
}
