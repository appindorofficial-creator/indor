/*
  Furniture & Assembly flow — landing content + solicitudes + file uploads.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.FurnitureAssemblyServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[FurnitureAssemblyServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_PageTitle] DEFAULT (N'Furniture & Assembly'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingSubtitulo] [nvarchar](300) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_PrecioDesde] DEFAULT (89),
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [BadgesTexto] [nvarchar](300) NULL,
        [BadgesIconos] [nvarchar](200) NULL,
        [EstimatedTimeLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_EstimatedTimeLabel] DEFAULT (N'Estimated time'),
        [EstimatedTimeValue] [nvarchar](60) NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_EstimatedTimeValue] DEFAULT (N'1-3 hours'),
        [EstimatedTimeNote] [nvarchar](120) NULL,
        [BestForLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_BestForLabel] DEFAULT (N'Best for'),
        [BestForValue] [nvarchar](120) NOT NULL,
        [BestForNote] [nvarchar](120) NULL,
        [CtaContinueTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_CtaContinueTexto] DEFAULT (N'Continue'),
        [CtaUploadTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_CtaUploadTexto] DEFAULT (N'Upload photos or manuals'),
        [PrecioBaseEstimado] [decimal](10,2) NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_PrecioBaseEstimado] DEFAULT (89),
        [DisclaimerTexto] [nvarchar](300) NULL,
        [Activo] [bit] NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_FurnitureAssemblyServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_FurnitureAssemblyServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_FurnitureAssemblyServicioLanding_MovingSetupServicioId]
        ON dbo.FurnitureAssemblyServicioLanding ([MovingSetupServicioId]);

    ALTER TABLE [dbo].[FurnitureAssemblyServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_FurnitureAssemblyServicioLanding_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    PRINT 'Table FurnitureAssemblyServicioLanding created.';
END
ELSE
BEGIN
    PRINT 'Table FurnitureAssemblyServicioLanding already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesFurnitureAssembly', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesFurnitureAssembly](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [TiposMueble] [nvarchar](400) NULL,
        [CantidadItems] [nvarchar](20) NULL,
        [CondicionItems] [nvarchar](30) NULL,
        [AnclajePared] [nvarchar](20) NULL,
        [Habitacion] [nvarchar](30) NULL,
        [DetallesAcceso] [nvarchar](300) NULL,
        [AyudaMover] [nvarchar](20) NULL,
        [FechaServicio] [date] NULL,
        [VentanaHorario] [nvarchar](30) NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesFurnitureAssembly_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesFurnitureAssembly_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesFurnitureAssembly] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesFurnitureAssembly]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesFurnitureAssembly_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesFurnitureAssembly]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesFurnitureAssembly_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesFurnitureAssembly]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesFurnitureAssembly_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesFurnitureAssembly_UserId_Servicio_Estado]
        ON [dbo].[SolicitudesFurnitureAssembly] ([UserId], [MovingSetupServicioId], [Estado]);

    PRINT 'Table SolicitudesFurnitureAssembly created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesFurnitureAssembly already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosFurnitureAssembly', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosFurnitureAssembly](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudFurnitureAssemblyId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [CategoriaArchivo] [nvarchar](40) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosFurnitureAssembly_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosFurnitureAssembly] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosFurnitureAssembly]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosFurnitureAssembly_SolicitudesFurnitureAssembly]
        FOREIGN KEY([SolicitudFurnitureAssemblyId]) REFERENCES [dbo].[SolicitudesFurnitureAssembly] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ArchivosFurnitureAssembly]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosFurnitureAssembly_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    PRINT 'Table ArchivosFurnitureAssembly created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosFurnitureAssembly already exists.';
END
GO

DECLARE @FurnitureServicioId INT = (
    SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Furniture & Assembly' ORDER BY Id
);

IF @FurnitureServicioId IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'FurnitureAssembly',
        LinkAction = N'FurnitureAssemblyService',
        LinkRouteId = Id
    WHERE Id = @FurnitureServicioId
      AND (LinkController IS NULL OR LinkController = N'MovingSetup');

    IF NOT EXISTS (SELECT 1 FROM dbo.FurnitureAssemblyServicioLanding WHERE MovingSetupServicioId = @FurnitureServicioId)
    BEGIN
        INSERT INTO dbo.FurnitureAssemblyServicioLanding (
            MovingSetupServicioId, PageTitle, LandingTitulo, LandingSubtitulo, ImagenUrl,
            PrecioDesde, IncluyeItems, IncluyeIconos, BadgesTexto, BadgesIconos,
            EstimatedTimeLabel, EstimatedTimeValue, EstimatedTimeNote,
            BestForLabel, BestForValue, BestForNote,
            CtaContinueTexto, CtaUploadTexto, PrecioBaseEstimado, DisclaimerTexto, Activo)
        VALUES (
            @FurnitureServicioId,
            N'Furniture & Assembly',
            N'Furniture & Assembly',
            N'Assembly help for move-in and home setup.',
            N'/inspeccion2.jpeg',
            89,
            N'Bed frames|Desks|Dining tables|Bookshelves|TV stands|Dressers|Chairs|And more',
            N'fa-check|fa-check|fa-check|fa-check|fa-check|fa-check|fa-check|fa-check',
            N'Fast booking|Trusted pros|Transparent pricing',
            N'fa-bolt|fa-shield-check|fa-tags',
            N'Estimated time', N'1-3 hours', N'Depending on items',
            N'Best for', N'Move-in, Setup, Reorganization', N'New home or refresh',
            N'Continue', N'Upload photos or manuals',
            89,
            N'Photos of the boxes, the room, and manuals help your provider speed up the service.',
            1);
        PRINT 'FurnitureAssemblyServicioLanding seeded.';
    END
END
GO

PRINT 'Furniture & Assembly flow schema ready.';
GO
