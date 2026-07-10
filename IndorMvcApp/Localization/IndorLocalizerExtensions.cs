using IndorMvcApp.Services;

namespace IndorMvcApp.Localization;

public static class IndorLocalizerExtensions
{
    /// <summary>Localize catalog/DB English strings using UiTranslations when no DB Spanish exists.</summary>
    public static string Catalog(this IIndorLocalizer localizer, string? text) =>
        CatalogText.PickWithUiFallback(text, null, localizer.IsSpanish);
}
