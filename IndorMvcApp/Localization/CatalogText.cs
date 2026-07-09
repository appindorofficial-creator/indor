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
}
