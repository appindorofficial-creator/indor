/*
  Cleaning flow — landing content + solicitudes + file uploads.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.CleaningServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CleaningServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_CleaningServicioLanding_PageTitle] DEFAULT (N'Cleaning Service'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](300) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_CleaningServicioLanding_PrecioDesde] DEFAULT (149),
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [BestForLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_CleaningServicioLanding_BestForLabel] DEFAULT (N'Best for'),
        [BestForOptions] [nvarchar](200) NOT NULL,
        [BestForIcons] [nvarchar](200) NULL,
        [BestForValues] [nvarchar](200) NOT NULL,
        [InfoBoxTitulo] [nvarchar](120) NULL,
        [InfoBoxTexto] [nvarchar](300) NULL,
        [CtaContinueTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_CleaningServicioLanding_CtaContinueTexto] DEFAULT (N'Continue'),
        [CtaUploadTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_CleaningServicioLanding_CtaUploadTexto] DEFAULT (N'Upload photos'),
        [PrecioBaseEstimado] [decimal](10,2) NOT NULL CONSTRAINT [DF_CleaningServicioLanding_PrecioBaseEstimado] DEFAULT (149),
        [DisclaimerTexto] [nvarchar](300) NULL,
        [Activo] [bit] NOT NULL CONSTRAINT [DF_CleaningServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_CleaningServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_CleaningServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_CleaningServicioLanding_MovingSetupServicioId]
        ON dbo.CleaningServicioLanding ([MovingSetupServicioId]);

    ALTER TABLE [dbo].[CleaningServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_CleaningServicioLanding_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    PRINT 'Table CleaningServicioLanding created.';
END
ELSE
BEGIN
    PRINT 'Table CleaningServicioLanding already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesCleaning', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesCleaning](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [TipoLimpieza] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesCleaning_TipoLimpieza] DEFAULT (N'MoveIn'),
        [TipoPropiedad] [nvarchar](30) NULL,
        [NumeroHabitaciones] [nvarchar](20) NULL,
        [NumeroBanos] [nvarchar](20) NULL,
        [CondicionActual] [nvarchar](30) NULL,
        [FechaServicio] [date] NULL,
        [VentanaHorario] [nvarchar](60) NULL,
        [AreasPrioridad] [nvarchar](400) NULL,
        [TareasExtra] [nvarchar](500) NULL,
        [SuministrosNecesarios] [nvarchar](20) NULL,
        [MetodoAcceso] [nvarchar](30) NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesCleaning_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesCleaning_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesCleaning] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesCleaning]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesCleaning_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesCleaning]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesCleaning_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesCleaning]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesCleaning_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesCleaning_UserId_Servicio_Estado]
        ON [dbo].[SolicitudesCleaning] ([UserId], [MovingSetupServicioId], [Estado]);

    PRINT 'Table SolicitudesCleaning created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesCleaning already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosCleaning', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosCleaning](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudCleaningId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosCleaning_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosCleaning] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosCleaning]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosCleaning_SolicitudesCleaning]
        FOREIGN KEY([SolicitudCleaningId]) REFERENCES [dbo].[SolicitudesCleaning] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ArchivosCleaning]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosCleaning_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    PRINT 'Table ArchivosCleaning created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosCleaning already exists.';
END
GO

DECLARE @CleaningServicioId INT = (
    SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Cleaning' ORDER BY Id
);

IF @CleaningServicioId IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'Cleaning',
        LinkAction = N'CleaningService',
        LinkRouteId = Id
    WHERE Id = @CleaningServicioId
      AND (LinkController IS NULL OR LinkController = N'MovingSetup');

    IF NOT EXISTS (SELECT 1 FROM dbo.CleaningServicioLanding WHERE MovingSetupServicioId = @CleaningServicioId)
    BEGIN
        INSERT INTO dbo.CleaningServicioLanding (
            MovingSetupServicioId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, IncluyeItems, IncluyeIconos,
            BestForLabel, BestForOptions, BestForIcons, BestForValues,
            InfoBoxTitulo, InfoBoxTexto,
            CtaContinueTexto, CtaUploadTexto, PrecioBaseEstimado, DisclaimerTexto, Activo)
        VALUES (
            @CleaningServicioId,
            N'Cleaning Service',
            N'Move-In / Move-Out Cleaning',
            N'A fresh start before or after your move.',
            N'Professional cleaning for homes, apartments, and condos before move-in or after move-out.',
            N'/inspeccion2.jpeg',
            149,
            N'Kitchens|Bathrooms|Floors|Dusting|Trash removal',
            N'fa-check|fa-check|fa-check|fa-check|fa-check',
            N'Best for',
            N'Move-In|Move-Out|Occupied home|Empty property',
            N'fa-house-circle-check|fa-house-circle-xmark|fa-users|fa-house',
            N'MoveIn|MoveOut|OccupiedHome|EmptyProperty',
            N'Tell us a few details',
            N'Choose the cleaning type, property size, and any extra tasks so we can send the right crew.',
            N'Continue', N'Upload photos',
            149,
            N'Final price may adjust if the home condition is different on arrival.',
            1);
        PRINT 'CleaningServicioLanding seeded for Cleaning service.';
    END
END
GO

PRINT 'Cleaning flow schema ready.';
GO
