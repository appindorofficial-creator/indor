/*
  Packing Help flow — landing content + solicitudes + file uploads.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.PackingServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PackingServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_PackingServicioLanding_PageTitle] DEFAULT (N'Service Detail'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](300) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_PackingServicioLanding_PrecioDesde] DEFAULT (89),
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [BestForLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_PackingServicioLanding_BestForLabel] DEFAULT (N'Best for'),
        [BestForOptions] [nvarchar](200) NOT NULL,
        [BestForIcons] [nvarchar](200) NULL,
        [BestForValues] [nvarchar](200) NOT NULL,
        [InfoBoxTexto] [nvarchar](300) NULL,
        [EstimatedTimeLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_PackingServicioLanding_EstimatedTimeLabel] DEFAULT (N'Estimated time'),
        [EstimatedTimeValue] [nvarchar](60) NOT NULL CONSTRAINT [DF_PackingServicioLanding_EstimatedTimeValue] DEFAULT (N'2-5 hrs'),
        [BestTimingLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_PackingServicioLanding_BestTimingLabel] DEFAULT (N'Best timing'),
        [BestTimingValue] [nvarchar](120) NOT NULL CONSTRAINT [DF_PackingServicioLanding_BestTimingValue] DEFAULT (N'1-3 days before moving'),
        [CtaContinueTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_PackingServicioLanding_CtaContinueTexto] DEFAULT (N'Continue'),
        [CtaUploadTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_PackingServicioLanding_CtaUploadTexto] DEFAULT (N'Upload photos or list'),
        [PrecioBaseEstimado] [decimal](10,2) NOT NULL CONSTRAINT [DF_PackingServicioLanding_PrecioBaseEstimado] DEFAULT (89),
        [DisclaimerTexto] [nvarchar](300) NULL,
        [Activo] [bit] NOT NULL CONSTRAINT [DF_PackingServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_PackingServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_PackingServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_PackingServicioLanding_MovingSetupServicioId]
        ON dbo.PackingServicioLanding ([MovingSetupServicioId]);

    ALTER TABLE [dbo].[PackingServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_PackingServicioLanding_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    PRINT 'Table PackingServicioLanding created.';
END
ELSE
BEGIN
    PRINT 'Table PackingServicioLanding already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesPacking', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesPacking](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [TipoEmpaque] [nvarchar](30) NULL,
        [CuandoMudanza] [nvarchar](30) NULL,
        [TipoPropiedad] [nvarchar](30) NULL,
        [TamanoHogar] [nvarchar](30) NULL,
        [FechaServicio] [date] NULL,
        [VentanaHorario] [nvarchar](60) NULL,
        [HabitacionesEmpacar] [nvarchar](400) NULL,
        [ItemsEspeciales] [nvarchar](400) NULL,
        [SuministrosNecesarios] [nvarchar](300) NULL,
        [DetallesAcceso] [nvarchar](300) NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesPacking_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesPacking_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesPacking] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesPacking]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesPacking_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesPacking]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesPacking_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesPacking]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesPacking_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesPacking_UserId_Servicio_Estado]
        ON [dbo].[SolicitudesPacking] ([UserId], [MovingSetupServicioId], [Estado]);

    PRINT 'Table SolicitudesPacking created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesPacking already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosPacking', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosPacking](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudPackingId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosPacking_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosPacking] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosPacking]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosPacking_SolicitudesPacking]
        FOREIGN KEY([SolicitudPackingId]) REFERENCES [dbo].[SolicitudesPacking] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ArchivosPacking]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosPacking_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    PRINT 'Table ArchivosPacking created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosPacking already exists.';
END
GO

DECLARE @PackingServicioId INT = (
    SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Packing Help' ORDER BY Id
);

IF @PackingServicioId IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'Packing',
        LinkAction = N'PackingService',
        LinkRouteId = Id
    WHERE Id = @PackingServicioId
      AND (LinkController IS NULL OR LinkController = N'MovingSetup');

    IF NOT EXISTS (SELECT 1 FROM dbo.PackingServicioLanding WHERE MovingSetupServicioId = @PackingServicioId)
    BEGIN
        INSERT INTO dbo.PackingServicioLanding (
            MovingSetupServicioId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, IncluyeItems, IncluyeIconos,
            BestForLabel, BestForOptions, BestForIcons, BestForValues,
            InfoBoxTexto, EstimatedTimeLabel, EstimatedTimeValue, BestTimingLabel, BestTimingValue,
            CtaContinueTexto, CtaUploadTexto, PrecioBaseEstimado, DisclaimerTexto, Activo)
        VALUES (
            @PackingServicioId,
            N'Service Detail',
            N'Packing Help',
            N'Pack smarter, move easier.',
            N'Our pros help you organize, box, label, and protect your belongings so everything arrives safely and stress-free.',
            N'/inspeccion2.jpeg',
            89,
            N'Boxes & supplies guidance|Room-by-room packing|Fragile item protection|Labeling support',
            N'fa-check|fa-check|fa-check|fa-check',
            N'Best for',
            N'Move-out|Busy families|Apartments|Large homes',
            N'fa-house-circle-xmark|fa-users|fa-building|fa-house-user',
            N'MoveOut|BusyFamilies|Apartments|LargeHomes',
            N'Packing materials can be requested during booking. Final pricing may vary based on scope and materials.',
            N'Estimated time', N'2-5 hrs', N'Best timing', N'1-3 days before moving',
            N'Continue', N'Upload photos or list',
            89,
            N'A provider may contact you to confirm supplies and timing.',
            1);
        PRINT 'PackingServicioLanding seeded for Packing Help service.';
    END
END
GO

PRINT 'Packing flow schema ready.';
GO
