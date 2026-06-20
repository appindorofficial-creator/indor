using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using IndorMvcApp.Models;

using Microsoft.Extensions.Options;



namespace IndorMvcApp.Services;



public class OpenAiInspectionAnalysisService(

    HttpClient httpClient,

    IOptions<OpenAiPropertyOptions> options,

    ILogger<OpenAiInspectionAnalysisService> logger) : IOpenAiInspectionAnalysisService

{

    private const int ChunkedPageThreshold = 15;

    private const int ChunkedCharThreshold = 45_000;



    private static readonly JsonSerializerOptions JsonOptions = new()

    {

        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        PropertyNameCaseInsensitive = true

    };



    public async Task<InspectionAnalysisResult> AnalyzeReportAsync(

        string propertyAddress,

        string reportFilePath,

        CancellationToken cancellationToken = default)

    {

        var pages = InspectionReportTextExtractor.ExtractAllPages(reportFilePath);

        if (pages.Count == 0 || pages.All(p => string.IsNullOrWhiteSpace(p.Text)))

        {

            return new InspectionAnalysisResult

            {

                Success = false,

                PageCount = pages.Count,

                ErrorMessage = pages.Count > 0

                    ? "This PDF has no readable text (it may be scanned images only). Upload a text-based PDF or OCR version."

                    : "Could not extract text from the inspection report."

            };

        }



        var pageCount = pages.Count;

        var openAi = options.Value;

        if (!openAi.Enabled || string.IsNullOrWhiteSpace(openAi.ApiKey))

        {

            logger.LogWarning("OpenAI inspection analysis skipped — API not configured");

            return BuildUnavailableResult(pageCount, "AI analysis is not configured. Enable OpenAI in application settings.");

        }



        var totalChars = InspectionReportTextExtractor.TotalCharCount(pages);

        var useChunks = pageCount > ChunkedPageThreshold || totalChars > ChunkedCharThreshold;



        try

        {

            if (useChunks)

            {

                logger.LogInformation(

                    "Analyzing {Pages}-page report in chunks ({Chars} chars) for {Address}",

                    pageCount,

                    totalChars,

                    propertyAddress);

                return await AnalyzeInChunksAsync(propertyAddress, pages, pageCount, cancellationToken);

            }



            var (text, _) = InspectionReportTextExtractor.ExtractFromFile(reportFilePath);

            var userPrompt = InspectionAnalysisPrompt.BuildUserPrompt(propertyAddress, text, pageCount);

            return await AnalyzeSinglePromptAsync(propertyAddress, userPrompt, pageCount, cancellationToken);

        }

        catch (TaskCanceledException ex)

        {

            logger.LogError(ex, "OpenAI inspection analysis timed out for {Address}", propertyAddress);

            return BuildUnavailableResult(

                pageCount,

                "OpenAI took too long analyzing this report. Please retry — large PDFs are processed in sections.");

        }

        catch (Exception ex)

        {

            logger.LogError(ex, "OpenAI inspection analysis failed for {Address}", propertyAddress);

            return BuildUnavailableResult(pageCount, DescribeException(ex));

        }

    }



    private async Task<InspectionAnalysisResult> AnalyzeSinglePromptAsync(

        string propertyAddress,

        string userPrompt,

        int pageCount,

        CancellationToken cancellationToken)

    {

        var (json, apiError) = await CallChatJsonAsync(

            InspectionAnalysisPrompt.SystemMessage, userPrompt, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))

        {

            logger.LogWarning("OpenAI inspection analysis returned empty JSON for {Address}", propertyAddress);

            return BuildUnavailableResult(

                pageCount,

                apiError ?? "OpenAI returned no analysis. Check your API key, billing, and try again.");

        }



        return ParseAnalysisJson(json, pageCount, requireFindings: true);

    }



    private async Task<InspectionAnalysisResult> AnalyzeInChunksAsync(

        string propertyAddress,

        IReadOnlyList<(int PageNumber, string Text)> pages,

        int pageCount,

        CancellationToken cancellationToken)

    {

        var chunks = InspectionReportTextExtractor.BuildPageChunks(pages);

        if (chunks.Count == 0)

        {

            return BuildUnavailableResult(pageCount, "Could not split the inspection report for analysis.");

        }



        var mergedFindings = new List<InspectionAnalysisFinding>();

        string? summary = null;

        string? lastError = null;

        var successfulChunks = 0;



        for (var i = 0; i < chunks.Count; i++)

        {

            cancellationToken.ThrowIfCancellationRequested();



            var userPrompt = InspectionAnalysisPrompt.BuildChunkUserPrompt(

                propertyAddress,

                pageCount,

                chunks[i],

                i + 1,

                chunks.Count);



            var (json, apiError) = await CallChatJsonAsync(

                InspectionAnalysisPrompt.SystemMessage, userPrompt, cancellationToken);



            if (string.IsNullOrWhiteSpace(json))

            {

                lastError = apiError;

                logger.LogWarning(

                    "OpenAI chunk {Chunk}/{Total} failed for {Address}: {Error}",

                    i + 1,

                    chunks.Count,

                    propertyAddress,

                    apiError);

                continue;

            }



            var partial = ParseAnalysisJson(json, pageCount, requireFindings: false);

            if (partial.Findings.Count == 0)

            {

                lastError = partial.ErrorMessage ?? apiError;

                continue;

            }



            successfulChunks++;

            if (summary == null && !string.IsNullOrWhiteSpace(partial.Summary))

            {

                summary = partial.Summary;

            }



            foreach (var finding in partial.Findings)

            {

                if (mergedFindings.Any(existing =>

                        string.Equals(existing.Title, finding.Title, StringComparison.OrdinalIgnoreCase)))

                {

                    continue;

                }



                mergedFindings.Add(finding);

            }

        }



        if (mergedFindings.Count == 0)

        {

            return BuildUnavailableResult(

                pageCount,

                lastError ?? "OpenAI could not extract repair findings from this report. Please retry.");

        }



        logger.LogInformation(

            "Chunked analysis complete for {Address}: {ChunksOk}/{ChunksTotal} sections, {FindingCount} findings",

            propertyAddress,

            successfulChunks,

            chunks.Count,

            mergedFindings.Count);



        return new InspectionAnalysisResult

        {

            Success = true,

            Summary = summary ?? $"INDOR AI identified {mergedFindings.Count} repair items across {pageCount} pages.",

            PageCount = pageCount,

            Findings = mergedFindings

        };

    }



    private Uri ResolveChatCompletionsUri()
    {
        var baseUrl = string.IsNullOrWhiteSpace(options.Value.BaseUrl)
            ? "https://api.openai.com/v1"
            : options.Value.BaseUrl.Trim().TrimEnd('/');
        return new Uri($"{baseUrl}/chat/completions");
    }

    private async Task<(string? Json, string? Error)> CallChatJsonAsync(

        string systemMessage, string userMessage, CancellationToken cancellationToken)

    {

        var requestBody = new

        {

            model = options.Value.Model,

            temperature = 0.2,

            max_tokens = 16384,

            response_format = new { type = "json_object" },

            messages = new[]

            {

                new { role = "system", content = systemMessage },

                new { role = "user", content = userMessage }

            }

        };



        using var request = new HttpRequestMessage(HttpMethod.Post, ResolveChatCompletionsUri());

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);

        request.Content = new StringContent(

            JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json");



        using var response = await httpClient.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);



        if (!response.IsSuccessStatusCode)

        {

            var apiError = JsonNode.Parse(responseBody)?["error"]?["message"]?.GetValue<string>();

            logger.LogWarning(

                "OpenAI inspection call failed ({StatusCode}): {Error}",

                (int)response.StatusCode,

                apiError ?? responseBody);

            return (null, apiError ?? $"OpenAI request failed ({(int)response.StatusCode}).");

        }



        var content = JsonNode.Parse(responseBody)?["choices"]?.AsArray()?.FirstOrDefault()?["message"]?["content"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(content))

        {

            logger.LogWarning("OpenAI inspection call returned empty content");

            return (null, "OpenAI returned an empty analysis response.");

        }



        return (content, null);

    }



    private static InspectionAnalysisResult ParseAnalysisJson(

        string json, int fallbackPageCount, bool requireFindings)

    {

        try

        {

            var root = JsonNode.Parse(json)?.AsObject();

            if (root == null)

            {

                return BuildUnavailableResult(fallbackPageCount, "AI response could not be parsed. Please try again.");

            }



            var summary = root["summary"]?.GetValue<string>();

            var pageCount = root["pageCount"]?.GetValue<int>() ?? fallbackPageCount;

            var findings = ParseFindings(root["findings"] as JsonArray);



            if (findings.Count == 0 && requireFindings)

            {

                return BuildUnavailableResult(

                    pageCount,

                    "No repair findings were detected in this report. Try a more detailed inspection PDF.");

            }



            return new InspectionAnalysisResult

            {

                Success = findings.Count > 0,

                Summary = summary,

                PageCount = pageCount > 0 ? pageCount : fallbackPageCount,

                Findings = findings,

                ErrorMessage = findings.Count == 0 ? "No findings in this section." : null

            };

        }

        catch

        {

            return BuildUnavailableResult(fallbackPageCount, "AI response could not be parsed. Please try again.");

        }

    }



    private static List<InspectionAnalysisFinding> ParseFindings(JsonArray? arr)

    {

        var findings = new List<InspectionAnalysisFinding>();

        if (arr == null)

        {

            return findings;

        }



        foreach (var node in arr)

        {

            if (node == null) continue;

            var title = node["title"]?.GetValue<string>()?.Trim();

            if (string.IsNullOrWhiteSpace(title)) continue;



            var trade = NormalizeTrade(node["trade"]?.GetValue<string>());

            var priority = NormalizePriority(node["priority"]?.GetValue<string>());

            var score = node["aiScore"]?.GetValue<int>() ?? PriorityDefaultScore(priority);



            var excerpt = node["sourceExcerpt"]?.GetValue<string>()?.Trim();

            if (string.IsNullOrWhiteSpace(excerpt))

            {

                excerpt = node["reportExcerpt"]?.GetValue<string>()?.Trim();

            }



            var sourceSection = node["sourceSection"]?.GetValue<string>()?.Trim();
            if (string.IsNullOrWhiteSpace(sourceSection))
            {
                sourceSection = node["reportSection"]?.GetValue<string>()?.Trim();
            }

            if (!string.IsNullOrWhiteSpace(sourceSection) && sourceSection.Length > 120)
            {
                sourceSection = sourceSection[..120];
            }

            var sourceSectionNumber = NormalizeSectionNumber(
                node["sourceSectionNumber"]?.GetValue<string>()
                ?? node["sectionNumber"]?.GetValue<string>()
                ?? node["reportSectionNumber"]?.GetValue<string>());

            int? sourcePage = null;

            if (node["sourcePage"] != null && int.TryParse(node["sourcePage"]?.ToString(), out var page) && page > 0)

            {

                sourcePage = page;

            }



            findings.Add(new InspectionAnalysisFinding

            {

                Title = title.Length > 200 ? title[..200] : title,

                Description = node["description"]?.GetValue<string>()?.Trim() ?? "",

                SourceExcerpt = TruncateExcerpt(excerpt),

                SourceSection = string.IsNullOrWhiteSpace(sourceSection) ? null : sourceSection,

                SourceSectionNumber = sourceSectionNumber,

                SourcePage = sourcePage,

                Priority = priority,

                Trade = trade,

                AiScore = Math.Clamp(score, 50, 100)

            });

        }



        return findings;

    }



    private static InspectionAnalysisResult BuildUnavailableResult(int pageCount, string message) =>

        new()

        {

            Success = false,

            PageCount = pageCount,

            ErrorMessage = message,

            Findings = []

        };



    private static string DescribeException(Exception ex)

    {

        var message = ex.InnerException?.Message ?? ex.Message;

        if (message.Length > 220)

        {

            message = message[..220] + "…";

        }



        return $"AI analysis error: {message}";

    }



    private static string? TruncateExcerpt(string? excerpt)
    {
        if (string.IsNullOrWhiteSpace(excerpt)) return null;
        excerpt = excerpt.Trim();
        return excerpt.Length > 2000 ? excerpt[..2000] : excerpt;
    }

    private static string? NormalizeSectionNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();
        var match = Regex.Match(value, @"^\d+(?:\.\d+)+");
        if (match.Success)
        {
            return match.Value.Length <= 30 ? match.Value : match.Value[..30];
        }

        return value.Length <= 30 ? value : value[..30];
    }



    private static string NormalizeTrade(string? trade)

    {

        var t = (trade ?? "").Trim();

        if (t.Equals(RealtorInspectionTrades.Handyman, StringComparison.OrdinalIgnoreCase)

            || t.Contains("handyman", StringComparison.OrdinalIgnoreCase)

            || t.Contains("general", StringComparison.OrdinalIgnoreCase))

        {

            return RealtorInspectionTrades.Handyman;

        }



        if (t.Contains("electr", StringComparison.OrdinalIgnoreCase)) return RealtorInspectionTrades.Electrical;

        if (t.Contains("hvac", StringComparison.OrdinalIgnoreCase) || t.Contains("cool", StringComparison.OrdinalIgnoreCase)

            || t.Contains("heat", StringComparison.OrdinalIgnoreCase) || t.Contains("furnace", StringComparison.OrdinalIgnoreCase)

            || t.Contains("a/c", StringComparison.OrdinalIgnoreCase) || t.Contains("air cond", StringComparison.OrdinalIgnoreCase))

        {

            return RealtorInspectionTrades.Hvac;

        }



        if (t.Contains("plumb", StringComparison.OrdinalIgnoreCase)) return RealtorInspectionTrades.Plumbing;

        if (t.Contains("roof", StringComparison.OrdinalIgnoreCase)) return RealtorInspectionTrades.Roof;

        if (t.Contains("paint", StringComparison.OrdinalIgnoreCase)) return RealtorInspectionTrades.Paint;



        if (IsHandymanScope(t))

        {

            return RealtorInspectionTrades.Handyman;

        }



        return RealtorInspectionTrades.Handyman;

    }



    private static bool IsHandymanScope(string trade) =>

        trade.Contains("carpent", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("drywall", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("door", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("window", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("deck", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("fence", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("railing", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("handrail", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("caulk", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("trim", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("baseboard", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("shelv", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("screen", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("hardware", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("hinge", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("grout", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("weather", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("gutter", StringComparison.OrdinalIgnoreCase)

        || trade.Contains("cabinet", StringComparison.OrdinalIgnoreCase);



    private static string NormalizePriority(string? priority)

    {

        var p = (priority ?? "").Trim();

        if (p.Contains("urgent", StringComparison.OrdinalIgnoreCase)) return RealtorInspectionFindingPriorities.Urgent;

        if (p.Contains("high", StringComparison.OrdinalIgnoreCase)) return RealtorInspectionFindingPriorities.High;

        return RealtorInspectionFindingPriorities.Moderate;

    }



    private static int PriorityDefaultScore(string priority) => priority switch

    {

        RealtorInspectionFindingPriorities.Urgent => 90,

        RealtorInspectionFindingPriorities.High => 80,

        _ => 70

    };

}


