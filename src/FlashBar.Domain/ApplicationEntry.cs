namespace FlashBar.Domain;

public sealed class ApplicationEntry
{
    public ApplicationEntry(string name, string executablePath, string? iconPath = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Application name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new ArgumentException("Application path is required.", nameof(executablePath));
        }

        Name = name.Trim();
        ExecutablePath = executablePath.Trim();
        IconPath = string.IsNullOrWhiteSpace(iconPath) ? null : iconPath.Trim();
    }

    public string Name { get; }

    public string ExecutablePath { get; }

    public string? IconPath { get; }
}
