using System.Text.Json;
using FlashBar.Domain;
using FlashBar.Infrastructure.Json;
using FlashBar.Repository;

namespace FlashBar.Infrastructure;

public sealed class JsonApplicationRepository(string configPath, string? defaultConfigPath = null) : IApplicationRepository
{
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public async Task<IReadOnlyList<ApplicationEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            return await LoadAsync(cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task AddAsync(ApplicationEntry application, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var applications = await LoadAsync(cancellationToken);
            if (applications.Any(existing => PathsEqual(existing.ExecutablePath, application.ExecutablePath)))
            {
                return;
            }

            applications = [.. applications, application];
            var config = new ApplicationsConfig
            {
                Applications = applications
                    .Select(entry => new ApplicationConfig
                    {
                        Name = entry.Name,
                        Path = entry.ExecutablePath,
                        Icon = entry.IconPath,
                    })
                    .ToList(),
            };

            var configDirectory = Path.GetDirectoryName(Path.GetFullPath(configPath))!;
            Directory.CreateDirectory(configDirectory);
            var temporaryPath = Path.Combine(configDirectory, $"{Path.GetFileName(configPath)}.{Guid.NewGuid():N}.tmp");
            try
            {
                await using (var stream = File.Create(temporaryPath))
                {
                    await JsonSerializer.SerializeAsync(
                        stream,
                        config,
                        ApplicationsConfigJsonContext.Default.ApplicationsConfig,
                        cancellationToken);
                }

                File.Move(temporaryPath, configPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<IReadOnlyList<ApplicationEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var sourcePath = File.Exists(configPath)
            ? configPath
            : defaultConfigPath is not null && File.Exists(defaultConfigPath)
                ? defaultConfigPath
                : null;

        if (sourcePath is null)
        {
            return [];
        }

        ApplicationsConfig? config;
        await using (var stream = File.OpenRead(sourcePath))
        {
            config = await JsonSerializer.DeserializeAsync(
                stream,
                ApplicationsConfigJsonContext.Default.ApplicationsConfig,
                cancellationToken);
        }

        var configDirectory = Path.GetDirectoryName(Path.GetFullPath(sourcePath))!;
        var entries = config?.Applications ?? [];
        var applications = new List<ApplicationEntry>(entries.Count);

        for (var index = 0; index < entries.Count; index++)
        {
            var entry = entries[index];

            if (string.IsNullOrWhiteSpace(entry.Name) || string.IsNullOrWhiteSpace(entry.Path))
            {
                throw new InvalidDataException($"Application #{index + 1} in '{sourcePath}' must have a 'name' and a 'path'.");
            }

            applications.Add(new ApplicationEntry(entry.Name, entry.Path, ResolveIconPath(entry.Icon, configDirectory)));
        }

        return applications;
    }

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.GetFullPath(first),
            Path.GetFullPath(second),
            StringComparison.OrdinalIgnoreCase);

    private static string? ResolveIconPath(string? icon, string configDirectory)
        => string.IsNullOrWhiteSpace(icon)
            ? null
            : Path.GetFullPath(Path.Combine(configDirectory, icon.Trim()));
}
