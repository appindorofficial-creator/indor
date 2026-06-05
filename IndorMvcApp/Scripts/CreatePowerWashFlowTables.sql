/*
  Power Wash Exterior flow — landing + solicitudes + photos for Home Care Priorities.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.PowerWashServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PowerWashServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_PowerWashServicioLanding_PageTitle] DEFAULT (N'Power Wash Exterior'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [InfoBoxTexto] [nvarchar](500) NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [BestForItems] [nvarchar](500) NULL,
        [BestForIconos] [nvarchar](300) NULL,
        [PreviewTexto] [nvarchar](400) NULL,
        [TipConfirmacionTexto] [nvarchar](300) NULL,
        [InfoCondicionTexto] [nvarchar](400) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_PowerWashServicioLanding_CtaTexto] DEFAULT (N'Start exterior check'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_PowerWashServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_PowerWashServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_PowerWashServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_PowerWashServicioLanding_HomeCarePriorityId]
        ON dbo.PowerWashServicioLanding ([HomeCarePriorityId]);

    ALTER TABLE [dbo].[PowerWashServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_PowerWashServicioLanding_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    PRINT 'Table PowerWashServicioLanding created.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesPowerWash', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesPowerWash](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [AreasSeleccionadas] [nvarchar](200) NULL,
        [MaterialExterior] [nvarchar](30) NULL,
        [NumeroPisos] [nvarchar](10) NULL,
        [ProblemasSeleccionados] [nvarchar](200) NULL,
        [AreasDelicadas] [nvarchar](200) NULL,
        [AccesoGrifo] [nvarchar](10) NULL,
        [TimingPreferido] [nvarchar](20) NULL,
        [VentanaHorario] [nvarchar](20) NULL,
        [FechaPreferida] [date] NULL,
        [RecordatorioAnual] [bit] NOT NULL CONSTRAINT [DF_SolicitudesPowerWash_RecordatorioAnual] DEFAULT (0),
        [Notas] [nvarchar](300) NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesPowerWash_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesPowerWash_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesPowerWash] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesPowerWash]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesPowerWash_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesPowerWash]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesPowerWash_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    ALTER TABLE [dbo].[SolicitudesPowerWash]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesPowerWash_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesPowerWash_UserId_Priority_Estado]
        ON [dbo].[SolicitudesPowerWash] ([UserId], [HomeCarePriorityId], [Estado]);

    PRINT 'Table SolicitudesPowerWash created.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosPowerWash', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosPowerWash](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudPowerWashId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosPowerWash_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosPowerWash] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosPowerWash]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosPowerWash_SolicitudesPowerWash]
        FOREIGN KEY([SolicitudPowerWashId]) REFERENCES [dbo].[SolicitudesPowerWash] ([Id]) ON DELETE CASCADE;

    PRINT 'Table ArchivosPowerWash created.';
END
GO

DECLARE @PwPriorityId INT = (
    SELECT TOP 1 Id FROM dbo.HomeCarePriorities
    WHERE Nombre = N'Power wash exterior'
    ORDER BY Id
);

IF @PwPriorityId IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET LinkController = N'PowerWash',
        LinkAction = N'PowerWashService',
        LinkUrl = NULL,
        ImagenUrl = CASE
            WHEN ImagenUrl IS NULL OR ImagenUrl = N'/servicio5.jpeg' THEN N'/priority-power-wash-exterior.png'
            ELSE ImagenUrl
        END
    WHERE Id = @PwPriorityId;

    UPDATE dbo.PowerWashServicioLanding
    SET ImagenUrl = N'/priority-power-wash-exterior.png'
    WHERE HomeCarePriorityId = @PwPriorityId
      AND (ImagenUrl IS NULL OR ImagenUrl = N'/servicio5.jpeg');

    IF NOT EXISTS (SELECT 1 FROM dbo.PowerWashServicioLanding WHERE HomeCarePriorityId = @PwPriorityId)
    BEGIN
        INSERT INTO dbo.PowerWashServicioLanding (
            HomeCarePriorityId, PageTitle, LandingTitulo, LandingTagline, InfoBoxTexto, ImagenUrl,
            BestForItems, BestForIconos, PreviewTexto, TipConfirmacionTexto, InfoCondicionTexto, CtaTexto, Activo)
        VALUES (
            @PwPriorityId,
            N'Power Wash Exterior',
            N'Power Wash Exterior',
            N'Recommended every 1–2 years',
            N'This service helps remove dirt, algae, mildew, pollen, and surface buildup from the exterior of your home.',
            N'/priority-power-wash-exterior.png',
            N'Vinyl siding|Brick|Stucco|Driveway|Patio|Fence',
            N'fa-house|fa-table-cells|fa-braille|fa-road|fa-umbrella-beach|fa-grip-lines',
            N'We''ll use your answers to understand your surface type, condition, and access so we can recommend the right approach.',
            N'Power washing is commonly recommended every 1–2 years, or sooner if you notice mildew, pollen, or staining.',
            N'We use this to choose the safest wash pressure for your home.',
            N'Start exterior check',
            1);
        PRINT 'PowerWashServicioLanding seeded for Power wash exterior.';
    END
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "Power wash exterior" not found — run CreateHomeCarePrioritiesTables.sql first.';
END
GO

PRINT 'Power Wash Exterior flow schema ready.';
GO
