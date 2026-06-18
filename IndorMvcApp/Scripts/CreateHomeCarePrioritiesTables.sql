/*
  Home Care Priorities — section config and priority cards for Home / Services.
  Safe to run multiple times.

  IMPORTANT: Home Care cards use dedicated /priority-*.png assets only.
  Do NOT reuse /servicio*.jpeg or /inspeccion*.jpeg (those belong to Services / Inspections).
*/

IF OBJECT_ID(N'dbo.HomeCarePrioritiesConfig', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HomeCarePrioritiesConfig](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Titulo] [nvarchar](80) NOT NULL,
        [Subtitulo] [nvarchar](200) NOT NULL,
        [IconoClase] [nvarchar](50) NOT NULL CONSTRAINT [DF_HomeCarePrioritiesConfig_IconoClase] DEFAULT (N'fa-shield-halved'),
        [ViewAllTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_HomeCarePrioritiesConfig_ViewAllTexto] DEFAULT (N'View all tasks'),
        [ViewAllController] [nvarchar](80) NULL,
        [ViewAllAction] [nvarchar](80) NULL,
        [Activo] [bit] NOT NULL CONSTRAINT [DF_HomeCarePrioritiesConfig_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_HomeCarePrioritiesConfig_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_HomeCarePrioritiesConfig] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Table HomeCarePrioritiesConfig created.';
END
GO

IF OBJECT_ID(N'dbo.HomeCarePriorities', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HomeCarePriorities](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](120) NOT NULL,
        [Subtitulo] [nvarchar](120) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [IconoClase] [nvarchar](50) NOT NULL CONSTRAINT [DF_HomeCarePriorities_IconoClase] DEFAULT (N'fa-wrench'),
        [LinkController] [nvarchar](80) NULL,
        [LinkAction] [nvarchar](80) NULL,
        [LinkUrl] [nvarchar](300) NULL,
        [Orden] [int] NOT NULL CONSTRAINT [DF_HomeCarePriorities_Orden] DEFAULT (0),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_HomeCarePriorities_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_HomeCarePriorities_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_HomeCarePriorities] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_HomeCarePriorities_Nombre] ON dbo.HomeCarePriorities ([Nombre]);
    PRINT 'Table HomeCarePriorities created.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HomeCarePrioritiesConfig)
BEGIN
    INSERT INTO dbo.HomeCarePrioritiesConfig (
        Titulo, Subtitulo, IconoClase, ViewAllTexto, ViewAllController, ViewAllAction, Activo)
    VALUES (
        N'Home Maintenance Planner',
        N'Stay ahead of important home maintenance.',
        N'fa-shield-halved',
        N'View all tasks',
        N'Home',
        N'HomeCareGuide',
        1);
    PRINT 'HomeCarePrioritiesConfig seeded.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HomeCarePriorities)
BEGIN
    INSERT INTO dbo.HomeCarePriorities (Nombre, Subtitulo, ImagenUrl, IconoClase, LinkController, LinkAction, Orden, Activo)
    VALUES
    (N'HVAC maintenance', N'Every year', N'/priority-hvac-maintenance.png', N'fa-fan', N'HvacMaintenance', N'HvacMaintenanceService', 1, 1),
    (N'Water heater flush', N'Every year', N'/priority-water-heater-flush.png', N'fa-droplet', N'WaterHeaterFlush', N'WaterHeaterFlushService', 2, 1),
    (N'Crawlspace check', N'Every 1–2 years', N'/priority-crawlspace-check.png', N'fa-warehouse', N'CrawlspaceCheck', N'CrawlspaceCheckService', 3, 1),
    (N'Roof inspection', N'Every 1–2 years', N'/priority-roof-inspection.png', N'fa-house-chimney', N'RoofInspection', N'RoofInspectionService', 4, 1),
    (N'Power wash exterior', N'Every 1–2 years', N'/priority-power-wash-exterior.png', N'fa-spray-can-sparkles', N'PowerWash', N'PowerWashService', 5, 1),
    (N'Exterior paint', N'Every 5–7 years', N'/priority-exterior-paint.png', N'fa-paint-roller', N'ExteriorPaint', N'ExteriorPaintReview', 6, 1),
    (N'Gutter cleaning', N'Recommended seasonally', N'/priority-gutter-cleaning.png', N'fa-water', N'GutterCleaning', N'GutterCleaningService', 7, 1),
    (N'Pest control', N'Recommended yearly', N'/priority-pest-control.png', N'fa-bug', N'PestControl', N'PestControlService', 8, 1),
    (N'Smoke Detector', N'Test monthly', N'/priority-smoke-detector.png', N'fa-bell', N'SmokeDetector', N'SmokeDetectorService', 9, 1);
    PRINT 'HomeCarePriorities seeded.';
END
GO

IF EXISTS (SELECT 1 FROM dbo.HomeCarePrioritiesConfig)
BEGIN
    UPDATE dbo.HomeCarePrioritiesConfig
    SET Titulo = N'Home Maintenance Planner'
    WHERE Titulo <> N'Home Maintenance Planner';

    UPDATE dbo.HomeCarePrioritiesConfig
    SET ViewAllController = N'Home',
        ViewAllAction = N'HomeCareGuide'
    WHERE ViewAllController = N'MyHome' AND ViewAllAction = N'Maintenance';
END
GO

-- Sync images for existing rows (never share Servicios / Inspecciones catalog JPEGs)
IF EXISTS (SELECT 1 FROM dbo.HomeCarePriorities)
BEGIN
    UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-hvac-maintenance.png'      WHERE Nombre = N'HVAC maintenance';
    UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-water-heater-flush.png'   WHERE Nombre = N'Water heater flush';
    UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-crawlspace-check.png'      WHERE Nombre = N'Crawlspace check';
    UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-roof-inspection.png'        WHERE Nombre = N'Roof inspection';
    UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-power-wash-exterior.png'   WHERE Nombre = N'Power wash exterior';
    UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-exterior-paint.png'         WHERE Nombre = N'Exterior paint';
    UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-gutter-cleaning.png'       WHERE Nombre = N'Gutter cleaning';
    UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-pest-control.png'           WHERE Nombre = N'Pest control';
    UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-smoke-detector.png'        WHERE Nombre = N'Smoke Detector';
    UPDATE dbo.HomeCarePriorities SET Subtitulo = N'Every 1–2 years'                    WHERE Nombre = N'Roof inspection';
    PRINT 'HomeCarePriorities images synced to priority-*.png assets.';
END
GO

PRINT 'Home Care Priorities schema ready.';
GO
