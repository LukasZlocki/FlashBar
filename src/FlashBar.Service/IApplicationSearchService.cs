using FlashBar.Domain;
using FlashBar.Repository;

namespace FlashBar.Service;

public interface IApplicationSearchService
{
    Task<IReadOnlyList<ApplicationEntry>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

public sealed class ApplicationSearchService(IApplicationRepository repository) : IApplicationSearchService
{
    public async Task<IReadOnlyList<ApplicationEntry>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var applications = await repository.GetAllAsync(cancellationToken);
        return ApplicationSearch.Match(applications, query);
    }
}
