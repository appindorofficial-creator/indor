using System.Text.Json;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

/// <summary>
/// Always loads OpenAiProperty from appsettings.json on disk so Azure deploy works without portal settings.
/// </summary>
public sealed class OpenAiPropertyOptionsPostConfigure(IWebHostEnvironment environment) : IPostConfigureOptions<OpenAiPropertyOptions>
{
    public void PostConfigure(string? name, OpenAiPropertyOptions options)
    {
        MergeFromFile(Path.Combine(environment.ContentRootPath, "appsettings.json"), options);

        var envFile = Path.Combine(environment.ContentRootPath, $"appsettings.{environment.EnvironmentName}.json");
        if (!string.Equals(envFile, Path.Combine(environment.ContentRootPath, "appsettings.json"), StringComparison.OrdinalIgnoreCase))
        {
            MergeFromFile(envFile, options);
        }
    }

    private static void MergeFromFile(string path, OpenAiPropertyOptions options)
    {
        if (!File.Exists(path))
        {
            return;
        }

        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);
        if (!doc.RootElement.TryGetProperty("OpenAiProperty", out var section) || section.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (section.TryGetProperty("BaseUrl", out var baseUrl) && baseUrl.ValueKind == JsonValueKind.String)
        {
            options.BaseUrl = baseUrl.GetString() ?? options.BaseUrl;
        }

        if (section.TryGetProperty("ApiKey", out var apiKey) && apiKey.ValueKind == JsonValueKind.String)
        {
            var key = apiKey.GetString();
            if (!string.IsNullOrWhiteSpace(key))
            {
                options.ApiKey = key;
            }
        }

        if (section.TryGetProperty("Model", out var model) && model.ValueKind == JsonValueKind.String)
        {
            options.Model = model.GetString() ?? options.Model;
        }

        if (section.TryGetProperty("ResearchModel", out var researchModel) && researchModel.ValueKind == JsonValueKind.String)
        {
            options.ResearchModel = researchModel.GetString() ?? options.ResearchModel;
        }

        if (section.TryGetProperty("SearchModel", out var searchModel) && searchModel.ValueKind == JsonValueKind.String)
        {
            options.SearchModel = searchModel.GetString() ?? options.SearchModel;
        }

        if (section.TryGetProperty("UseChatGptSearchModelForResearch", out var chatGptSearch) && chatGptSearch.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            options.UseChatGptSearchModelForResearch = chatGptSearch.GetBoolean();
        }

        if (section.TryGetProperty("WebSearchContextSize", out var contextSize) && contextSize.ValueKind == JsonValueKind.String)
        {
            options.WebSearchContextSize = contextSize.GetString() ?? options.WebSearchContextSize;
        }

        if (section.TryGetProperty("FastModel", out var fastModel) && fastModel.ValueKind == JsonValueKind.String)
        {
            options.FastModel = fastModel.GetString() ?? options.FastModel;
        }

        if (section.TryGetProperty("Enabled", out var enabled) && enabled.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            options.Enabled = enabled.GetBoolean();
        }

        if (section.TryGetProperty("EnableWebSearch", out var enableWebSearch) && enableWebSearch.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            options.EnableWebSearch = enableWebSearch.GetBoolean();
        }

        if (section.TryGetProperty("UseQuickEnrichmentFirst", out var quickFirst) && quickFirst.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            options.UseQuickEnrichmentFirst = quickFirst.GetBoolean();
        }

        if (section.TryGetProperty("DeferFullHouseFactResearch", out var deferFull) && deferFull.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            options.DeferFullHouseFactResearch = deferFull.GetBoolean();
        }
    }
}
