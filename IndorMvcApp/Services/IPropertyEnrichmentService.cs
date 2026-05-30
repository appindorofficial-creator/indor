using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IPropertyEnrichmentService
{
    Task<PropertyEnrichmentResult> EnrichPropertyAsync(PropertyInfoViewModel propertyInfo);
}

public class PropertyEnrichmentResult
{
    public bool Success { get; set; }
    public string? RawJson { get; set; }
    public string? ErrorMessage { get; set; }
    public string DataSource { get; set; } = "Estimated";
    public long? ExternalPropertyId { get; set; }
}
