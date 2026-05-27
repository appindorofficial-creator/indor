/*
  TV Wall Mounting flow — landing content + solicitudes + file uploads.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.TvWallMountingServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TvWallMountingServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_PageTitle] DEFAULT (N'Service Detail'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](300) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_PrecioDesde] DEFAULT (129),
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [BestForLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_BestForLabel] DEFAULT (N'Best for'),
        [BestForOptions] [nvarchar](200) NOT NULL,
        [BestForIcons] [nvarchar](200) NULL,
        [BestForValues] [nvarchar](200) NOT NULL,
        [InfoBoxTexto] [nvarchar](300) NULL,
        [EstimatedTimeLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_EstimatedTimeLabel] DEFAULT (N'Estimated time'),
        [EstimatedTimeValue] [nvarchar](60) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_EstimatedTimeValue] DEFAULT (N'60-90 min'),
        [BestTimingLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_BestTimingLabel] DEFAULT (N'Best recommendation'),
        [BestTimingValue] [nvarchar](120) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_BestTimingValue] DEFAULT (N'After move-in or room setup'),
        [CtaContinueTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_CtaContinueTexto] DEFAULT (N'Continue'),
        [CtaUploadTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_CtaUploadTexto] DEFAULT (N'Upload photos first'),
        [PrecioBaseEstimado] [decimal](10,2) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_PrecioBaseEstimado] DEFAULT (129),
        [DisclaimerTexto] [nvarchar](300) NULL,
        [Activo] [bit] NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_TvWallMountingServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_TvWallMountingServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_TvWallMountingServicioLanding_MovingSetupServicioId]
        ON dbo.TvWallMountingServicioLanding ([MovingSetupServicioId]);

    ALTER TABLE [dbo].[TvWallMountingServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_TvWallMountingServicioLanding_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    PRINT 'Table TvWallMountingServicioLanding created.';
END
ELSE
BEGIN
    PRINT 'Table TvWallMountingServicioLanding already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesTvWallMounting', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesTvWallMounting](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [TipoSolicitud] [nvarchar](30) NULL,
        [TamanoTv] [nvarchar](30) NULL,
        [CantidadTvs] [nvarchar](20) NULL,
        [Habitacion] [nvarchar](30) NULL,
        [TipoPared] [nvarchar](30) NULL,
        [TieneSoporte] [nvarchar](30) NULL,
        [ConfiguracionCables] [nvarchar](30) NULL,
        [TomaCercana] [nvarchar](20) NULL,
        [MontajePrevio] [nvarchar](20) NULL,
        [DetallesAcceso] [nvarchar](30) NULL,
        [VentanaHorario] [nvarchar](30) NULL,
        [FechaServicio] [date] NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesTvWallMounting_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesTvWallMounting_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesTvWallMounting] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesTvWallMounting]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesTvWallMounting_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesTvWallMounting]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesTvWallMounting_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesTvWallMounting]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesTvWallMounting_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesTvWallMounting_UserId_Servicio_Estado]
        ON [dbo].[SolicitudesTvWallMounting] ([UserId], [MovingSetupServicioId], [Estado]);

    PRINT 'Table SolicitudesTvWallMounting created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesTvWallMounting already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosTvWallMounting', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosTvWallMounting](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudTvWallMountingId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [CategoriaArchivo] [nvarchar](40) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosTvWallMounting_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosTvWallMounting] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosTvWallMounting]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosTvWallMounting_SolicitudesTvWallMounting]
        FOREIGN KEY([SolicitudTvWallMountingId]) REFERENCES [dbo].[SolicitudesTvWallMounting] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ArchivosTvWallMounting]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosTvWallMounting_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    PRINT 'Table ArchivosTvWallMounting created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosTvWallMounting already exists.';
END
GO

DECLARE @TvServicioId INT = (
    SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'TV Wall Mounting' ORDER BY Id
);

IF @TvServicioId IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'TvWallMounting',
        LinkAction = N'TvWallMountingService',
        LinkRouteId = Id
    WHERE Id = @TvServicioId
      AND (LinkController IS NULL OR LinkController = N'MovingSetup');

    IF NOT EXISTS (SELECT 1 FROM dbo.TvWallMountingServicioLanding WHERE MovingSetupServicioId = @TvServicioId)
    BEGIN
        INSERT INTO dbo.TvWallMountingServicioLanding (
            MovingSetupServicioId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, IncluyeItems, IncluyeIconos,
            BestForLabel, BestForOptions, BestForIcons, BestForValues,
            InfoBoxTexto, EstimatedTimeLabel, EstimatedTimeValue, BestTimingLabel, BestTimingValue,
            CtaContinueTexto, CtaUploadTexto, PrecioBaseEstimado, DisclaimerTexto, Activo)
        VALUES (
            @TvServicioId,
            N'Service Detail',
            N'TV Wall Mounting',
            N'Secure, clean installation for your new space.',
            N'Professional TV mounting with clean setup and basic safety check.',
            N'/inspeccion2.jpeg',
            129,
            N'TV mounting|Bracket alignment|Cable organization|Basic safety check',
            N'fa-tv|fa-grip-lines-vertical|fa-plug|fa-shield-check',
            N'Best for',
            N'Apartments|Homes|Offices|Move-ins',
            N'fa-building|fa-house|fa-briefcase|fa-box-open',
            N'Apartments|Homes|Offices|MoveIns',
            N'Great for new homes, re-mounting, or setting up entertainment spaces. Final pricing may vary based on wall type and cable concealment.',
            N'Estimated time', N'60-90 min', N'Best recommendation', N'After move-in or room setup',
            N'Continue', N'Upload photos first',
            129,
            N'Your installer will review any uploaded photos before arrival.',
            1);
        PRINT 'TvWallMountingServicioLanding seeded.';
    END
END
GO

PRINT 'TV Wall Mounting flow schema ready.';
GO
