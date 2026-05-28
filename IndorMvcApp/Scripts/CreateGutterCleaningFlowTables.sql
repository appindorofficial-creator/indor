/*
  Gutter Cleaning flow — landing + solicitudes + photos for Home Care Priorities.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.GutterCleaningServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[GutterCleaningServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_GutterCleaningServicioLanding_PageTitle] DEFAULT (N'Gutter Cleaning'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [InfoBoxTexto] [nvarchar](500) NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [WhyItMattersItems] [nvarchar](500) NULL,
        [WhyItMattersIconos] [nvarchar](300) NULL,
        [NextStepsItems] [nvarchar](500) NULL,
        [NextStepsIconos] [nvarchar](300) NULL,
        [RecommendedTimingItems] [nvarchar](500) NULL,
        [RecommendedTimingIconos] [nvarchar](300) NULL,
        [InfoConfirmacionTexto] [nvarchar](300) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_GutterCleaningServicioLanding_CtaTexto] DEFAULT (N'Continue'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_GutterCleaningServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_GutterCleaningServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_GutterCleaningServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_GutterCleaningServicioLanding_HomeCarePriorityId]
        ON dbo.GutterCleaningServicioLanding ([HomeCarePriorityId]);

    ALTER TABLE [dbo].[GutterCleaningServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_GutterCleaningServicioLanding_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    PRINT 'Table GutterCleaningServicioLanding created.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesGutterCleaning', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesGutterCleaning](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [TipoAccionInicial] [nvarchar](20) NULL,
        [NumeroPisos] [nvarchar](10) NULL,
        [TipoCanaletas] [nvarchar](20) NULL,
        [ProtectorCanaletas] [nvarchar](10) NULL,
        [UltimaLimpieza] [nvarchar](20) NULL,
        [CantidadBajantes] [int] NULL,
        [ProblemasSeleccionados] [nvarchar](200) NULL,
        [AreaProblema] [nvarchar](20) NULL,
        [ObjetivoHoy] [nvarchar](20) NULL,
        [PreferenciaRecordatorio] [nvarchar](20) NULL,
        [FechaRecordatorioPersonalizada] [date] NULL,
        [FechaVisitaPreferida] [date] NULL,
        [Notas] [nvarchar](300) NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [RecordatorioPrimaveraOtono] [bit] NOT NULL CONSTRAINT [DF_SolicitudesGutterCleaning_RecordatorioPrimaveraOtono] DEFAULT (0),
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesGutterCleaning_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesGutterCleaning_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesGutterCleaning] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesGutterCleaning]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesGutterCleaning_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesGutterCleaning]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesGutterCleaning_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    ALTER TABLE [dbo].[SolicitudesGutterCleaning]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesGutterCleaning_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesGutterCleaning_UserId_Priority_Estado]
        ON [dbo].[SolicitudesGutterCleaning] ([UserId], [HomeCarePriorityId], [Estado]);

    PRINT 'Table SolicitudesGutterCleaning created.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosGutterCleaning', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosGutterCleaning](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudGutterCleaningId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosGutterCleaning_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosGutterCleaning] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosGutterCleaning]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosGutterCleaning_SolicitudesGutterCleaning]
        FOREIGN KEY([SolicitudGutterCleaningId]) REFERENCES [dbo].[SolicitudesGutterCleaning] ([Id]) ON DELETE CASCADE;

    PRINT 'Table ArchivosGutterCleaning created.';
END
GO

DECLARE @GcPriorityId INT = (
    SELECT TOP 1 Id FROM dbo.HomeCarePriorities
    WHERE Nombre = N'Gutter cleaning'
    ORDER BY Id
);

IF @GcPriorityId IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET LinkController = N'GutterCleaning',
        LinkAction = N'GutterCleaningService',
        LinkUrl = NULL
    WHERE Id = @GcPriorityId;

    IF NOT EXISTS (SELECT 1 FROM dbo.GutterCleaningServicioLanding WHERE HomeCarePriorityId = @GcPriorityId)
    BEGIN
        INSERT INTO dbo.GutterCleaningServicioLanding (
            HomeCarePriorityId, PageTitle, LandingTitulo, LandingTagline, InfoBoxTexto, ImagenUrl,
            WhyItMattersItems, WhyItMattersIconos, NextStepsItems, NextStepsIconos,
            RecommendedTimingItems, RecommendedTimingIconos, InfoConfirmacionTexto, CtaTexto, Activo)
        VALUES (
            @GcPriorityId,
            N'Gutter Cleaning',
            N'Gutter Cleaning',
            N'Recommended twice a year',
            N'Gutters should be cleaned in the spring and fall to help prevent clogs, overflow, fascia damage, foundation issues, and water intrusion.',
            N'/priority-gutter-cleaning.png',
            N'Prevents overflow|Helps protect roof edges|Keeps downspouts clear|Reduces water around foundation',
            N'fa-droplet|fa-house-chimney|fa-faucet|fa-house-flood-water',
            N'We saved your reminder schedule|A pro can review your request|You can update this anytime in My Home',
            N'fa-bell|fa-user-check|fa-house',
            N'Spring cleaning: March – May|Fall cleaning: September – November',
            N'fa-seedling|fa-leaf',
            N'Routine gutter cleaning helps prevent overflow, roof damage, and foundation issues.',
            N'Continue',
            1);
        PRINT 'GutterCleaningServicioLanding seeded for Gutter cleaning.';
    END
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "Gutter cleaning" not found — run CreateHomeCarePrioritiesTables.sql first.';
END
GO

PRINT 'Gutter Cleaning flow schema ready.';
GO
