/*
  Smoke / CO Check flow — landing + solicitudes for Home Care Priorities.
  Safe to run multiple times.
*/

IF NOT EXISTS (SELECT 1 FROM dbo.HomeCarePriorities WHERE Nombre = N'Smoke Detector')
BEGIN
    INSERT INTO dbo.HomeCarePriorities (Nombre, Subtitulo, ImagenUrl, IconoClase, LinkController, LinkAction, Orden, Activo)
    VALUES (N'Smoke Detector', N'Test monthly', N'/priority-smoke-detector.png', N'fa-bell', N'SmokeDetector', N'SmokeDetectorService', 9, 1);
    PRINT 'HomeCarePriorities row "Smoke Detector" inserted.';
END
ELSE
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET Subtitulo = N'Test monthly',
        ImagenUrl = N'/priority-smoke-detector.png',
        IconoClase = N'fa-bell',
        LinkController = N'SmokeDetector',
        LinkAction = N'SmokeDetectorService',
        LinkUrl = NULL,
        Orden = 9,
        Activo = 1
    WHERE Nombre = N'Smoke Detector';
END
GO

IF OBJECT_ID(N'dbo.SmokeDetectorServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SmokeDetectorServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_SmokeDetectorServicioLanding_PageTitle] DEFAULT (N'Smoke / CO Check'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingSubtitulo] [nvarchar](400) NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [TrackItems] [nvarchar](500) NULL,
        [TrackDescriptions] [nvarchar](500) NULL,
        [TrackIconos] [nvarchar](300) NULL,
        [WhereTrackItems] [nvarchar](300) NULL,
        [WhereTrackIconos] [nvarchar](300) NULL,
        [ReminderBannerTexto] [nvarchar](400) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_SmokeDetectorServicioLanding_CtaTexto] DEFAULT (N'Start reminder setup'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_SmokeDetectorServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SmokeDetectorServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_SmokeDetectorServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_SmokeDetectorServicioLanding_HomeCarePriorityId]
        ON dbo.SmokeDetectorServicioLanding ([HomeCarePriorityId]);

    ALTER TABLE [dbo].[SmokeDetectorServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_SmokeDetectorServicioLanding_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    PRINT 'Table SmokeDetectorServicioLanding created.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesSmokeDetector', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesSmokeDetector](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [CantidadAlarmas] [nvarchar](10) NULL,
        [UbicacionesSeleccionadas] [nvarchar](200) NULL,
        [TiposAlarmas] [nvarchar](200) NULL,
        [UltimaPrueba] [nvarchar](20) NULL,
        [UltimoCambioBateria] [nvarchar](20) NULL,
        [AnioInstalacion] [int] NULL,
        [AnioInstalacionDesconocido] [bit] NOT NULL CONSTRAINT [DF_SolicitudesSmokeDetector_AnioInstalacionDesconocido] DEFAULT (0),
        [ProblemasSeleccionados] [nvarchar](200) NULL,
        [RecordatorioMensual] [bit] NOT NULL CONSTRAINT [DF_SolicitudesSmokeDetector_RecordatorioMensual] DEFAULT (1),
        [RecordatorioBateriaAnual] [bit] NOT NULL CONSTRAINT [DF_SolicitudesSmokeDetector_RecordatorioBateriaAnual] DEFAULT (1),
        [RecordatorioReemplazo10Anos] [bit] NOT NULL CONSTRAINT [DF_SolicitudesSmokeDetector_RecordatorioReemplazo10Anos] DEFAULT (1),
        [RecordatorioRevisionEstacional] [bit] NOT NULL CONSTRAINT [DF_SolicitudesSmokeDetector_RecordatorioRevisionEstacional] DEFAULT (1),
        [TipoAccionAyuda] [nvarchar](20) NULL,
        [FechaInstalacionReferencia] [datetime2](7) NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesSmokeDetector_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesSmokeDetector_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesSmokeDetector] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesSmokeDetector]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesSmokeDetector_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesSmokeDetector]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesSmokeDetector_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    ALTER TABLE [dbo].[SolicitudesSmokeDetector]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesSmokeDetector_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesSmokeDetector_UserId_Priority_Estado]
        ON [dbo].[SolicitudesSmokeDetector] ([UserId], [HomeCarePriorityId], [Estado]);

    PRINT 'Table SolicitudesSmokeDetector created.';
END
GO

DECLARE @SdPriorityId INT = (
    SELECT TOP 1 Id FROM dbo.HomeCarePriorities
    WHERE Nombre = N'Smoke Detector'
    ORDER BY Id
);

IF @SdPriorityId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SmokeDetectorServicioLanding WHERE HomeCarePriorityId = @SdPriorityId)
    BEGIN
        INSERT INTO dbo.SmokeDetectorServicioLanding (
            HomeCarePriorityId, PageTitle, LandingTitulo, LandingSubtitulo, ImagenUrl,
            TrackItems, TrackDescriptions, TrackIconos, WhereTrackItems, WhereTrackIconos,
            ReminderBannerTexto, CtaTexto, Activo)
        VALUES (
            @SdPriorityId,
            N'Smoke / CO Check',
            N'Protect your home and the people in it.',
            N'Smoke and carbon monoxide alarms are your first line of defense. Regular checks keep them ready when it matters most.',
            N'/priority-smoke-detector.png',
            N'Test monthly|Battery check yearly|Replace alarm every 10 years',
            N'Press the test button to make sure your alarm is working.|Check and replace batteries at least once a year.|Alarms should be replaced 10 years from the install date.',
            N'fa-calendar|fa-battery-full|fa-rotate',
            N'Bedroom alarms|Hallway alarms|Living area alarms|CO combo units',
            N'fa-bed|fa-door-open|fa-couch|fa-circle',
            N'INDOR will remind you when it''s time to test, change batteries, or replace older alarms.',
            N'Start reminder setup',
            1);
        PRINT 'SmokeDetectorServicioLanding seeded for Smoke Detector.';
    END
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "Smoke Detector" not found.';
END
GO

PRINT 'Smoke Detector flow schema ready.';
GO
