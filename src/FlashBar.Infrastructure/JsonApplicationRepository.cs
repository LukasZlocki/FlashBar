using System.Text.Json;
using FlashBar.Domain;
using FlashBar.Infrastructure.Json;
using FlashBar.Repository;

namespace FlashBar.Infrastructure;

public sealed class JsonApplicationRepository(string configPath) : IApplicationRepository
{
    public async Task<IReadOnlyList<ApplicationEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(configPath))
        {
            return [];
        }

        ApplicationsConfig? config;
        await using (var stream = File.OpenRead(configPath))
        {
            config = await JsonSerializer.DeserializeAsync(
                stream,
                ApplicationsConfigJsonContext.Default.ApplicationsConfig,
                cancellationToken);
        }

        var configDirectory = Path.GetDirectoryName(Path.GetFullPath(configPath))!;
        var entries = config?.Applications ?? [];
        var applications = new List<ApplicationEntry>(entries.Count);

        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];

            if (string.IsNullOrWhiteSpace(entry.Name) || string.IsNullOrWhiteSpace(entry.Path))
            {
                throw new InvalidDataException($"Application #{index + 1} in '{configPath}' must have a 'name' and a 'path'.");
            }

            applications.Add(new ApplicationEntry(entry.Name, entry.Path, ResolveIconPath(entry.Icon, configDirectory)));
        }

        return applications;
    }

    private static string? ResolveIconPath(string? icon, string configDirectory)
        => string.IsNullOrWhiteSpace(icon)
            ? null
            : Path.GetFullPath(Path.Combine(configDirectory, icon.Trim()));
}
