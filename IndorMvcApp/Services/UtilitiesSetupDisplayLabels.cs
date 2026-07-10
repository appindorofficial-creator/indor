namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class UtilitiesSetupDisplayLabels
{
    public static string FormatService(string? value) => value switch
    {
        "Internet" => DisplayLabelsLocalization.L("Internet"),
        "Cable" => DisplayLabelsLocalization.L("Cable"),
        "Electricity" => DisplayLabelsLocalization.L("Electricity"),
        "Water" => DisplayLabelsLocalization.L("Water"),
        "Gas" => DisplayLabelsLocalization.L("Gas"),
        _ => value ?? "â€”"
    };

    public static string FormatServicesList(string? pipeValue) =>
        string.IsNullOrWhiteSpace(pipeValue)
            ? "â€”"
            : string.Join(", ", pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(FormatService));

    public static string FormatContactPreference(string? value) => value switch
    {
        "ContactMyself" => DisplayLabelsLocalization.L("I'll contact providers myself"),
        "SaveForLater" => DisplayLabelsLocalization.L("Save providers for later"),
        _ => value ?? "â€”"
    };

    public static string FormatCableOption(string? value) => value switch
    {
        "InternetOnly" => DisplayLabelsLocalization.L("Internet only"),
        "InternetTv" => DisplayLabelsLocalization.L("Internet + TV"),
        "BringMyOwn" => DisplayLabelsLocalization.L("Bring my own"),
        _ => value ?? "â€”"
    };

    public static string FormatUtilityType(string? value) => value switch
    {
        "Electricity" => DisplayLabelsLocalization.L("Electricity"),
        "Water" => DisplayLabelsLocalization.L("Water"),
        "Gas" => DisplayLabelsLocalization.L("Gas"),
        _ => value ?? "Utility"
    };

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMM d, yyyy") ?? "To be scheduled";

    public static string FormatInternetSummary(string? providerName, string? speed, decimal price) =>
        string.IsNullOrWhiteSpace(providerName)
            ? "Skipped"
            : $"{providerName} â€” {speed ?? "Plans available"} â€” from ${price:0}/mo";

    public static string UtilityIcon(string? tipo) => tipo switch
    {
        "Electricity" => DisplayLabelsLocalization.L("fa-bolt"),
        "Water" => DisplayLabelsLocalization.L("fa-droplet"),
        "Gas" => DisplayLabelsLocalization.L("fa-fire-flame-simple"),
        _ => DisplayLabelsLocalization.L("fa-plug")
    };

    public static string ProviderBadgeClass(string? etiqueta) => etiqueta switch
    {
        "Fastest" => DisplayLabelsLocalization.L("badge-green"),
        "Reliable" => DisplayLabelsLocalization.L("badge-purple"),
        "Great value" => DisplayLabelsLocalization.L("badge-blue"),
        _ => DisplayLabelsLocalization.L("badge-blue")
    };
}
