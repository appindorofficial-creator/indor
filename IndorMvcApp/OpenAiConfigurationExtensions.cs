using System.Text.Json;

namespace IndorMvcApp;

public static class OpenAiConfigurationExtensions
{
    /// <summary>
    /// Forces OpenAiProperty values from appsettings JSON files so Azure Application Settings are not required.
    /// </summary>
    public static void ApplyOpenAiFromAppsettingsJson(this ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        var basePath = Path.Combine(environment.ContentRootPath, "appsettings.json");
        MergeOpenAiSection(basePath, overrides);

        var envPath = Path.Combine(environment.ContentRootPath, $"appsettings.{environment.EnvironmentName}.json");
        if (!string.Equals(basePath, envPath, StringComparison.OrdinalIgnoreCase))
        {
            MergeOpenAiSection(envPath, overrides);
        }

        if (overrides.Count > 0)
        {
            configuration.AddInMemoryCollection(overrides);
        }
    }

    private static void MergeOpenAiSection(string path, Dictionary<string, string?> target)
    {
        if (!File.Exists(path))
        {
            return;
        }

        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);
        if (!doc.RootElement.TryGetProperty("OpenAiProperty", out var openAi) || openAi.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var prop in openAi.EnumerateObject())
        {
            target[$"OpenAiProperty:{prop.Name}"] = prop.Value.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => prop.Value.GetRawText(),
                JsonValueKind.String => prop.Value.GetString(),
                _ => prop.Value.GetRawText()
            };
        }
    }
}
