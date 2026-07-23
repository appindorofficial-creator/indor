namespace IndorMvcApp.Localization;

/// <summary>Shared photo/file source action-sheet labels (global upload UIs).</summary>
public static class UiTranslationsFileSource
{
    public static IEnumerable<KeyValuePair<string, string>> Entries { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Photo Library"] = "Biblioteca de fotos",
            ["Take Photo"] = "Tomar foto",
            ["Choose Files"] = "Elegir archivos",
            // Stored enrichment / lookup fallbacks shown on confirmation screens
            ["Address not found on Redfin or Zillow"] = "Dirección no encontrada en Redfin o Zillow",
            ["Address not found on Redfin or Zillow."] = "Dirección no encontrada en Redfin o Zillow.",
            ["Address not found."] = "Dirección no encontrada.",
            ["Address not found"] = "Dirección no encontrada",
        };
}
