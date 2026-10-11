using FlashBar.Domain;

namespace FlashBar.Repository;

public interface IApplicationRepository
{
    Task<IReadOnlyList<ApplicationEntry>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(ApplicationEntry application, CancellationToken cancellationToken = default);
}
