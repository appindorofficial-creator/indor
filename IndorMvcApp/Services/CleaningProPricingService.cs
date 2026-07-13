namespace IndorMvcApp.Services;

public static class CleaningProPricingService
{
    public const decimal HourlyRatePerCleaner = 35m;
    public const decimal SalesTaxRate = 0.0825m;

    public static readonly IReadOnlyList<CleaningProAreaOption> AreaOptions =
    [
        new("Bathrooms", "Bathrooms", "fa-bath"),
        new("Kitchen", "Kitchen", "fa-kitchen-set"),
        new("Bedrooms", "Bedrooms", "fa-bed"),
        new("LivingRoom", "Living room", "fa-couch"),
        new("Baseboards", "Baseboards", "fa-grip-lines"),
        new("Floors", "Floors", "fa-border-all"),
        new("InsideFridge", "Inside fridge", "fa-snowflake"),
        new("InsideOven", "Inside oven", "fa-fire-burner"),
        new("Windows", "Windows", "fa-window-maximize"),
        new("Dusting", "Dusting", "fa-wind")
    ];

    public static readonly IReadOnlyList<CleaningProAddonOption> AddonOptions =
    [
        new("InsideOven", "Inside oven cleaning", 25m, "fa-fire-burner"),
        new("InsideFridge", "Inside fridge cleaning", 20m, "fa-snowflake")
    ];

    public static string NormalizeCrewCode(string? crewCode) => crewCode?.Trim() switch
    {
        "1" or "One" => "One",
        "2" or "Two" => "Two",
        "3" or "Three" => "Three",
        _ => string.IsNullOrWhiteSpace(crewCode) ? string.Empty : crewCode!
    };

    public static decimal GetHourlyRate(string? crewCode) => NormalizeCrewCode(crewCode) switch
    {
        "Two" => 70m,
        "Three" => 105m,
        _ => 35m
    };

    public static int GetCleanerCount(string? crewCode) => NormalizeCrewCode(crewCode) switch
    {
        "Two" => 2,
        "Three" => 3,
        _ => 1
    };

    public static decimal GetAddonsTotal(string? pipeValue)
    {
        if (string.IsNullOrWhiteSpace(pipeValue)) return 0m;

        return pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(code => AddonOptions.FirstOrDefault(a => a.Code == code)?.Price ?? 0m)
            .Sum();
    }

    public static CleaningProPriceBreakdown Calculate(
        string? crewCode,
        decimal hours,
        string? addonsPipe)
    {
        var hourlyRate = GetHourlyRate(crewCode);
        var serviceSubtotal = hourlyRate * hours;
        var addons = GetAddonsTotal(addonsPipe);
        var subtotal = serviceSubtotal + addons;
        var tax = Math.Round(subtotal * SalesTaxRate, 2);
        var total = subtotal + tax;

        return new CleaningProPriceBreakdown(hourlyRate, hours, serviceSubtotal, addons, subtotal, tax, total);
    }

    public static IEnumerable<CleaningProAreaOption> ParseAreas(string? pipeValue)
    {
        if (string.IsNullOrWhiteSpace(pipeValue)) yield break;

        foreach (var code in pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var area = AreaOptions.FirstOrDefault(a => a.Code == code);
            if (area != null) yield return area;
        }
    }

    public static IEnumerable<CleaningProAddonOption> ParseAddons(string? pipeValue)
    {
        if (string.IsNullOrWhiteSpace(pipeValue)) yield break;

        foreach (var code in pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var addon = AddonOptions.FirstOrDefault(a => a.Code == code);
            if (addon != null) yield return addon;
        }
    }
}

public record CleaningProAreaOption(string Code, string Label, string Icon);

public record CleaningProAddonOption(string Code, string Label, decimal Price, string Icon);

public record CleaningProPriceBreakdown(
    decimal HourlyRate,
    decimal Hours,
    decimal ServiceSubtotal,
    decimal AddonsTotal,
    decimal Subtotal,
    decimal Tax,
    decimal Total);
