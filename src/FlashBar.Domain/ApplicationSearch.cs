namespace FlashBar.Domain;

public static class ApplicationSearch
{
    public static IReadOnlyList<ApplicationEntry> Match(IEnumerable<ApplicationEntry> applications, string? query)
    {
        var term = query?.Trim() ?? string.Empty;

        if (term.Length == 0)
        {
            return applications.ToList();
        }

        return applications
            .Where(application => application.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(application => application.Name.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ToList();
    }
}
