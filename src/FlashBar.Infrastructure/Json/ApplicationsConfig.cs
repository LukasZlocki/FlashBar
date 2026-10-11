using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlashBar.Infrastructure.Json;

internal sealed class ApplicationsConfig
{
    public List<ApplicationConfig>? Applications { get; set; }
}

internal sealed class ApplicationConfig
{
    public string? Name { get; set; }

    public string? Path { get; set; }

    public string? Icon { get; set; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(ApplicationsConfig))]
internal sealed partial class ApplicationsConfigJsonContext : JsonSerializerContext
{
}
