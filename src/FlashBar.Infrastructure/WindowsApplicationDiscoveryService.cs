using FlashBar.Domain;
using FlashBar.Service;

namespace FlashBar.Infrastructure;

public sealed class WindowsApplicationDiscoveryService : IApplicationDiscoveryService
{
    private const int MaximumResults = 50;
    private readonly IReadOnlyList<string> _searchRoots;

    public WindowsApplicationDiscoveryService(IEnumerable<string>? searchRoots = null)
    {
        _searchRoots = (searchRoots ?? GetStartMenuRoots()).ToArray();
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

            foreach (var shortcutPath in Directory.EnumerateFiles(root, "*.lnk", options))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = Path.GetFileNameWithoutExtension(shortcutPath);
                if (name.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    var iconPath = WindowsShortcutIconCache.GetIconPath(shortcutPath);
                    results.TryAdd(shortcutPath, new ApplicationEntry(name, shortcutPath, iconPath));
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

    private static IReadOnlyList<string> GetStartMenuRoots()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
        };

        return roots
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
