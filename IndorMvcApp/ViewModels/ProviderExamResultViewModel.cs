namespace IndorMvcApp.ViewModels;

public class ProviderExamResultViewModel
{
    public int ScorePercent { get; init; }
    public bool Passed { get; init; }
    public int PassingPercent { get; init; }
    public int CorrectCount { get; init; }
    public int TotalCount { get; init; }
    public string TradeLabel { get; init; } = "trade";
    public IReadOnlyList<ProviderExamResultItem> Items { get; init; } = [];
}

public class ProviderExamResultItem
{
    public int Number { get; init; }
    public string Text { get; init; } = "";
    public IReadOnlyList<string> Options { get; init; } = [];
    public int? SelectedIndex { get; init; }
    public int CorrectIndex { get; init; }
    public bool IsCorrect { get; init; }

    public bool WasAnswered => SelectedIndex.HasValue;
    public string? SelectedAnswerText =>
        SelectedIndex is int i && i >= 0 && i < Options.Count ? Options[i] : null;
    public string? CorrectAnswerText =>
        CorrectIndex >= 0 && CorrectIndex < Options.Count ? Options[CorrectIndex] : null;
}
