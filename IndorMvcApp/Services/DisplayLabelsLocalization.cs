using System.Globalization;
using IndorMvcApp.Localization;

namespace IndorMvcApp.Services;

/// <summary>Shared localization helper for *DisplayLabels services.</summary>
public static class DisplayLabelsLocalization
{
    public static bool IsSpanishUi => UiCulture.IsSpanish(CultureInfo.CurrentUICulture.Name);

    public static string L(string english) =>
        CatalogText.PickWithUiFallback(english, null, IsSpanishUi);
}
