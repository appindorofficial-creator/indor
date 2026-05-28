/*
  Safe Air 365 microservice flow — landing content + solicitudes + photo uploads.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SafeAirServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SafeAirServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MicroservicioId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_SafeAirServicioLanding_PageTitle] DEFAULT (N'Safe Air 365'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](400) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_SafeAirServicioLanding_PrecioDesde] DEFAULT (49),
        [PrecioTexto] [nvarchar](120) NULL,
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [InfoBoxTexto] [nvarchar](400) NULL,
        [CtaScheduleTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_SafeAirServicioLanding_CtaScheduleTexto] DEFAULT (N'Schedule with INDOR'),
        [CtaChangedTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_SafeAirServicioLanding_CtaChangedTexto] DEFAULT (N'I changed it myself'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_SafeAirServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SafeAirServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_SafeAirServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_SafeAirServicioLanding_MicroservicioId]
        ON dbo.SafeAirServicioLanding ([MicroservicioId]);

    ALTER TABLE [dbo].[SafeAirServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_SafeAirServicioLanding_Microservicios]
        FOREIGN KEY([MicroservicioId]) REFERENCES [dbo].[Microservicios] ([Id]);

    PRINT 'Table SafeAirServicioLanding created.';
END
ELSE
BEGIN
    PRINT 'Table SafeAirServicioLanding already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesSafeAir', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesSafeAir](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MicroservicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [TipoNecesidad] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesSafeAir_TipoNecesidad] DEFAULT (N'IndorReplaces'),
        [CantidadFiltros] [nvarchar](10) NULL,
        [FiltroAncho] [decimal](5,2) NULL,
        [FiltroAlto] [decimal](5,2) NULL,
        [FiltroProfundidad] [decimal](5,2) NULL,
        [FiltroTamanioDesconocido] [bit] NOT NULL CONSTRAINT [DF_SolicitudesSafeAir_FiltroTamanioDesconocido] DEFAULT (0),
        [UbicacionFiltro] [nvarchar](30) NULL,
        [ProveedorFiltro] [nvarchar](30) NULL,
        [RecordatorioActivo] [bit] NOT NULL CONSTRAINT [DF_SolicitudesSafeAir_RecordatorioActivo] DEFAULT (1),
        [VentanaTiempo] [nvarchar](30) NULL,
        [DetallesAcceso] [nvarchar](120) NULL,
        [NotasAcceso] [nvarchar](500) NULL,
        [FechaProximoRecordatorio] [date] NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesSafeAir_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesSafeAir_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesSafeAir] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesSafeAir]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesSafeAir_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesSafeAir]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesSafeAir_Microservicios]
        FOREIGN KEY([MicroservicioId]) REFERENCES [dbo].[Microservicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesSafeAir]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesSafeAir_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesSafeAir_UserId_Microservicio_Estado]
        ON [dbo].[SolicitudesSafeAir] ([UserId], [MicroservicioId], [Estado]);

    PRINT 'Table SolicitudesSafeAir created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesSafeAir already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosSafeAir', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosSafeAir](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudSafeAirId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosSafeAir_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosSafeAir] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosSafeAir]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosSafeAir_SolicitudesSafeAir]
        FOREIGN KEY([SolicitudSafeAirId]) REFERENCES [dbo].[SolicitudesSafeAir] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ArchivosSafeAir]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosSafeAir_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    PRINT 'Table ArchivosSafeAir created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosSafeAir already exists.';
END
GO

DECLARE @SafeAirMicroId INT = (
    SELECT TOP 1 Id FROM dbo.Microservicios WHERE Nombre = N'Safe Air 365' ORDER BY Id
);

IF @SafeAirMicroId IS NULL
BEGIN
    SET @SafeAirMicroId = 1;
END

IF @SafeAirMicroId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SafeAirServicioLanding WHERE MicroservicioId = @SafeAirMicroId)
    BEGIN
        INSERT INTO dbo.SafeAirServicioLanding (
            MicroservicioId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, PrecioTexto, IncluyeItems, IncluyeIconos, InfoBoxTexto,
            CtaScheduleTexto, CtaChangedTexto, Activo)
        VALUES (
            @SafeAirMicroId,
            N'Safe Air 365',
            N'Safe Air 365',
            N'Change it yourself or let INDOR handle it.',
            N'Never forget a filter change again. We help you track sizes, send reminders, and book replacements when you need them.',
            N'/aire.jpeg',
            49,
            N'From $49 for provider replacement',
            N'Filter reminders|Size tracking|DIY or pro service|Basic airflow check',
            N'fa-bell|fa-ruler|fa-user-hard-hat|fa-wind',
            N'Don''t know your filter size? Add a photo or choose "I don''t know" and your provider can verify it.',
            N'Schedule with INDOR',
            N'I changed it myself',
            1);
        PRINT 'SafeAirServicioLanding seeded for Safe Air 365.';
    END
END
GO

PRINT 'Safe Air 365 flow schema ready.';
GO
