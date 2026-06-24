using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public class CompositePropertyEnrichmentService : IPropertyEnrichmentService
{
    private readonly OpenAiPropertyEnrichmentService _openAiService;
    private readonly IAttomPropertyService _attomService;
    private readonly OpenAiPropertyOptions _openAiOptions;
    private readonly AttomOptions _attomOptions;
    private readonly ILogger<CompositePropertyEnrichmentService> _logger;

    public CompositePropertyEnrichmentService(
        OpenAiPropertyEnrichmentService openAiService,
        IAttomPropertyService attomService,
        Microsoft.Extensions.Options.IOptions<OpenAiPropertyOptions> openAiOptions,
        Microsoft.Extensions.Options.IOptions<AttomOptions> attomOptions,
        ILogger<CompositePropertyEnrichmentService> logger)
    {
        _openAiService = openAiService;
        _attomService = attomService;
        _openAiOptions = openAiOptions.Value;
        _attomOptions = attomOptions.Value;
        _logger = logger;
    }

    public async Task<PropertyEnrichmentResult> EnrichPropertyAsync(PropertyInfoViewModel propertyInfo)
    {
        if (_openAiOptions.Enabled && !string.IsNullOrWhiteSpace(_openAiOptions.ApiKey))
        {
            var aiResult = await _openAiService.EnrichPropertyAsync(propertyInfo);
            if (aiResult.Success
                || !string.IsNullOrWhiteSpace(aiResult.RawJson)
                || PropertyEnrichmentMapper.HasMeaningfulDetails(propertyInfo.PropertyDetails ?? new PropertyDetailsInfo()))
            {
                if (!aiResult.Success)
                {
                    _logger.LogInformation(
                        "OpenAI enrichment partial for {Address}: {Reason}",
                        propertyInfo.FormattedAddress,
                        aiResult.ErrorMessage ?? "Unknown");
                    aiResult.Success = true;
                    aiResult.ErrorMessage = null;
                }

                return aiResult;
            }

            _logger.LogInformation(
                "OpenAI enrichment did not succeed for {Address}: {Reason}",
                propertyInfo.FormattedAddress,
                aiResult.ErrorMessage ?? "Unknown");
        }

        if (_attomOptions.Enabled && !string.IsNullOrWhiteSpace(_attomOptions.ApiKey))
        {
            var attomResult = await _attomService.EnrichPropertyAsync(propertyInfo);
            return new PropertyEnrichmentResult
            {
                Success = attomResult.Success,
                RawJson = attomResult.RawJson,
                ErrorMessage = attomResult.ErrorMessage,
                DataSource = attomResult.Success ? "ATTOM" : "Estimated",
                ExternalPropertyId = attomResult.AttomPropertyId
            };
        }

        return new PropertyEnrichmentResult
        {
            Success = false,
            ErrorMessage = "No property enrichment provider is configured.",
            DataSource = "Estimated"
        };
    }
}
