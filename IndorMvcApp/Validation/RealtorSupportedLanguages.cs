namespace IndorMvcApp.Validation;

public static class RealtorSupportedLanguages
{
    public static IReadOnlyList<string> Supported { get; } =
    [
        "English",
        "Spanish"
    ];

    public static bool TryNormalize(string? languagesCsv, out string normalizedCsv, out string? errorMessage)
    {
        normalizedCsv = string.Empty;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(languagesCsv))
        {
            errorMessage = "Select at least one language.";
            return false;
        }

        var selected = languagesCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (selected.Count == 0)
        {
            errorMessage = "Select at least one language.";
            return false;
        }

        var invalid = selected
            .Where(l => !Supported.Contains(l, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (invalid.Count > 0)
        {
            errorMessage = "Only English and Spanish are supported.";
            return false;
        }

        normalizedCsv = string.Join(", ", selected
            .Select(l => Supported.First(s => string.Equals(s, l, StringComparison.OrdinalIgnoreCase))));

        return true;
    }

    public static string SerializeJson(string normalizedCsv)
    {
        if (string.IsNullOrWhiteSpace(normalizedCsv))
        {
            return "[]";
        }

        var items = normalizedCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        return System.Text.Json.JsonSerializer.Serialize(items);
    }
}
