using System.Net.Http.Headers;

using System.Text;

using System.Text.Json;

using System.Text.Json.Nodes;

using IndorMvcApp.ViewModels;

using Microsoft.Extensions.Options;



namespace IndorMvcApp.Services;



public class OpenAiMaintenanceRecommendationService(

    HttpClient httpClient,

    IOptions<OpenAiPropertyOptions> options,

    ILogger<OpenAiMaintenanceRecommendationService> logger) : IOpenAiMaintenanceRecommendationService

{

    private static readonly JsonSerializerOptions JsonOptions = new()

    {

        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        PropertyNameCaseInsensitive = true

    };



    public async Task<PropertyMaintenancePlanViewModel> GenerateAsync(

        PropertyInfoViewModel propertyInfo,

        CancellationToken cancellationToken = default)

    {

        var openAi = options.Value;

        if (!openAi.Enabled || string.IsNullOrWhiteSpace(openAi.ApiKey))

        {

            logger.LogWarning("Maintenance AI skipped — OpenAI not configured for {Address}", propertyInfo.FormattedAddress);

            return Unavailable("AI maintenance suggestions are not configured. Add your OpenAI API key in application settings.");

        }



        try

        {

            var userPrompt = MaintenanceRecommendationPrompt.BuildUserPrompt(propertyInfo);

            var (json, apiError) = await CallChatJsonAsync(

                MaintenanceRecommendationPrompt.SystemMessage,

                userPrompt,

                cancellationToken);



            if (!string.IsNullOrWhiteSpace(apiError))

            {

                logger.LogWarning("Maintenance AI failed for {Address}: {Error}", propertyInfo.FormattedAddress, apiError);

                return Unavailable(apiError);

            }



            if (string.IsNullOrWhiteSpace(json))

            {

                return Unavailable("OpenAI did not return maintenance recommendations. Please try again.");

            }



            var plan = ParseJson(json);

            if (plan.Items.Count < 3)

            {

                logger.LogWarning(

                    "Maintenance AI returned insufficient items ({Count}) for {Address}",

                    plan.Items.Count,

                    propertyInfo.FormattedAddress);



                return Unavailable("OpenAI returned an incomplete maintenance plan. Please search your address again.");

            }



            plan.DataSource = "OpenAI";

            plan.GeneratedUtc = DateTime.UtcNow;

            plan.IsAiGenerated = true;

            return plan;

        }

        catch (Exception ex)

        {

            logger.LogError(ex, "Maintenance recommendation failed for {Address}", propertyInfo.FormattedAddress);

            return Unavailable("We couldn't reach OpenAI right now. Please try again in a moment.");

        }

    }



    private async Task<(string? Json, string? Error)> CallChatJsonAsync(

        string systemMessage, string userMessage, CancellationToken cancellationToken)

    {

        var requestBody = new

        {

            model = string.IsNullOrWhiteSpace(options.Value.FastModel)
                ? options.Value.Model
                : options.Value.FastModel,

            temperature = 0.35,

            max_tokens = options.Value.MaintenanceMaxTokens,

            response_format = new { type = "json_object" },

            messages = new[]

            {

                new { role = "system", content = systemMessage },

                new { role = "user", content = userMessage }

            }

        };



        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);

        request.Content = new StringContent(

            JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json");



        using var response = await httpClient.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);



        if (!response.IsSuccessStatusCode)

        {

            var apiError = JsonNode.Parse(responseBody)?["error"]?["message"]?.GetValue<string>();

            return (null, apiError ?? $"OpenAI request failed ({(int)response.StatusCode}).");

        }



        var content = JsonNode.Parse(responseBody)?["choices"]?.AsArray()?.FirstOrDefault()?["message"]?["content"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(content))

        {

            return (null, "OpenAI returned an empty maintenance response.");

        }



        return (content, null);

    }



    private static PropertyMaintenancePlanViewModel ParseJson(string json)

    {

        var plan = new PropertyMaintenancePlanViewModel();

        try

        {

            var root = JsonNode.Parse(json)?.AsObject();

            if (root == null) return plan;



            plan.Summary = root["summary"]?.GetValue<string>()?.Trim() ?? "";

            if (root["items"] is not JsonArray arr) return plan;



            var sort = 0;

            foreach (var node in arr)

            {

                if (node == null) continue;

                var title = node["title"]?.GetValue<string>()?.Trim();

                if (string.IsNullOrWhiteSpace(title)) continue;



                var category = NormalizeCategory(node["category"]?.GetValue<string>());
                var normalizedTitle = title.Length > 120 ? title[..120] : title;
                var resolvedIcon = PropertyMaintenanceIconResolver.Resolve(
                    node["icon"]?.GetValue<string>()?.Trim(),
                    category,
                    normalizedTitle);

                plan.Items.Add(new PropertyMaintenanceItemViewModel
                {
                    Title = normalizedTitle,
                    Description = node["description"]?.GetValue<string>()?.Trim() ?? "",
                    Category = category,
                    Priority = NormalizePriority(node["priority"]?.GetValue<string>()),
                    Frequency = node["frequency"]?.GetValue<string>()?.Trim() ?? "As needed",
                    Icon = resolvedIcon,

                    Reason = node["reason"]?.GetValue<string>()?.Trim(),

                    SortOrder = ++sort

                });

                if (sort >= 12) break;

            }

        }

        catch

        {

            // return partial/empty — caller validates count

        }



        return plan;

    }



    private static PropertyMaintenancePlanViewModel Unavailable(string message) =>

        new()

        {

            Summary = message,

            DataSource = "Unavailable",

            IsAiGenerated = false,

            GeneratedUtc = DateTime.UtcNow,

            Items = []

        };



    private static string NormalizeCategory(string? category)

    {

        var c = (category ?? "General").Trim();

        return c switch

        {

            _ when c.Contains("hvac", StringComparison.OrdinalIgnoreCase) => "HVAC",

            _ when c.Contains("plumb", StringComparison.OrdinalIgnoreCase) => "Plumbing",

            _ when c.Contains("electr", StringComparison.OrdinalIgnoreCase) => "Electrical",

            _ when c.Contains("roof", StringComparison.OrdinalIgnoreCase) => "Roof",

            _ when c.Contains("exterior", StringComparison.OrdinalIgnoreCase) => "Exterior",

            _ when c.Contains("safe", StringComparison.OrdinalIgnoreCase) => "Safety",

            _ when c.Contains("land", StringComparison.OrdinalIgnoreCase) => "Landscaping",

            _ => "General"

        };

    }



    private static string NormalizePriority(string? priority)

    {

        var p = (priority ?? "Routine").Trim();

        if (p.Contains("urgent", StringComparison.OrdinalIgnoreCase)) return "Urgent";

        if (p.Contains("high", StringComparison.OrdinalIgnoreCase)) return "High";

        return "Routine";

    }

}


