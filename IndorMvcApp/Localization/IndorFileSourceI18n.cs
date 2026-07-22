using IndorMvcApp.Services;

namespace IndorMvcApp.Localization;

/// <summary>
/// Canonical labels for the in-app photo/file source sheet (replaces the English iOS OS sheet).
/// Prefer <see cref="IIndorLocalizer"/> / UiTranslations; Spanish fallbacks guard against missing keys.
/// </summary>
public static class IndorFileSourceI18n
{
    public const string LibraryKey = "Photo Library";
    public const string CameraKey = "Take Photo";
    public const string FilesKey = "Choose Files";

    public static string Library(IIndorLocalizer localizer) => Resolve(localizer, LibraryKey, "Biblioteca de fotos");

    public static string Camera(IIndorLocalizer localizer) => Resolve(localizer, CameraKey, "Tomar foto");

    public static string Files(IIndorLocalizer localizer) => Resolve(localizer, FilesKey, "Elegir archivos");

    private static string Resolve(IIndorLocalizer localizer, string englishKey, string spanishFallback)
    {
        var text = localizer[englishKey];
        if (localizer.IsSpanish
            && (string.IsNullOrWhiteSpace(text) || string.Equals(text, englishKey, StringComparison.Ordinal)))
        {
            return spanishFallback;
        }

        return string.IsNullOrWhiteSpace(text) ? englishKey : text;
    }
}
