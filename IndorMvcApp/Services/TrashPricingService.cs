namespace IndorMvcApp.Services;

public static class TrashPricingService
{
    public static readonly IReadOnlyList<TrashBinOption> BinOptions =
    [
        new("Trash", "Trash", "fa-trash", "/trash-bin-trash.png", "blue"),
        new("Recycle", "Recycle", "fa-recycle", "/trash-bin-recycle.png", "blue"),
        new("YardWaste", "Yard waste", "fa-leaf", "/trash-bin-yardwaste.png", "green")
    ];

    public static readonly IReadOnlyDictionary<string, decimal> BinCountPrices =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["One"] = 20m,
            ["Two"] = 25m,
            ["Three"] = 30m
        };

    public static decimal GetMonthlyPrice(string? cantidadBins) =>
        cantidadBins != null && BinCountPrices.TryGetValue(cantidadBins, out var price)
            ? price
            : 20m;

    public static string FormatBinsPipe(IEnumerable<string> codes) =>
        string.Join("|", codes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct());

    public static IEnumerable<TrashBinOption> ParseBins(string? pipeValue)
    {
        if (string.IsNullOrWhiteSpace(pipeValue)) yield break;

        foreach (var code in pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var bin = BinOptions.FirstOrDefault(b => b.Code == code);
            if (bin != null) yield return bin;
        }
    }
}

public record TrashBinOption(string Code, string Label, string Icon, string? ImageUrl = null, string ThumbTone = "blue");
