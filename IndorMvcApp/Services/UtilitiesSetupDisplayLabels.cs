namespace IndorMvcApp.Services;

public static class UtilitiesSetupDisplayLabels
{
    public static string FormatService(string? value) => value switch
    {
        "Internet" => "Internet",
        "Cable" => "Cable",
        "Electricity" => "Electricity",
        "Water" => "Water",
        "Gas" => "Gas",
        _ => value ?? "—"
    };

    public static string FormatServicesList(string? pipeValue) =>
        string.IsNullOrWhiteSpace(pipeValue)
            ? "—"
            : string.Join(", ", pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(FormatService));

    public static string FormatContactPreference(string? value) => value switch
    {
        "ContactMyself" => "I'll contact providers myself",
        "SaveForLater" => "Save providers for later",
        _ => value ?? "—"
    };

    public static string FormatCableOption(string? value) => value switch
    {
        "InternetOnly" => "Internet only",
        "InternetTv" => "Internet + TV",
        "BringMyOwn" => "Bring my own",
        _ => value ?? "—"
    };

    public static string FormatUtilityType(string? value) => value switch
    {
        "Electricity" => "Electricity",
        "Water" => "Water",
        "Gas" => "Gas",
        _ => value ?? "Utility"
    };

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMM d, yyyy") ?? "To be scheduled";

    public static string FormatInternetSummary(string? providerName, string? speed, decimal price) =>
        string.IsNullOrWhiteSpace(providerName)
            ? "Skipped"
            : $"{providerName} — {speed ?? "Plans available"} — from ${price:0}/mo";

    public static string UtilityIcon(string? tipo) => tipo switch
    {
        "Electricity" => "fa-bolt",
        "Water" => "fa-droplet",
        "Gas" => "fa-fire-flame-simple",
        _ => "fa-plug"
    };

    public static string ProviderBadgeClass(string? etiqueta) => etiqueta switch
    {
        "Fastest" => "badge-green",
        "Reliable" => "badge-purple",
        "Great value" => "badge-blue",
        _ => "badge-blue"
    };
}
