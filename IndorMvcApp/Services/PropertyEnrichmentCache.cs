using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace IndorMvcApp.Services;

public sealed class PropertyEnrichmentCache
{
    private const string CacheKeyPrefix = "property-enrichment:";
    private readonly IMemoryCache _cache;
    private readonly OpenAiPropertyOptions _options;

    public PropertyEnrichmentCache(IMemoryCache cache, Microsoft.Extensions.Options.IOptions<OpenAiPropertyOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public bool TryGet(string address, out CachedPropertyEnrichment? entry)
    {
        entry = null;
        if (!_options.EnableEnrichmentCache || _options.EnrichmentCacheHours <= 0)
        {
            return false;
        }

        return _cache.TryGetValue(BuildKey(address), out entry);
    }

    public void Set(string address, CachedPropertyEnrichment entry)
    {
        if (!_options.EnableEnrichmentCache || _options.EnrichmentCacheHours <= 0)
        {
            return;
        }

        _cache.Set(
            BuildKey(address),
            entry,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_options.EnrichmentCacheHours)
            });
    }

    private string BuildKey(string address)
    {
        var normalized = NormalizeAddress(address);
        return string.Join(
            ':',
            CacheKeyPrefix,
            normalized,
            _options.EnableWebSearch ? "web" : "chat",
            _options.UseTwoStepPipeline ? "2step" : "1step",
            _options.Model.Trim().ToLowerInvariant());
    }

    internal static string NormalizeAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return string.Empty;
        }

        var collapsed = Regex.Replace(address.Trim(), @"\s+", " ", RegexOptions.CultureInvariant);
        return collapsed.ToUpperInvariant();
    }
}

public sealed class CachedPropertyEnrichment
{
    public required string RawJson { get; init; }
    public required string DataSource { get; init; }
    public bool Success { get; init; }
}
