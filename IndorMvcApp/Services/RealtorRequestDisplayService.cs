namespace IndorMvcApp.Services;

public static class RealtorRequestDisplayService
{
    public static string NeedLabel(string? value) => value switch
    {
        "Buy" => "Buy a home",
        "Sell" => "Sell a home",
        "Rent" => "Rent a home",
        "GeneralGuidance" => "General guidance",
        _ => "Buy a home"
    };

    public static string TimeframeLabel(string? value) => value switch
    {
        "ASAP" => "ASAP",
        "Days30" => "30 days",
        "Days60Plus" => "60+ days",
        _ => value ?? "ASAP"
    };

    public static string StatusLabel(string? value) => value switch
    {
        "MatchingInProgress" => "Matching in progress",
        "Matched" => "Matched",
        "Completed" => "Completed",
        "Cancelled" => "Cancelled",
        _ => "Matching in progress"
    };

    public static string? PriceRangeLabel(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    public static readonly (string Value, string Label)[] PriceRangeOptions =
    [
        ("Under300k", "Under $300k"),
        ("300k-500k", "$300k – $500k"),
        ("500k-750k", "$500k – $750k"),
        ("750kPlus", "$750k+")
    ];

    public static string? ExtractCityFromAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        var parts = address.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return parts[^2].Trim();
        }

        return null;
    }
}
