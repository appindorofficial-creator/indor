namespace IndorMvcApp.Services;

public class OpenAiPropertyOptions
{
    public const string SectionName = "OpenAiProperty";

    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4o-mini";
    public string ResearchModel { get; set; } = "gpt-4o";
    /// <summary>Chat Completions model with built-in web search (same capability ChatGPT uses for browsing).</summary>
    public string SearchModel { get; set; } = "gpt-4o-search-preview";
    /// <summary>Use SearchModel for all property research (ChatGPT parity). Avoids gpt-4o without live search.</summary>
    public bool UseChatGptSearchModelForResearch { get; set; } = true;
    /// <summary>Web search depth for SearchModel: low, medium, or high (high = closer to ChatGPT browsing).</summary>
    public string WebSearchContextSize { get; set; } = "medium";
    /// <summary>Faster model for organization, maintenance, and inspection chunk calls.</summary>
    public string FastModel { get; set; } = "gpt-4o-mini";
    public bool Enabled { get; set; } = true;
    public bool EnableWebSearch { get; set; } = true;
    /// <summary>Limit web search to property listing / assessor domains. Disable for broader (often faster) search.</summary>
    public bool RestrictWebSearchDomains { get; set; } = false;
    /// <summary>Second OpenAI call to reorganize research JSON. Off by default — one web-search call is faster and more reliable.</summary>
    public bool UseTwoStepPipeline { get; set; } = false;
    /// <summary>Skip the organization API call when research JSON already includes House Fact sections.</summary>
    public bool SkipOrganizationWhenStructured { get; set; } = true;
    /// <summary>Return address enrichment immediately and generate maintenance recommendations in the background.</summary>
    public bool DeferMaintenanceRecommendations { get; set; } = true;
    public bool EnableEnrichmentCache { get; set; } = true;
    public int EnrichmentCacheHours { get; set; } = 168;
    public double ResearchTemperature { get; set; } = 0;
    public double OrganizationTemperature { get; set; } = 0;
    public int EnrichmentMaxTokens { get; set; } = 8192;
    public int OrganizationMaxTokens { get; set; } = 4096;
    /// <summary>Seconds before web search falls back to chat-only research.</summary>
    public int WebSearchTimeoutSeconds { get; set; } = 120;
    /// <summary>Try a short Zillow-focused lookup before the full 13-section House Fact research.</summary>
    public bool UseQuickEnrichmentFirst { get; set; } = true;
    /// <summary>On first address save, return quick Zillow stats immediately and run full House Fact research in the background.</summary>
    public bool DeferFullHouseFactResearch { get; set; } = true;
    public int QuickEnrichmentMaxTokens { get; set; } = 2048;
    public int QuickWebSearchTimeoutSeconds { get; set; } = 75;
    public int MaintenanceMaxTokens { get; set; } = 4096;
    public int InspectionMaxTokens { get; set; } = 8192;
    public int InspectionParallelChunks { get; set; } = 3;
    public int InspectionPagesPerChunk { get; set; } = 15;
}
