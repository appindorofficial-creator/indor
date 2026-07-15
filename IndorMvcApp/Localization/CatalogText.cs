namespace IndorMvcApp.Localization;

public static class CatalogText
{
    public static string Pick(string? english, string? spanish, bool isSpanish)
    {
        if (isSpanish && !string.IsNullOrWhiteSpace(spanish))
        {
            return spanish;
        }

        return english ?? string.Empty;
    }

    public static string PickPipeList(string? english, string? spanish, bool isSpanish)
        => Pick(english, spanish, isSpanish);

    public static string PickWithUiFallback(string? english, string? spanish, bool isSpanish)
    {
        if (!isSpanish)
        {
            return english ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(spanish))
        {
            // Fix imperfect *Es seeds (e.g. mixed EN/ES) via UI dictionary keys.
            if (UiTranslations.Spanish.TryGetValue(spanish, out var fromSpanishKey))
            {
                return fromSpanishKey;
            }

            return spanish;
        }

        var key = english ?? string.Empty;
        return UiTranslations.Spanish.TryGetValue(key, out var translated) ? translated : key;
    }

    public static string PickPipeListWithUiFallback(string? english, string? spanish, bool isSpanish)
    {
        if (!isSpanish)
        {
            return english ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(spanish))
        {
            return spanish;
        }

        if (string.IsNullOrWhiteSpace(english))
        {
            return string.Empty;
        }

        return string.Join("|",
            english.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(part => PickWithUiFallback(part, null, true)));
    }
}
