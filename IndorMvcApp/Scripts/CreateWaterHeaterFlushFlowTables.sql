/*
  Water Heater Flush flow — landing + solicitudes for Home Care Priorities.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.WaterHeaterFlushServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WaterHeaterFlushServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_WaterHeaterFlushServicioLanding_PageTitle] DEFAULT (N'Water Heater Flush'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](400) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_WaterHeaterFlushServicioLanding_PrecioDesde] DEFAULT (79),
        [PrecioTexto] [nvarchar](120) NULL,
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [PreviewItems] [nvarchar](500) NULL,
        [PreviewIconos] [nvarchar](300) NULL,
        [InfoBoxTexto] [nvarchar](400) NULL,
        [ResumenServicioTexto] [nvarchar](200) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_WaterHeaterFlushServicioLanding_CtaTexto] DEFAULT (N'Continue'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_WaterHeaterFlushServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_WaterHeaterFlushServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_WaterHeaterFlushServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_WaterHeaterFlushServicioLanding_HomeCarePriorityId]
        ON dbo.WaterHeaterFlushServicioLanding ([HomeCarePriorityId]);

    ALTER TABLE [dbo].[WaterHeaterFlushServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_WaterHeaterFlushServicioLanding_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    PRINT 'Table WaterHeaterFlushServicioLanding created.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesWaterHeaterFlush', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesWaterHeaterFlush](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [TipoCalentador] [nvarchar](15) NULL,
        [FuenteEnergia] [nvarchar](15) NULL,
        [NumeroSerie] [nvarchar](80) NULL,
        [SerialDesconocido] [bit] NOT NULL CONSTRAINT [DF_SolicitudesWaterHeaterFlush_SerialDesconocido] DEFAULT (0),
        [MarcaModelo] [nvarchar](80) NULL,
        [Ubicacion] [nvarchar](20) NULL,
        [UltimoFlush] [nvarchar](20) NULL,
        [SintomasSeleccionados] [nvarchar](200) NULL,
        [TipoServicio] [nvarchar](20) NULL,
        [RecordatorioAnual] [bit] NOT NULL CONSTRAINT [DF_SolicitudesWaterHeaterFlush_RecordatorioAnual] DEFAULT (0),
        [PreferenciaTiempo] [nvarchar](20) NULL,
        [FechaVisita] [date] NULL,
        [NotasAdicionales] [nvarchar](200) NULL,
        [TelefonoContacto] [nvarchar](30) NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesWaterHeaterFlush_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesWaterHeaterFlush_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesWaterHeaterFlush] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesWaterHeaterFlush]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesWaterHeaterFlush_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesWaterHeaterFlush]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesWaterHeaterFlush_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    ALTER TABLE [dbo].[SolicitudesWaterHeaterFlush]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesWaterHeaterFlush_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesWaterHeaterFlush_UserId_Priority_Estado]
        ON [dbo].[SolicitudesWaterHeaterFlush] ([UserId], [HomeCarePriorityId], [Estado]);

    PRINT 'Table SolicitudesWaterHeaterFlush created.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosWaterHeaterFlush', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosWaterHeaterFlush](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudWaterHeaterFlushId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosWaterHeaterFlush_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosWaterHeaterFlush] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosWaterHeaterFlush]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosWaterHeaterFlush_SolicitudesWaterHeaterFlush]
        FOREIGN KEY([SolicitudWaterHeaterFlushId]) REFERENCES [dbo].[SolicitudesWaterHeaterFlush] ([Id]) ON DELETE CASCADE;

    PRINT 'Table ArchivosWaterHeaterFlush created.';
END
GO

DECLARE @WhPriorityId INT = (
    SELECT TOP 1 Id FROM dbo.HomeCarePriorities
    WHERE Nombre = N'Water heater flush'
    ORDER BY Id
);

IF @WhPriorityId IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET LinkController = N'WaterHeaterFlush',
        LinkAction = N'WaterHeaterFlushService',
        LinkUrl = NULL,
        ImagenUrl = CASE
            WHEN ImagenUrl IS NULL OR ImagenUrl = N'/inspeccion4.jpeg' THEN N'/priority-water-heater-flush.png'
            ELSE ImagenUrl
        END
    WHERE Id = @WhPriorityId;

    UPDATE dbo.WaterHeaterFlushServicioLanding
    SET ImagenUrl = N'/priority-water-heater-flush.png'
    WHERE HomeCarePriorityId = @WhPriorityId
      AND (ImagenUrl IS NULL OR ImagenUrl = N'/inspeccion4.jpeg');

    IF NOT EXISTS (SELECT 1 FROM dbo.WaterHeaterFlushServicioLanding WHERE HomeCarePriorityId = @WhPriorityId)
    BEGIN
        INSERT INTO dbo.WaterHeaterFlushServicioLanding (
            HomeCarePriorityId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, PrecioTexto, IncluyeItems, IncluyeIconos, PreviewItems, PreviewIconos,
            InfoBoxTexto, ResumenServicioTexto, CtaTexto, Activo)
        VALUES (
            @WhPriorityId,
            N'Water Heater Flush',
            N'Water Heater Flush',
            N'Recommended yearly',
            N'Recommended yearly to keep your system clean and efficient.',
            N'/priority-water-heater-flush.png',
            79,
            N'From $79',
            N'Remove sediment buildup|Improve efficiency|Extend tank life',
            N'fa-water|fa-leaf|fa-shield-halved',
            N'Serial number|Last maintenance|Any symptoms|Preferred date',
            N'fa-barcode|fa-calendar|fa-circle-question|fa-calendar-check',
            N'Over time, sediment builds up at the bottom of your water heater tank. This can reduce efficiency, cause rumbling noises, and shorten the life of your system.',
            N'Annual flush + basic visual check',
            N'Continue',
            1);
        PRINT 'WaterHeaterFlushServicioLanding seeded for Water heater flush.';
    END
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "Water heater flush" not found — run CreateHomeCarePrioritiesTables.sql first.';
END
GO

PRINT 'Water Heater Flush flow schema ready.';
GO
