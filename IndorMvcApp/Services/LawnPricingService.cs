namespace IndorMvcApp.Services;

public static class LawnPricingService
{
    public const decimal SubscriptionDiscount = 10m;

    public static readonly IReadOnlyList<LawnAddonOption> AddonOptions =
    [
        new("TrimOneTree", "Trim 1 tree", 25m, "fa-tree"),
        new("TrimTwoTrees", "Trim 2 trees", 45m, "fa-tree"),
        new("PlantFlowers", "Plant flowers", 35m, "fa-seedling"),
        new("EdgeBorders", "Edge borders", 20m, "fa-border-all"),
        new("BushTrimming", "Bush trimming", 30m, "fa-leaf"),
        new("LeafHaulAway", "Leaf / debris haul-away", 18m, "fa-recycle")
    ];

    public static readonly IReadOnlyList<LawnAreaOption> AreaOptions =
    [
        new("FrontYard", "Front yard only", 45m, "fa-house", false),
        new("BackYard", "Back yard only", 45m, "fa-fence", false),
        new("FrontBack", "Front + Back yard", 75m, "fa-house-chimney", false),
        new("SideYard", "Side yard", 55m, "fa-road", true),
        new("FullProperty", "Full property", 95m, "fa-map", true)
    ];

    public static decimal GetBasePrice(string? areaCode)
    {
        var area = AreaOptions.FirstOrDefault(a => a.Code == areaCode);
        return area?.Price ?? 45m;
    }

    public static decimal GetAddonsTotal(string? pipeValue)
    {
        if (string.IsNullOrWhiteSpace(pipeValue)) return 0m;

        return pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(code => AddonOptions.FirstOrDefault(a => a.Code == code)?.Price ?? 0m)
            .Sum();
    }

    public static decimal GetSubscriptionDiscount(string? tipoServicio) =>
        string.Equals(tipoServicio, "Subscription", StringComparison.OrdinalIgnoreCase)
            ? SubscriptionDiscount
            : 0m;

    public static decimal CalculateTotal(string? tipoServicio, string? areaCode, string? addonsPipe)
    {
        var basePrice = GetBasePrice(areaCode);
        var addons = GetAddonsTotal(addonsPipe);
        var discount = GetSubscriptionDiscount(tipoServicio);
        return Math.Max(0m, basePrice + addons - discount);
    }

    public static IEnumerable<LawnAddonOption> ParseAddons(string? pipeValue)
    {
        if (string.IsNullOrWhiteSpace(pipeValue)) yield break;

        foreach (var code in pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var addon = AddonOptions.FirstOrDefault(a => a.Code == code);
            if (addon != null) yield return addon;
        }
    }

    public static string FormatAddonsPipe(IEnumerable<string> codes) =>
        string.Join("|", codes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct());
}

public record LawnAddonOption(string Code, string Label, decimal Price, string Icon);

public record LawnAreaOption(string Code, string Label, decimal Price, string Icon, bool IsCustomQuote);
