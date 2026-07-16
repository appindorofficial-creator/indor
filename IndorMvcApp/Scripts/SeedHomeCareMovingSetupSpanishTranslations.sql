-- =============================================================
-- IndorDB — Seed Spanish translations for Home Care Guide & Moving Setup
-- Prerequisite: AlterCatalogTablesAddSpanishColumns.sql (adds *Es columns)
-- Safe to run multiple times.
--
-- Usage (SSMS / sqlcmd):
--   USE [IndorDB];
--   GO
--   :r SeedHomeCareMovingSetupSpanishTranslations.sql
-- =============================================================

SET NOCOUNT ON;
GO

/* ---------- Home Care Priorities config ---------- */
IF OBJECT_ID(N'dbo.HomeCarePrioritiesConfig', N'U') IS NOT NULL
   AND COL_LENGTH(N'HomeCarePrioritiesConfig', N'TituloEs') IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePrioritiesConfig
    SET TituloEs = N'Planificador de mantenimiento del hogar',
        SubtituloEs = N'Mantente al día con el mantenimiento importante del hogar.',
        ViewAllTextoEs = N'Ver todas las tareas'
    WHERE Activo = 1;

    -- Legacy title still in some databases
    UPDATE dbo.HomeCarePrioritiesConfig
    SET TituloEs = N'Guía de cuidado del hogar'
    WHERE Titulo = N'Home Care Guide' AND Activo = 1;

    PRINT 'HomeCarePrioritiesConfig Spanish updated.';
END
GO

/* ---------- Home Care priority cards ---------- */
IF OBJECT_ID(N'dbo.HomeCarePriorities', N'U') IS NOT NULL
   AND COL_LENGTH(N'HomeCarePriorities', N'NombreEs') IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePriorities SET NombreEs = N'Mantenimiento HVAC',       SubtituloEs = N'Cada año'              WHERE Nombre = N'HVAC maintenance';
    UPDATE dbo.HomeCarePriorities SET NombreEs = N'Lavado de calentador de agua', SubtituloEs = N'Cada año'        WHERE Nombre = N'Water heater flush';
    UPDATE dbo.HomeCarePriorities SET NombreEs = N'Revisión de espacio bajo el piso', SubtituloEs = N'Cada 1–2 años' WHERE Nombre = N'Crawlspace check';
    UPDATE dbo.HomeCarePriorities SET NombreEs = N'Inspección de techo',      SubtituloEs = N'Cada 1–2 años'        WHERE Nombre = N'Roof inspection';
    UPDATE dbo.HomeCarePriorities SET NombreEs = N'Lavado a presión exterior', SubtituloEs = N'Cada 1–2 años'      WHERE Nombre = N'Power wash exterior';
    UPDATE dbo.HomeCarePriorities SET NombreEs = N'Pintura exterior',         SubtituloEs = N'Cada 5–7 años'        WHERE Nombre = N'Exterior paint';
    UPDATE dbo.HomeCarePriorities SET NombreEs = N'Limpieza de canaletas',    SubtituloEs = N'Recomendado estacionalmente' WHERE Nombre = N'Gutter cleaning';
    UPDATE dbo.HomeCarePriorities SET NombreEs = N'Control de plagas',        SubtituloEs = N'Recomendado anualmente' WHERE Nombre = N'Pest control';
    UPDATE dbo.HomeCarePriorities SET NombreEs = N'Detector de humo',         SubtituloEs = N'Probar mensualmente'    WHERE Nombre = N'Smoke Detector';

    PRINT 'HomeCarePriorities Spanish updated.';
END
GO

/* ---------- Moving Setup config ---------- */
IF OBJECT_ID(N'dbo.MovingSetupConfig', N'U') IS NOT NULL
   AND COL_LENGTH(N'MovingSetupConfig', N'TituloEs') IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupConfig
    SET TituloEs = N'Configuración de mudanza',
        SubtituloEs = N'Todo lo que necesitas antes, durante y después de tu mudanza.',
        ViewAllTextoEs = N'Ver todo',
        FeaturedEtiquetaEs = N'DESTACADO',
        FeaturedTituloEs = N'Asistente de mudanza',
        FeaturedDescripcionEs = N'Reserva mudanza, limpieza, configuración y servicios en un solo lugar.',
        FeaturedCaracteristicasEs = N'Reserva rápida|Profesionales de confianza|Precios transparentes',
        FeaturedCtaTextoEs = N'Iniciar configuración de mudanza'
    WHERE Activo = 1;

    PRINT 'MovingSetupConfig Spanish updated.';
END
GO

/* ---------- Moving Setup services ---------- */
IF OBJECT_ID(N'dbo.MovingSetupServicios', N'U') IS NOT NULL
   AND COL_LENGTH(N'MovingSetupServicios', N'NombreEs') IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupServicios SET NombreEs = N'Mudanza'              WHERE Nombre = N'Moving';
    UPDATE dbo.MovingSetupServicios SET NombreEs = N'Limpieza'             WHERE Nombre = N'Cleaning';
    UPDATE dbo.MovingSetupServicios SET NombreEs = N'Ayuda con empaque'    WHERE Nombre = N'Packing Help';
    UPDATE dbo.MovingSetupServicios SET NombreEs = N'Muebles y ensamblaje' WHERE Nombre = N'Furniture & Assembly';
    UPDATE dbo.MovingSetupServicios SET NombreEs = N'Montaje de TV'        WHERE Nombre = N'TV Wall Mounting';
    UPDATE dbo.MovingSetupServicios SET NombreEs = N'Configuración de servicios' WHERE Nombre = N'Utilities Setup';
    UPDATE dbo.MovingSetupServicios SET NombreEs = N'Ayuda general'        WHERE Nombre = N'General Help';

    PRINT 'MovingSetupServicios Spanish updated.';
END
GO

/* ---------- Moving Setup quick links ---------- */
IF OBJECT_ID(N'dbo.MovingSetupEnlacesRapidos', N'U') IS NOT NULL
   AND COL_LENGTH(N'MovingSetupEnlacesRapidos', N'NombreEs') IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupEnlacesRapidos SET NombreEs = N'Lista de direcciones'       WHERE Nombre = N'Address checklist';
    UPDATE dbo.MovingSetupEnlacesRapidos SET NombreEs = N'Suministros'              WHERE Nombre = N'Supplies';
    UPDATE dbo.MovingSetupEnlacesRapidos SET NombreEs = N'Recolección de donaciones' WHERE Nombre = N'Donation pickup';
    UPDATE dbo.MovingSetupEnlacesRapidos SET NombreEs = N'Consejos de mudanza'      WHERE Nombre = N'Move tips';

    PRINT 'MovingSetupEnlacesRapidos Spanish updated.';
END
GO

PRINT 'SeedHomeCareMovingSetupSpanishTranslations completed.';
GO
