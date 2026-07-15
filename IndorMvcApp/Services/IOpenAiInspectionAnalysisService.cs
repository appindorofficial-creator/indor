namespace IndorMvcApp.Services;

public sealed class InspectionAnalysisFinding
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? SourceExcerpt { get; set; }
    public string? SourceSection { get; set; }
    public string? SourceSectionNumber { get; set; }
    public int? SourcePage { get; set; }
    public string Priority { get; set; } = "Moderate";
    public string Trade { get; set; } = "Electrical";
    public int AiScore { get; set; } = 75;
}

public sealed class InspectionAnalysisResult
{
    public bool Success { get; set; }
    public string? Summary { get; set; }
    public int PageCount { get; set; }
    public List<InspectionAnalysisFinding> Findings { get; set; } = [];
    public string? ErrorMessage { get; set; }
}

public interface IOpenAiInspectionAnalysisService
{
    Task<InspectionAnalysisResult> AnalyzeReportAsync(
        string propertyAddress,
        string reportFilePath,
        CancellationToken cancellationToken = default,
        bool useSpanish = false);
}
