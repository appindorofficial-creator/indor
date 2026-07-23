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
            // Service request marketplace — placed early so curated later files still win
            // for any shared short keys; unique long strings/toasts persist.
            UiTranslationsServiceRequest.Entries,
            UiTranslationsRealtor.Entries,
            UiTranslationsFlows.Entries,
            UiTranslationsPropietarioServices.Entries,
            UiTranslationsInspeccionesEmergency.Entries,
            UiTranslationsServiceWizards.Entries,
            UiTranslationsValidation.Entries,
            UiTranslationsRemaining.Entries,
            UiTranslationsPropertyAdministratorFlows.Entries,
            UiTranslationsPropertyAdministratorUi.Entries,
            UiTranslationsFinal.Entries,
            UiTranslationsEmergencyDisplay.Entries,
            // Last so Bug 20 moving-setup fixes override incomplete Flows/Final Spanish.
            UiTranslationsMovingSetup.Entries,
            // Additive Property Snapshot (Resumen) keys — keep after shared merges.
            UiTranslationsPropertySnapshot.Entries,
            // Additive House Facts section chips + section pages (Datos del hogar).
            UiTranslationsHouseFact.Entries,
            // Provider profile / onboarding service catalog labels (must win over incomplete Drywall etc.).
            UiTranslationsProviderCatalog.Entries,
            // Global photo/file source sheet — last so every persona inherits Biblioteca/Tomar/Elegir.
            UiTranslationsFileSource.Entries);
}
