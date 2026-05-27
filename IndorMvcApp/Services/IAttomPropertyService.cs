using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IAttomPropertyService
{
    Task<AttomEnrichmentResult> EnrichPropertyAsync(PropertyInfoViewModel propertyInfo);
}

public class AttomEnrichmentResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long? AttomPropertyId { get; set; }
    public string? RawJson { get; set; }
}
