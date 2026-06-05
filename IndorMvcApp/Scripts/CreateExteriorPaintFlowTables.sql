/*
  Exterior Paint Review flow — landing + solicitudes + photos for Home Care Priorities.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.ExteriorPaintServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExteriorPaintServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_ExteriorPaintServicioLanding_PageTitle] DEFAULT (N'Exterior Paint Review'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](400) NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_ExteriorPaintServicioLanding_PrecioDesde] DEFAULT (0),
        [PrecioTexto] [nvarchar](120) NULL,
        [InfoBoxTexto] [nvarchar](400) NULL,
        [WhyItMattersItems] [nvarchar](500) NULL,
        [WhyItMattersIconos] [nvarchar](300) NULL,
        [NextStepsItems] [nvarchar](500) NULL,
        [NextStepsIconos] [nvarchar](300) NULL,
        [ReminderTexto] [nvarchar](300) NULL,
        [ResumenServicioTexto] [nvarchar](300) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_ExteriorPaintServicioLanding_CtaTexto] DEFAULT (N'Continue'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_ExteriorPaintServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_ExteriorPaintServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_ExteriorPaintServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_ExteriorPaintServicioLanding_HomeCarePriorityId]
        ON dbo.ExteriorPaintServicioLanding ([HomeCarePriorityId]);

    ALTER TABLE [dbo].[ExteriorPaintServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_ExteriorPaintServicioLanding_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    PRINT 'Table ExteriorPaintServicioLanding created.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesExteriorPaint', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesExteriorPaint](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [UltimaPintura] [nvarchar](20) NULL,
        [TipoSuperficie] [nvarchar](30) NULL,
        [MantenerMismoColor] [nvarchar](10) NULL,
        [ProblemasSeleccionados] [nvarchar](200) NULL,
        [AreasSeleccionadas] [nvarchar](200) NULL,
        [ActualizacionColor] [nvarchar](10) NULL,
        [LavadoPresionReciente] [nvarchar](10) NULL,
        [NumeroPisos] [nvarchar](10) NULL,
        [TimingPreferido] [nvarchar](20) NULL,
        [RecordatorioAnual] [bit] NOT NULL CONSTRAINT [DF_SolicitudesExteriorPaint_RecordatorioAnual] DEFAULT (0),
        [Notas] [nvarchar](300) NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesExteriorPaint_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesExteriorPaint_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesExteriorPaint] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesExteriorPaint]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesExteriorPaint_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesExteriorPaint]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesExteriorPaint_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    ALTER TABLE [dbo].[SolicitudesExteriorPaint]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesExteriorPaint_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesExteriorPaint_UserId_Priority_Estado]
        ON [dbo].[SolicitudesExteriorPaint] ([UserId], [HomeCarePriorityId], [Estado]);

    PRINT 'Table SolicitudesExteriorPaint created.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosExteriorPaint', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosExteriorPaint](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudExteriorPaintId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [CategoriaFoto] [nvarchar](20) NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosExteriorPaint_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosExteriorPaint] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosExteriorPaint]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosExteriorPaint_SolicitudesExteriorPaint]
        FOREIGN KEY([SolicitudExteriorPaintId]) REFERENCES [dbo].[SolicitudesExteriorPaint] ([Id]) ON DELETE CASCADE;

    PRINT 'Table ArchivosExteriorPaint created.';
END
GO

DECLARE @EpPriorityId INT = (
    SELECT TOP 1 Id FROM dbo.HomeCarePriorities
    WHERE Nombre = N'Exterior paint'
    ORDER BY Id
);

IF @EpPriorityId IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET LinkController = N'ExteriorPaint',
        LinkAction = N'ExteriorPaintReview',
        LinkUrl = NULL,
        ImagenUrl = CASE
            WHEN ImagenUrl IS NULL OR ImagenUrl IN (N'/servicio6.jpeg', N'/servicio10.jpeg', N'/servicio5.jpeg') THEN N'/priority-exterior-paint.png'
            ELSE ImagenUrl
        END
    WHERE Id = @EpPriorityId;

    UPDATE dbo.ExteriorPaintServicioLanding
    SET ImagenUrl = N'/priority-exterior-paint.png'
    WHERE HomeCarePriorityId = @EpPriorityId
      AND (ImagenUrl IS NULL OR ImagenUrl IN (N'/servicio6.jpeg', N'/servicio10.jpeg', N'/servicio5.jpeg'));

    IF NOT EXISTS (SELECT 1 FROM dbo.ExteriorPaintServicioLanding WHERE HomeCarePriorityId = @EpPriorityId)
    BEGIN
        INSERT INTO dbo.ExteriorPaintServicioLanding (
            HomeCarePriorityId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, PrecioTexto, InfoBoxTexto, WhyItMattersItems, WhyItMattersIconos,
            NextStepsItems, NextStepsIconos, ReminderTexto, ResumenServicioTexto, CtaTexto, Activo)
        VALUES (
            @EpPriorityId,
            N'Exterior Paint Review',
            N'Exterior Paint Review',
            N'Recommended every 5 years',
            N'Help us understand your exterior so we can schedule the right paint review.',
            N'/priority-exterior-paint.png',
            0,
            NULL,
            N'Paint sooner if you see peeling, fading, or damaged caulk.',
            N'Fresh exterior paint protects siding and trim|Annual visual checks help catch peeling and bad caulk early|A full repaint is often needed about every 5 years, depending on material and weather',
            N'fa-shield-halved|fa-magnifying-glass|fa-calendar',
            N'We''ll review your photos|We''ll confirm scope and surface type|We''ll help you plan timing and color options',
            N'fa-image|fa-clipboard-list|fa-paint-roller',
            N'Check paint condition every year',
            N'Exterior paint review and planning',
            N'Continue',
            1);
        PRINT 'ExteriorPaintServicioLanding seeded for Exterior paint.';
    END
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "Exterior paint" not found — run CreateHomeCarePrioritiesTables.sql first.';
END
GO

PRINT 'Exterior Paint flow schema ready.';
GO
