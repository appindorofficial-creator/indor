using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
            return new PropertyEnrichmentResult
            {
                Success = false,
                ErrorMessage = "OpenAI property enrichment is not configured."
            };
        }

        var address = BuildAddressPrompt(propertyInfo);
        if (string.IsNullOrWhiteSpace(address))
        {
            return new PropertyEnrichmentResult
            {
                Success = false,
                ErrorMessage = "Insufficient address data for AI lookup."
            };
        }

        try
        {
            if (_enrichmentCache.TryGet(address, out var cached) && cached != null)
            {
                _logger.LogInformation("Using cached OpenAI enrichment for {Address}", address);
                return ApplyEnrichmentPayload(propertyInfo, cached.RawJson, cached.DataSource);
            }

            var researchPayload = await GetResearchPayloadAsync(address, propertyInfo);
            if (string.IsNullOrWhiteSpace(researchPayload))
            {
                return new PropertyEnrichmentResult
                {
                    Success = false,
                    ErrorMessage = "OpenAI research step did not return data."
                };
            }

            var finalJson = researchPayload;
            if (_options.UseTwoStepPipeline)
            {
                var organizedJson = await CallChatJsonAsync(
                    HouseFactOrganizationPrompt.SystemMessage,
                    HouseFactOrganizationPrompt.BuildUserPrompt(researchPayload),
                    temperature: _options.OrganizationTemperature,
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

            var dataSource = ReadDataSource(finalJson, _options.EnableWebSearch);
            var result = ApplyEnrichmentPayload(propertyInfo, finalJson, dataSource);

            if (result.Success)
            {
                _enrichmentCache.Set(address, new CachedPropertyEnrichment
                {
                    RawJson = finalJson,
                    DataSource = result.DataSource ?? "AI-estimated",
                    Success = result.Success
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI House Fact enrichment failed for {Address}", propertyInfo.FormattedAddress);
            return new PropertyEnrichmentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private PropertyEnrichmentResult ApplyEnrichmentPayload(
        PropertyInfoViewModel propertyInfo,
        string finalJson,
        string dataSource)
    {
        propertyInfo.PropertyDetails = new PropertyDetailsInfo();
        var applied = PropertyEnrichmentMapper.ApplyPayload(propertyInfo, finalJson);
        RegionalPropertyHints.Apply(propertyInfo);
        propertyInfo.AttomRawJson = finalJson;
        propertyInfo.DataSource = dataSource;
        var success = applied || HasOrganizedSections(finalJson) || HasLegacyHouseFact(finalJson);

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
        if (_options.EnableWebSearch)
        {
            var webResult = await CallWebResearchAsync(address, propertyInfo);
            if (!string.IsNullOrWhiteSpace(webResult))
            {
                return webResult;
            }

            _logger.LogWarning("Web search research failed for {Address}; falling back to chat completions.", address);
        }

        return await CallChatJsonAsync(
            HouseFactPrompt.SystemMessage,
            HouseFactPrompt.BuildUserPrompt(address),
            temperature: _options.ResearchTemperature,
            stepName: "research-chat");
    }

    private async Task<string?> CallWebResearchAsync(string address, PropertyInfoViewModel propertyInfo)
    {
        var location = BuildUserLocation(propertyInfo);
        var requestBody = new Dictionary<string, object?>
        {
            ["model"] = string.IsNullOrWhiteSpace(_options.ResearchModel) ? _options.Model : _options.ResearchModel,
            ["tool_choice"] = "required",
            ["input"] = HouseFactPrompt.BuildWebResearchPrompt(address),
            ["instructions"] = HouseFactPrompt.WebSearchSystemMessage,
            ["tools"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["type"] = "web_search",
                    ["user_location"] = location,
                    ["filters"] = new Dictionary<string, object?>
                    {
                        ["allowed_domains"] = PropertySearchDomains
                    }
                }
            },
            ["text"] = new Dictionary<string, object?>
            {
                ["format"] = new Dictionary<string, string> { ["type"] = "json_object" }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI web search failed ({StatusCode}): {Body}", (int)response.StatusCode, Truncate(responseBody, 500));
            return null;
        }

        var text = ExtractResponsesText(responseBody);
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("OpenAI web search returned empty text for {Address}", address);
            return null;
        }

        _logger.LogInformation("OpenAI web search succeeded for {Address} ({Length} chars)", address, text.Length);
        return text;
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

    private async Task<string?> CallChatJsonAsync(string systemMessage, string userMessage, double temperature, string stepName)
    {
        var requestBody = new
        {
            model = _options.Model,
            temperature,
            seed = 42,
            max_tokens = 16384,
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
            _logger.LogWarning("OpenAI {Step} step failed ({StatusCode})", stepName, (int)response.StatusCode);
            return null;
        }

        return JsonNode.Parse(responseBody)?["choices"]?.AsArray()?.FirstOrDefault()?["message"]?["content"]?.GetValue<string>();
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
