namespace IndorMvcApp.Services;

public class OpenAiPropertyOptions
{
    public const string SectionName = "OpenAiProperty";

    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4o";
    public string ResearchModel { get; set; } = "gpt-4o";
    public bool Enabled { get; set; } = true;
    public bool EnableWebSearch { get; set; } = true;
    public bool UseTwoStepPipeline { get; set; } = true;
    public bool EnableEnrichmentCache { get; set; } = true;
    public int EnrichmentCacheHours { get; set; } = 168;
    public double ResearchTemperature { get; set; } = 0;
    public double OrganizationTemperature { get; set; } = 0;
}
