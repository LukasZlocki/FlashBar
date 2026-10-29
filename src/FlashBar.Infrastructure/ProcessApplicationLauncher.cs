using System.Diagnostics;
using FlashBar.Domain;
using FlashBar.Repository;

namespace FlashBar.Infrastructure;

public sealed class ProcessApplicationLauncher : IApplicationLauncher
{
    public Task LaunchAsync(ApplicationEntry application, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var executablePath = application.ExecutablePath;
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException($"Application file not found: {executablePath}", executablePath);
        }

        var startInfo = new ProcessStartInfo(executablePath)
        {
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(executablePath)!,
        };

        Process.Start(startInfo)?.Dispose();
        return Task.CompletedTask;
    }
}
