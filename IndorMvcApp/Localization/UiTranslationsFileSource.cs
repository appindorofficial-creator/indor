namespace IndorMvcApp.Localization;

/// <summary>Shared photo/file source action-sheet labels (global upload UIs).</summary>
public static class UiTranslationsFileSource
{
    public static IEnumerable<KeyValuePair<string, string>> Entries { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Address not found on Redfin or Zillow"] = "Dirección no encontrada en Redfin o Zillow",
            ["Address not found on Redfin or Zillow."] = "Dirección no encontrada en Redfin o Zillow.",
            ["Address not found."] = "Dirección no encontrada.",
            ["Address not found"] = "Dirección no encontrada",
            ["Could not start your project request. Run Scripts/CreateRemodelingServicioFlowTables.sql on the database and try again."] =
                "No se pudo iniciar la solicitud del proyecto. Ejecuta Scripts/CreateRemodelingServicioFlowTables.sql en la base de datos e inténtalo de nuevo.",
            ["Could not save your project details. Please ensure the remodeling flow tables exist in the database and try again."] =
                "No se pudieron guardar los detalles del proyecto. Asegúrate de que existan las tablas del flujo de remodelación e inténtalo de nuevo.",
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
