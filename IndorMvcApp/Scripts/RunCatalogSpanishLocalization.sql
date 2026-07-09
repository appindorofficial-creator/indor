-- =============================================================
-- IndorDB — Run all bilingual catalog scripts (manual order)
-- =============================================================
-- Run these scripts manually in SSMS or sqlcmd, in this order:
--
--   1. AlterAspNetUsersAddPreferredUiCulture.sql   (UI language preference)
--   2. AlterCatalogTablesAddSpanishColumns.sql     (add *Es columns)
--   3. SeedCatalogSpanishTranslations.sql          (populate Spanish text)
--   4. SeedHomeCareMovingSetupSpanishTranslations.sql (Home Care Guide + Moving Setup)
--
-- Optional: after seeding, add Spanish for custom rows / landing pages:
--   UPDATE dbo.Microservicios SET NombreEs = N'...' WHERE Id = ...;
--   UPDATE dbo.LawnServicioLanding SET PageTitleEs = N'...' WHERE MicroservicioId = 2;
--
-- Notes:
--   - English stays in existing columns (Nombre, LabelEn, PageTitle, …)
--   - Spanish goes in new *Es columns
--   - Scripts are idempotent — safe to re-run
--   - Landing pages (*ServicioLanding) get columns in step 2;
--     translate their content manually or with follow-up UPDATE scripts
-- =============================================================

PRINT 'RunCatalogSpanishLocalization — execute the three scripts listed in the file header.';
GO
