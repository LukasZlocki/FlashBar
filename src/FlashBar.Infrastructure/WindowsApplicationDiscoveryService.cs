using FlashBar.Domain;
using FlashBar.Service;

namespace FlashBar.Infrastructure;

public sealed class WindowsApplicationDiscoveryService : IApplicationDiscoveryService
{
    private const int MaximumResults = 50;
    private readonly IReadOnlyList<string> _searchRoots;

    public WindowsApplicationDiscoveryService(IEnumerable<string>? searchRoots = null)
    {
        _searchRoots = (searchRoots ?? GetSearchRoots()).ToArray();
    }

    public Task<IReadOnlyList<ApplicationEntry>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<IReadOnlyList<ApplicationEntry>>([]);
        }

        return Task.Run(() => SearchRoots(_searchRoots, query.Trim(), cancellationToken), cancellationToken);
    }

    private static IReadOnlyList<ApplicationEntry> SearchRoots(IReadOnlyList<string> roots, string query, CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, ApplicationEntry>(StringComparer.OrdinalIgnoreCase);
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
        };

        foreach (var root in roots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (var executablePath in Directory.EnumerateFiles(root, "*.exe", options))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = Path.GetFileNameWithoutExtension(executablePath);
                if (name.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    results.TryAdd(executablePath, new ApplicationEntry(name, executablePath));
                }
            }
        }

        return SortResults(results.Values, query).Take(MaximumResults).ToList();
    }

    private static IReadOnlyList<ApplicationEntry> SortResults(IEnumerable<ApplicationEntry> applications, string query)
        => applications
            .OrderBy(application => application.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(application => application.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static IReadOnlyList<string> GetSearchRoots()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs"),
        };

        return roots
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
