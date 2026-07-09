namespace IndorMvcApp.Localization;

/// <summary>
/// English UI strings are the lookup keys; Spanish values are applied when es-US is active.
/// Merged from area-specific translation files.
/// </summary>
public static class UiTranslations
{
    public static readonly IReadOnlyDictionary<string, string> Spanish =
        UiTranslationBuilder.Merge(
            UiTranslationsShared.Entries,
            UiTranslationsHome.Entries,
            UiTranslationsProveedor.Entries,
            UiTranslationsRealtor.Entries,
            UiTranslationsFlows.Entries);
}
