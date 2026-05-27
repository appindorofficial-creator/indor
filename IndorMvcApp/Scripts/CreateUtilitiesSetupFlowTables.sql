/*
  Utilities Setup flow — internet catalog, solicitudes, utility contacts.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.UtilitiesSetupProveedorInternet', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UtilitiesSetupProveedorInternet](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Codigo] [nvarchar](40) NOT NULL,
        [Nombre] [nvarchar](80) NOT NULL,
        [Etiqueta] [nvarchar](40) NULL,
        [Velocidad] [nvarchar](60) NULL,
        [DetalleExtra] [nvarchar](120) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_UtilitiesSetupProveedorInternet_PrecioDesde] DEFAULT (0),
        [Orden] [int] NOT NULL CONSTRAINT [DF_UtilitiesSetupProveedorInternet_Orden] DEFAULT (0),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_UtilitiesSetupProveedorInternet_Activo] DEFAULT (1),
        CONSTRAINT [PK_UtilitiesSetupProveedorInternet] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_UtilitiesSetupProveedorInternet_Codigo]
        ON dbo.UtilitiesSetupProveedorInternet ([Codigo]);

    PRINT 'Table UtilitiesSetupProveedorInternet created.';
END
ELSE
BEGIN
    PRINT 'Table UtilitiesSetupProveedorInternet already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesUtilitiesSetup', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesUtilitiesSetup](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [ServiciosConectar] [nvarchar](120) NULL,
        [FechaServicio] [date] NULL,
        [PreferenciaContacto] [nvarchar](30) NULL,
        [ProveedorInternetId] [int] NULL,
        [OpcionCable] [nvarchar](30) NULL,
        [OmitirInternet] [bit] NOT NULL CONSTRAINT [DF_SolicitudesUtilitiesSetup_OmitirInternet] DEFAULT (0),
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesUtilitiesSetup_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesUtilitiesSetup_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesUtilitiesSetup] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesUtilitiesSetup]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesUtilitiesSetup_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesUtilitiesSetup]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesUtilitiesSetup_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesUtilitiesSetup]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesUtilitiesSetup_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    ALTER TABLE [dbo].[SolicitudesUtilitiesSetup]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesUtilitiesSetup_ProveedorInternet]
        FOREIGN KEY([ProveedorInternetId]) REFERENCES [dbo].[UtilitiesSetupProveedorInternet] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesUtilitiesSetup_UserId_Servicio_Estado]
        ON [dbo].[SolicitudesUtilitiesSetup] ([UserId], [MovingSetupServicioId], [Estado]);

    PRINT 'Table SolicitudesUtilitiesSetup created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesUtilitiesSetup already exists.';
END
GO

IF OBJECT_ID(N'dbo.UtilitiesSetupContactos', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UtilitiesSetupContactos](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudUtilitiesSetupId] [int] NOT NULL,
        [TipoUtilidad] [nvarchar](30) NOT NULL,
        [Nombre] [nvarchar](120) NOT NULL,
        [Telefono] [nvarchar](40) NULL,
        [Website] [nvarchar](300) NULL,
        [IconoClase] [nvarchar](50) NULL,
        [Orden] [int] NOT NULL CONSTRAINT [DF_UtilitiesSetupContactos_Orden] DEFAULT (0),
        CONSTRAINT [PK_UtilitiesSetupContactos] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[UtilitiesSetupContactos]
        WITH CHECK ADD CONSTRAINT [FK_UtilitiesSetupContactos_SolicitudesUtilitiesSetup]
        FOREIGN KEY([SolicitudUtilitiesSetupId]) REFERENCES [dbo].[SolicitudesUtilitiesSetup] ([Id]) ON DELETE CASCADE;

    PRINT 'Table UtilitiesSetupContactos created.';
END
ELSE
BEGIN
    PRINT 'Table UtilitiesSetupContactos already exists.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.UtilitiesSetupProveedorInternet)
BEGIN
    INSERT INTO dbo.UtilitiesSetupProveedorInternet (Codigo, Nombre, Etiqueta, Velocidad, DetalleExtra, PrecioDesde, Orden, Activo)
    VALUES
    (N'Spectrum', N'Spectrum', N'Popular', N'Up to 500 Mbps', N'TV bundles available', 79.99, 1, 1),
    (N'AttFiber', N'AT&T Fiber', N'Fastest', N'Up to 1 Gig', N'TV bundles available', 89.99, 2, 1),
    (N'GoogleFiber', N'Google Fiber', N'Reliable', N'Up to 1 Gig', N'No annual contract', 99.99, 3, 1),
    (N'Kinetic', N'Kinetic', N'Great value', N'Up to 500 Mbps', N'TV bundles available', 69.99, 4, 1);
    PRINT 'UtilitiesSetupProveedorInternet seeded.';
END
GO

DECLARE @UtilServicioId INT = (
    SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Utilities Setup' ORDER BY Id
);

IF @UtilServicioId IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'UtilitiesSetup',
        LinkAction = N'UtilitiesSetupAddress',
        LinkRouteId = Id
    WHERE Id = @UtilServicioId
      AND (LinkController IS NULL OR LinkController = N'MovingSetup');

    PRINT 'MovingSetupServicios link updated for Utilities Setup.';
END
GO

PRINT 'Utilities Setup flow schema ready.';
GO
