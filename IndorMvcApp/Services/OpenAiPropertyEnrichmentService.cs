using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public class OpenAiPropertyEnrichmentService : IPropertyEnrichmentService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiPropertyOptions _options;
    private readonly PropertyEnrichmentCache _enrichmentCache;
    private readonly ILogger<OpenAiPropertyEnrichmentService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly string[] PropertySearchDomains =
    [
        "zillow.com",
        "redfin.com",
        "realtor.com",
        "homes.com",
        "mecknc.gov",
        "tax.mecknc.gov",
        "minthill.com"
    ];

    public OpenAiPropertyEnrichmentService(
        HttpClient httpClient,
        IOptions<OpenAiPropertyOptions> options,
        PropertyEnrichmentCache enrichmentCache,
        ILogger<OpenAiPropertyEnrichmentService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _enrichmentCache = enrichmentCache;
        _logger = logger;
    }

    public async Task<PropertyEnrichmentResult> EnrichPropertyAsync(PropertyInfoViewModel propertyInfo)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogError("OpenAI property enrichment is disabled or ApiKey is missing.");
            return new PropertyEnrichmentResult
            {
                Success = false,
                ErrorMessage = "OpenAI property enrichment is not configured."
            };
        }

        _logger.LogInformation(
            "Starting OpenAI enrichment with ResearchModel={ResearchModel}, SearchModel={SearchModel}",
            ResolveResearchModel(),
            ResolveSearchModel());

        var address = BuildAddressPrompt(propertyInfo);
        if (string.IsNullOrWhiteSpace(address))
        {
            return new PropertyEnrichmentResult
            {
                Success = false,
                ErrorMessage = "Insufficient address data for AI lookup."
            };
        }

        string? researchPayload = null;

        try
        {
            if (_enrichmentCache.TryGet(address, out var cached) && cached != null)
            {
                _logger.LogInformation("Using cached OpenAI enrichment for {Address}", address);
                return ApplyEnrichmentPayload(propertyInfo, cached.RawJson, cached.DataSource);
            }

            researchPayload = await GetResearchPayloadAsync(address, propertyInfo);
            if (string.IsNullOrWhiteSpace(researchPayload))
            {
                return new PropertyEnrichmentResult
                {
                    Success = false,
                    ErrorMessage = "OpenAI research step did not return data."
                };
            }

            researchPayload = await MaybeCorrectLivingAreaAsync(address, researchPayload);

            propertyInfo.PropertyDetails ??= new PropertyDetailsInfo();
            PropertyEnrichmentMapper.ApplyPayload(propertyInfo, researchPayload);

            var finalJson = researchPayload;
            if (_options.UseTwoStepPipeline
                && !(_options.SkipOrganizationWhenStructured && HasUsableResearchStructure(researchPayload)))
            {
                try
                {
                    var organizedJson = await CallChatJsonAsync(
                        HouseFactOrganizationPrompt.SystemMessage,
                        HouseFactOrganizationPrompt.BuildUserPrompt(researchPayload),
                        temperature: _options.OrganizationTemperature,
                        model: ResolveFastModel(),
                        maxTokens: _options.OrganizationMaxTokens,
                        stepName: "organization");

                    if (!string.IsNullOrWhiteSpace(organizedJson))
                    {
                        finalJson = MergeResearchIntoFinal(organizedJson, researchPayload);
                    }
                    else
                    {
                        _logger.LogWarning("Organization step failed for {Address}; using research payload only.", address);
                    }
                }
                catch (Exception orgEx) when (orgEx is TaskCanceledException or OperationCanceledException or TimeoutException)
                {
                    _logger.LogWarning(orgEx, "Organization timed out for {Address}; using research payload only.", address);
                }
            }

            var dataSource = ReadDataSource(finalJson, _options.EnableWebSearch);
            var result = FinalizeEnrichment(propertyInfo, finalJson, dataSource, address);
            if (!result.Success)
            {
                _logger.LogWarning(
                    "Organized enrichment incomplete for {Address}; falling back to research payload.",
                    address);
                result = FinalizeEnrichment(
                    propertyInfo,
                    researchPayload,
                    ReadDataSource(researchPayload, _options.EnableWebSearch),
                    address);
            }

            return result;
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException or TimeoutException)
        {
            _logger.LogWarning(ex, "OpenAI enrichment timed out for {Address}", propertyInfo.FormattedAddress);
            if (!string.IsNullOrWhiteSpace(researchPayload))
            {
                return FinalizeEnrichment(
                    propertyInfo,
                    researchPayload,
                    ReadDataSource(researchPayload, _options.EnableWebSearch),
                    address);
            }

            return new PropertyEnrichmentResult
            {
                Success = false,
                ErrorMessage = "OpenAI took too long researching this address. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI House Fact enrichment failed for {Address}", propertyInfo.FormattedAddress);
            if (!string.IsNullOrWhiteSpace(researchPayload))
            {
                return FinalizeEnrichment(
                    propertyInfo,
                    researchPayload,
                    "AI — research only",
                    address);
            }

            return new PropertyEnrichmentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private PropertyEnrichmentResult FinalizeEnrichment(
        PropertyInfoViewModel propertyInfo,
        string finalJson,
        string dataSource,
        string address)
    {
        var result = ApplyEnrichmentPayload(propertyInfo, finalJson, dataSource);
        if (result.Success)
        {
            _enrichmentCache.Set(address, new CachedPropertyEnrichment
            {
                RawJson = finalJson,
                DataSource = result.DataSource ?? "AI-estimated",
                Success = true
            });
        }

        return result;
    }

    private PropertyEnrichmentResult ApplyEnrichmentPayload(
        PropertyInfoViewModel propertyInfo,
        string finalJson,
        string dataSource)
    {
        propertyInfo.PropertyDetails ??= new PropertyDetailsInfo();
        var applied = PropertyEnrichmentMapper.ApplyPayload(propertyInfo, finalJson);
        RegionalPropertyHints.Apply(propertyInfo);
        propertyInfo.AttomRawJson = finalJson;
        propertyInfo.DataSource = dataSource;
        var success = applied
            || HasOrganizedSections(finalJson)
            || HasLegacyHouseFact(finalJson)
            || HasAnyPropertyJson(finalJson)
            || PropertyEnrichmentMapper.HasMeaningfulDetails(propertyInfo.PropertyDetails);

        return new PropertyEnrichmentResult
        {
            Success = success,
            RawJson = finalJson,
            DataSource = propertyInfo.DataSource,
            ErrorMessage = success
                ? null
                : "OpenAI JSON did not include usable property details."
        };
    }

    private async Task<string?> GetResearchPayloadAsync(string address, PropertyInfoViewModel propertyInfo)
    {
        var runFullResearch = propertyInfo.RequestFullHouseFactResearch || !_options.DeferFullHouseFactResearch;

        string? quickPayload = null;
        if (_options.UseQuickEnrichmentFirst)
        {
            quickPayload = await TryQuickResearchAsync(address, propertyInfo);
        }

        quickPayload ??= GetExistingQuickSeed(propertyInfo);

        var quickIsUsable = !string.IsNullOrWhiteSpace(quickPayload) && HasUsableQuickPayload(quickPayload);

        if (!runFullResearch)
        {
            if (!string.IsNullOrWhiteSpace(quickPayload) && HasAnyPropertyJson(quickPayload))
            {
                _logger.LogInformation("Quick property enrichment succeeded for {Address}", address);
                return NormalizeJsonPayload(quickPayload);
            }

            _logger.LogWarning("Quick enrichment unavailable for {Address}; trying search + web fallbacks.", address);
            return await RunPropertySearchFallbacksAsync(address, propertyInfo, HouseFactQuickPrompt.SystemMessage);
        }

        if (quickIsUsable)
        {
            _logger.LogInformation(
                "Quick enrichment available for {Address}; running full House Fact research.",
                address);
        }
        else
        {
            _logger.LogWarning("Quick enrichment unavailable for {Address}; trying full research.", address);
        }

        string? fullPayload = null;
        if (_options.UseChatGptSearchModelForResearch)
        {
            fullPayload = await CallChatSearchJsonAsync(
                HouseFactPrompt.WebSearchSystemMessage,
                HouseFactPrompt.BuildWebResearchPrompt(address),
                stepName: "full-chatgpt-search",
                maxTokens: Math.Min(_options.EnrichmentMaxTokens, 4096),
                timeoutSeconds: _options.WebSearchTimeoutSeconds);
        }

        if (string.IsNullOrWhiteSpace(fullPayload) && _options.EnableWebSearch)
        {
            try
            {
                fullPayload = await CallWebResearchAsync(
                    address,
                    propertyInfo,
                    HouseFactPrompt.BuildWebResearchPrompt(address),
                    HouseFactPrompt.WebSearchSystemMessage,
                    _options.EnrichmentMaxTokens,
                    _options.WebSearchTimeoutSeconds,
                    stepName: "full-web");
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException or TimeoutException)
            {
                _logger.LogWarning(ex, "Full web search timed out for {Address}.", address);
            }
        }

        if (string.IsNullOrWhiteSpace(fullPayload))
        {
            fullPayload = await CallChatJsonAsync(
                HouseFactPrompt.SystemMessage,
                HouseFactPrompt.BuildUserPrompt(address),
                temperature: _options.ResearchTemperature,
                model: ResolveResearchModel(),
                maxTokens: _options.EnrichmentMaxTokens,
                stepName: "research-chat");
        }

        if (!string.IsNullOrWhiteSpace(fullPayload) && quickIsUsable)
        {
            return MergeEnrichmentPayloads(quickPayload!, fullPayload);
        }

        if (!string.IsNullOrWhiteSpace(fullPayload))
        {
            return fullPayload;
        }

        if (quickIsUsable)
        {
            return quickPayload;
        }

        return await RunPropertySearchFallbacksAsync(address, propertyInfo, HouseFactQuickPrompt.SystemMessage);
    }

    private async Task<string?> RunPropertySearchFallbacksAsync(
        string address,
        PropertyInfoViewModel propertyInfo,
        string systemMessage)
    {
        var search = await CallChatSearchJsonAsync(
            systemMessage,
            ChatGptPropertyPrompt.BuildSearchUserPrompt(address),
            stepName: "property-search-fallback",
            maxTokens: _options.QuickEnrichmentMaxTokens,
            timeoutSeconds: _options.QuickWebSearchTimeoutSeconds);
        if (HasAnyPropertyJson(search))
        {
            return NormalizeJsonPayload(search!);
        }

        if (_options.EnableWebSearch)
        {
            try
            {
                var web = await CallWebResearchAsync(
                    address,
                    propertyInfo,
                    HouseFactQuickPrompt.BuildUserPrompt(address),
                    systemMessage,
                    _options.QuickEnrichmentMaxTokens,
                    _options.QuickWebSearchTimeoutSeconds,
                    stepName: "property-web-fallback");
                if (HasAnyPropertyJson(web))
                {
                    return NormalizeJsonPayload(web!);
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException or TimeoutException)
            {
                _logger.LogWarning(ex, "Web research fallback timed out for {Address}.", address);
            }
        }

        return await CallChatJsonAsync(
            systemMessage,
            HouseFactQuickPrompt.BuildUserPrompt(address),
            temperature: _options.ResearchTemperature,
            model: ResolveResearchModel(),
            maxTokens: _options.QuickEnrichmentMaxTokens,
            stepName: "property-chat-fallback");
    }

    private static string? GetExistingQuickSeed(PropertyInfoViewModel propertyInfo)
    {
        var existing = propertyInfo.AttomRawJson;
        return !string.IsNullOrWhiteSpace(existing) && HasUsableQuickPayload(existing)
            ? existing
            : null;
    }

    internal static bool IsQuickOnlyPayload(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return false;
        }

        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root == null)
            {
                return false;
            }

            if (root["propertyIdentity"] != null || root["sections"]?.AsArray()?.Count > 0)
            {
                return false;
            }

            return root["propertyDetails"] != null
                || root["basicPropertyFacts"] != null
                || root["listingMarketData"] != null;
        }
        catch
        {
            return false;
        }
    }

    private static string MergeEnrichmentPayloads(string quickJson, string fullJson)
    {
        try
        {
            var fullRoot = JsonNode.Parse(fullJson)?.AsObject();
            var quickRoot = JsonNode.Parse(quickJson)?.AsObject();
            if (fullRoot == null)
            {
                return quickJson;
            }

            if (quickRoot == null)
            {
                return fullJson;
            }

            var fullDetails = fullRoot["propertyDetails"] as JsonObject;
            var quickDetails = quickRoot["propertyDetails"] as JsonObject;
            if (quickDetails != null)
            {
                fullDetails ??= new JsonObject();
                OverlayPropertyDetails(fullDetails, quickDetails);
                fullRoot["propertyDetails"] = fullDetails;
            }

            foreach (var key in new[] { "formattedAddress", "confidence", "sources", "listingMarketData", "basicPropertyFacts" })
            {
                if (fullRoot[key] == null && quickRoot[key] != null)
                {
                    fullRoot[key] = quickRoot[key]?.DeepClone();
                }
            }

            return fullRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            return fullJson;
        }
    }

    private static void OverlayPropertyDetails(JsonObject target, JsonObject quickDetails)
    {
        foreach (var key in new[]
                 {
                     "livingArea", "livingAreaSource", "livingAreaSourceName",
                     "yearBuilt", "bedrooms", "bedroomsSource",
                     "bathrooms", "bathroomsSource",
                     "estimatedValue", "estimatedValueSource",
                     "lotSizeSqFt", "lotSizeAcres", "propertyType", "countyName"
                 })
        {
            if (!ShouldOverlayPropertyDetail(target[key], quickDetails[key], key))
            {
                continue;
            }

            target[key] = quickDetails[key]?.DeepClone();
        }
    }

    private static bool ShouldOverlayPropertyDetail(JsonNode? existing, JsonNode? incoming, string key)
    {
        if (incoming == null)
        {
            return false;
        }

        if (existing == null)
        {
            return true;
        }

        if (key.Contains("livingArea", StringComparison.OrdinalIgnoreCase)
            && key.Equals("livingArea", StringComparison.OrdinalIgnoreCase))
        {
            var existingArea = ReadOverlayInt(existing);
            var incomingArea = ReadOverlayInt(incoming);
            return incomingArea is > 0 && (existingArea is not > 0 || incomingArea > existingArea);
        }

        if (key.Equals("yearBuilt", StringComparison.OrdinalIgnoreCase))
        {
            var existingYear = ReadOverlayInt(existing);
            var incomingYear = ReadOverlayInt(incoming);
            return incomingYear is > 1800 && existingYear is not > 1800;
        }

        if (key.Contains("Source", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(existing.GetValue<string>());
        }

        return IsEmptyPropertyDetail(existing);
    }

    private static bool IsEmptyPropertyDetail(JsonNode? node)
    {
        if (node == null)
        {
            return true;
        }

        return node.GetValueKind() switch
        {
            JsonValueKind.Null => true,
            JsonValueKind.String => string.IsNullOrWhiteSpace(node.GetValue<string>()),
            JsonValueKind.Number => node.GetValue<decimal>() <= 0,
            _ => false
        };
    }

    private static int? ReadOverlayInt(JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
            if (node.GetValueKind() == JsonValueKind.Number)
            {
                return node.GetValue<int>();
            }
        }
        catch
        {
            // fall through
        }

        return int.TryParse(node.GetValue<string>(), out var parsed) ? parsed : null;
    }

    private async Task<string> MaybeCorrectLivingAreaAsync(string address, string payload)
    {
        if (!PropertyEnrichmentMapper.NeedsLivingAreaCorrection(payload))
        {
            return payload;
        }

        _logger.LogInformation("Living area looks incorrect for {Address}; running Zillow header sq ft lookup.", address);

        var correction = await CallChatSearchJsonAsync(
            LivingAreaHeaderPrompt.SystemMessage,
            LivingAreaHeaderPrompt.BuildUserPrompt(address),
            stepName: "living-area-header");
        if (string.IsNullOrWhiteSpace(correction))
        {
            return payload;
        }

        var merged = PropertyEnrichmentMapper.MergeLivingAreaCorrection(payload, correction);
        if (!string.Equals(merged, payload, StringComparison.Ordinal))
        {
            _logger.LogInformation("Living area corrected for {Address} after Zillow header lookup.", address);
        }

        return merged;
    }

    private async Task<string?> TryQuickResearchAsync(string address, PropertyInfoViewModel propertyInfo)
    {
        var chatSearch = await CallChatSearchJsonAsync(
            HouseFactQuickPrompt.SystemMessage,
            ChatGptPropertyPrompt.BuildSearchUserPrompt(address),
            stepName: "quick-chatgpt-search",
            maxTokens: _options.QuickEnrichmentMaxTokens,
            timeoutSeconds: _options.QuickWebSearchTimeoutSeconds);
        if (HasAnyPropertyJson(chatSearch))
        {
            return NormalizeJsonPayload(chatSearch!);
        }

        if (_options.EnableWebSearch)
        {
            try
            {
                var webResult = await CallWebResearchAsync(
                    address,
                    propertyInfo,
                    HouseFactQuickPrompt.BuildUserPrompt(address),
                    HouseFactQuickPrompt.SystemMessage,
                    _options.QuickEnrichmentMaxTokens,
                    _options.QuickWebSearchTimeoutSeconds,
                    stepName: "quick-web");
                if (HasAnyPropertyJson(webResult))
                {
                    return NormalizeJsonPayload(webResult!);
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException or TimeoutException)
            {
                _logger.LogWarning(ex, "Quick web search timed out for {Address}.", address);
            }
        }

        return null;
    }

    private static bool HasAnyPropertyJson(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return false;
        }

        try
        {
            var normalized = NormalizeJsonPayload(rawJson);
            if (HasUsableQuickPayload(normalized))
            {
                return true;
            }

            var root = JsonNode.Parse(normalized);
            if (root == null)
            {
                return false;
            }

            if (root["propertyIdentity"] != null || root["sections"]?.AsArray()?.Count > 0)
            {
                return true;
            }

            if (root["propertyDetails"] is JsonObject details && details.Count > 0)
            {
                foreach (var kv in details)
                {
                    if (kv.Value == null) continue;
                    if (kv.Value.GetValueKind() is JsonValueKind.Number && kv.Value.GetValue<decimal>() > 0)
                    {
                        return true;
                    }

                    if (kv.Value.GetValueKind() == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(kv.Value.GetValue<string>()))
                    {
                        return true;
                    }
                }
            }

            return root["basicPropertyFacts"] != null || root["listingMarketData"] != null;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeJsonPayload(string rawJson)
    {
        var extracted = ExtractJsonPayload(rawJson);
        return string.IsNullOrWhiteSpace(extracted) ? rawJson.Trim() : extracted;
    }

    private static string? ExtractJsonPayload(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var trimmed = text.Trim();
        if (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
        {
            return trimmed;
        }

        var fenced = Regex.Match(trimmed, @"```(?:json)?\s*(\{.*\})\s*```", RegexOptions.Singleline);
        if (fenced.Success)
        {
            return fenced.Groups[1].Value.Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        return trimmed;
    }

    private static bool HasUsableQuickPayload(string rawJson)
    {
        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root == null) return false;

            var details = root["propertyDetails"];
            if (details != null)
            {
                var hasLiving = ReadQuickInt(details["livingArea"]) is > 0;
                var hasYear = ReadQuickInt(details["yearBuilt"]) is > 1800;
                var hasBeds = ReadQuickInt(details["bedrooms"]) is > 0;
                var hasBaths = ReadQuickDecimal(details["bathrooms"]) is > 0;
                var hasValue = ReadQuickDecimal(details["estimatedValue"]) is > 0;
                if (hasLiving || hasYear || (hasBeds && hasBaths) || (hasBeds && hasValue) || (hasBaths && hasValue))
                {
                    return true;
                }
            }

            return root["basicPropertyFacts"] != null && root["sources"]?.AsArray()?.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static int? ReadQuickInt(JsonNode? node)
    {
        if (node == null) return null;
        try
        {
            if (node.GetValueKind() == JsonValueKind.Number) return node.GetValue<int>();
        }
        catch
        {
            // fall through
        }

        return int.TryParse(node.GetValue<string>(), out var parsed) ? parsed : null;
    }

    private static decimal? ReadQuickDecimal(JsonNode? node)
    {
        if (node == null) return null;
        try
        {
            if (node.GetValueKind() == JsonValueKind.Number) return node.GetValue<decimal>();
        }
        catch
        {
            // fall through
        }

        return decimal.TryParse(node.GetValue<string>(), out var parsed) ? parsed : null;
    }

    private async Task<string?> CallWebResearchAsync(
        string address,
        PropertyInfoViewModel propertyInfo,
        string input,
        string instructions,
        int maxOutputTokens,
        int timeoutSeconds,
        string stepName)
    {
        var location = BuildUserLocation(propertyInfo);
        var webSearchTool = new Dictionary<string, object?> { ["type"] = "web_search", ["user_location"] = location };
        if (_options.RestrictWebSearchDomains)
        {
            webSearchTool["filters"] = new Dictionary<string, object?>
            {
                ["allowed_domains"] = PropertySearchDomains
            };
        }

        var requestBody = new Dictionary<string, object?>
        {
            ["model"] = ResolveResearchModel(),
            ["tool_choice"] = "required",
            ["input"] = input,
            ["instructions"] = instructions,
            ["tools"] = new object[] { webSearchTool },
            ["max_output_tokens"] = Math.Clamp(maxOutputTokens, 512, 16384)
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json");

        using var timeoutCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 20, 180)));
        using var response = await _httpClient.SendAsync(request, timeoutCts.Token);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenAI {Step} failed ({StatusCode}) for model {Model}: {Body}",
                stepName,
                (int)response.StatusCode,
                ResolveResearchModel(),
                Truncate(responseBody, 500));
            return null;
        }

        var text = ExtractResponsesText(responseBody);
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("OpenAI {Step} returned empty text for {Address}", stepName, address);
            return null;
        }

        _logger.LogInformation("OpenAI {Step} succeeded for {Address} ({Length} chars)", stepName, address, text.Length);
        return NormalizeJsonPayload(text);
    }

    private static Dictionary<string, object?> BuildUserLocation(PropertyInfoViewModel info)
    {
        var location = new Dictionary<string, object?> { ["type"] = "approximate" };

        if (!string.IsNullOrWhiteSpace(info.Country))
        {
            location["country"] = info.Country.Length == 2 ? info.Country.ToUpperInvariant() : "US";
        }
        else
        {
            location["country"] = "US";
        }

        if (!string.IsNullOrWhiteSpace(info.City))
        {
            location["city"] = info.City.Trim();
        }

        if (!string.IsNullOrWhiteSpace(info.State))
        {
            location["region"] = info.State.Trim();
        }

        return location;
    }

    private static string? ExtractResponsesText(string responseBody)
    {
        try
        {
            var root = JsonNode.Parse(responseBody);
            if (root == null) return null;

            var outputText = root["output_text"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(outputText))
            {
                return outputText;
            }

            if (root["output"] is not JsonArray output) return null;

            foreach (var item in output)
            {
                if (item?["type"]?.GetValue<string>() != "message") continue;
                if (item["content"] is not JsonArray content) continue;

                foreach (var part in content)
                {
                    var text = part?["text"]?.GetValue<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }
        }
        catch
        {
            // fall through
        }

        return null;
    }

    private async Task<string?> CallChatJsonAsync(
        string systemMessage,
        string userMessage,
        double temperature,
        string model,
        int maxTokens,
        string stepName)
    {
        var requestBody = new
        {
            model,
            temperature,
            seed = 42,
            max_tokens = maxTokens,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = systemMessage },
                new { role = "user", content = userMessage }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenAI {Step} step failed ({StatusCode}): {Body}",
                stepName,
                (int)response.StatusCode,
                Truncate(responseBody, 500));
            return null;
        }

        return JsonNode.Parse(responseBody)?["choices"]?.AsArray()?.FirstOrDefault()?["message"]?["content"]?.GetValue<string>();
    }

    private Task<string?> CallChatSearchJsonAsync(string address, string stepName) =>
        CallChatSearchJsonAsync(
            HouseFactQuickPrompt.SystemMessage,
            HouseFactQuickPrompt.BuildUserPrompt(address),
            stepName);

    private Task<string?> CallChatSearchJsonAsync(
        string systemMessage,
        string userMessage,
        string stepName,
        int? maxTokens = null,
        int? timeoutSeconds = null) =>
        CallChatSearchJsonCoreAsync(systemMessage, userMessage, stepName, maxTokens, timeoutSeconds);

    private async Task<string?> CallChatSearchJsonCoreAsync(
        string systemMessage,
        string userMessage,
        string stepName,
        int? maxTokens,
        int? timeoutSeconds)
    {
        var tokens = maxTokens ?? _options.QuickEnrichmentMaxTokens;
        var timeout = timeoutSeconds ?? _options.QuickWebSearchTimeoutSeconds;
        var contextSize = string.IsNullOrWhiteSpace(_options.WebSearchContextSize)
            ? "medium"
            : _options.WebSearchContextSize.Trim().ToLowerInvariant();

        var content = await PostChatSearchRequestAsync(
            systemMessage,
            userMessage,
            stepName,
            tokens,
            timeout,
            contextSize,
            useJsonResponseFormat: false);

        if (string.IsNullOrWhiteSpace(content) && !IsSearchPreviewModel())
        {
            content = await PostChatSearchRequestAsync(
                systemMessage,
                userMessage,
                stepName + "-jsonmode",
                tokens,
                timeout,
                contextSize,
                useJsonResponseFormat: true);
        }

        return string.IsNullOrWhiteSpace(content) ? null : NormalizeJsonPayload(content);
    }

    private async Task<string?> PostChatSearchRequestAsync(
        string systemMessage,
        string userMessage,
        string stepName,
        int tokens,
        int timeout,
        string contextSize,
        bool useJsonResponseFormat)
    {
        var requestBody = new Dictionary<string, object?>
        {
            ["model"] = ResolveSearchModel(),
            ["web_search_options"] = new Dictionary<string, object?> { ["search_context_size"] = contextSize },
            ["max_tokens"] = tokens,
            ["messages"] = new object[]
            {
                new Dictionary<string, string> { ["role"] = "system", ["content"] = systemMessage },
                new Dictionary<string, string> { ["role"] = "user", ["content"] = userMessage }
            }
        };

        // gpt-4o-search-preview rejects temperature and often rejects response_format.
        if (useJsonResponseFormat && !IsSearchPreviewModel())
        {
            requestBody["response_format"] = new Dictionary<string, string> { ["type"] = "json_object" };
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json");

        using var timeoutCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(Math.Clamp(timeout, 20, 300)));
        using var response = await _httpClient.SendAsync(request, timeoutCts.Token);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "OpenAI {Step} failed ({StatusCode}) for model {Model}: {Body}",
                stepName,
                (int)response.StatusCode,
                ResolveSearchModel(),
                Truncate(responseBody, 500));
            return null;
        }

        var content = JsonNode.Parse(responseBody)?["choices"]?.AsArray()?.FirstOrDefault()?["message"]?["content"]?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(content))
        {
            _logger.LogInformation(
                "OpenAI {Step} succeeded ({Length} chars, model={Model}, jsonMode={JsonMode})",
                stepName,
                content.Length,
                ResolveSearchModel(),
                useJsonResponseFormat);
        }

        return content;
    }

    private string ResolveSearchModel()
    {
        if (!string.IsNullOrWhiteSpace(_options.SearchModel))
        {
            return _options.SearchModel.Trim();
        }

        return "gpt-4o-search-preview";
    }

    private bool IsSearchPreviewModel()
    {
        var model = ResolveSearchModel();
        return model.Contains("search-preview", StringComparison.OrdinalIgnoreCase)
            || model.Contains("search_preview", StringComparison.OrdinalIgnoreCase);
    }

    private static string MergeResearchIntoFinal(string organizedJson, string researchPayload)
    {
        try
        {
            var organized = JsonNode.Parse(organizedJson)?.AsObject();
            if (organized == null) return organizedJson;

            try
            {
                organized["_researchInput"] = JsonNode.Parse(researchPayload);
            }
            catch
            {
                organized["_researchInputText"] = researchPayload;
            }

            return organized.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            return organizedJson;
        }
    }

    private string ResolveFastModel() =>
        string.IsNullOrWhiteSpace(_options.FastModel) ? _options.Model : _options.FastModel.Trim();

    private string ResolveResearchModel()
    {
        if (!string.IsNullOrWhiteSpace(_options.ResearchModel))
        {
            return _options.ResearchModel.Trim();
        }

        return ResolveFastModel();
    }

    private string ResolveChatResearchModel()
    {
        if (_options.UseChatGptSearchModelForResearch)
        {
            return ResolveSearchModel();
        }

        var research = ResolveResearchModel();
        return research.Contains("search", StringComparison.OrdinalIgnoreCase)
            ? ResolveFastModel()
            : research;
    }

    private static bool HasUsableResearchStructure(string rawJson)
    {
        if (HasOrganizedSections(rawJson) || HasLegacyHouseFact(rawJson))
        {
            return true;
        }

        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root?["propertyDetails"] is JsonObject details)
            {
                foreach (var kv in details)
                {
                    if (kv.Value == null) continue;
                    if (kv.Value.GetValueKind() == JsonValueKind.Number)
                    {
                        return true;
                    }

                    if (kv.Value.GetValueKind() == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(kv.Value.GetValue<string>()))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            // fall through
        }

        return false;
    }

    private static bool HasOrganizedSections(string rawJson)
    {
        try
        {
            var sections = JsonNode.Parse(rawJson)?["sections"]?.AsArray();
            return sections != null && sections.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static bool HasLegacyHouseFact(string rawJson)
    {
        try
        {
            var root = JsonNode.Parse(rawJson);
            return root?["propertyIdentity"] != null;
        }
        catch
        {
            return false;
        }
    }

    private static string ReadDataSource(string rawJson, bool usedWebSearch)
    {
        try
        {
            var root = JsonNode.Parse(rawJson);
            var confidence = root?["confidence"]?.GetValue<string>()
                ?? root?["dataConfidence"]?.GetValue<string>();

            if (usedWebSearch)
            {
                return string.IsNullOrWhiteSpace(confidence)
                    ? "AI — web search"
                    : $"AI — web search ({confidence})";
            }

            if (string.IsNullOrWhiteSpace(confidence))
            {
                return "AI-estimated";
            }

            return confidence.Contains("verification", StringComparison.OrdinalIgnoreCase)
                ? "AI — needs verification"
                : $"AI — {confidence}";
        }
        catch
        {
            return usedWebSearch ? "AI — web search" : "AI-estimated";
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "...";

    internal static string BuildAddressPrompt(PropertyInfoViewModel info)
    {
        if (!string.IsNullOrWhiteSpace(info.FormattedAddress))
        {
            return info.FormattedAddress.Trim();
        }

        var parts = new[]
        {
            string.Join(" ", new[] { info.HouseNumber, info.Street }.Where(x => !string.IsNullOrWhiteSpace(x))),
            info.City,
            info.State,
            info.PostalCode,
            info.Country
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        return string.Join(", ", parts);
    }
}
