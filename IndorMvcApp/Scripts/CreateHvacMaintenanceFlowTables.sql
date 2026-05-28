/*
  HVAC Maintenance flow — landing + solicitudes for Home Care Priorities.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.HvacMaintenanceServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HvacMaintenanceServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_HvacMaintenanceServicioLanding_PageTitle] DEFAULT (N'HVAC Tune-Up'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](400) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_HvacMaintenanceServicioLanding_PrecioDesde] DEFAULT (89),
        [PrecioTexto] [nvarchar](120) NULL,
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [PreviewItems] [nvarchar](500) NULL,
        [PreviewIconos] [nvarchar](300) NULL,
        [InfoBoxTexto] [nvarchar](400) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_HvacMaintenanceServicioLanding_CtaTexto] DEFAULT (N'Start tune-up request'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_HvacMaintenanceServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_HvacMaintenanceServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_HvacMaintenanceServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_HvacMaintenanceServicioLanding_HomeCarePriorityId]
        ON dbo.HvacMaintenanceServicioLanding ([HomeCarePriorityId]);

    ALTER TABLE [dbo].[HvacMaintenanceServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_HvacMaintenanceServicioLanding_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    PRINT 'Table HvacMaintenanceServicioLanding created.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesHvacMaintenance', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesHvacMaintenance](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [NumeroSerieAc] [nvarchar](80) NULL,
        [SerialDesconocido] [bit] NOT NULL CONSTRAINT [DF_SolicitudesHvacMaintenance_SerialDesconocido] DEFAULT (0),
        [UltimoMantenimiento] [nvarchar](80) NULL,
        [UltimoMantenimientoDesconocido] [bit] NOT NULL CONSTRAINT [DF_SolicitudesHvacMaintenance_UltimoMantenimientoDesconocido] DEFAULT (0),
        [TamanioFiltro] [nvarchar](40) NULL,
        [NotasTecnico] [nvarchar](500) NULL,
        [FechaVisita] [date] NULL,
        [VentanaHorario] [nvarchar](20) NULL,
        [TipoServicio] [nvarchar](20) NULL,
        [RecordatorioAnual] [bit] NOT NULL CONSTRAINT [DF_SolicitudesHvacMaintenance_RecordatorioAnual] DEFAULT (0),
        [TelefonoContacto] [nvarchar](30) NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesHvacMaintenance_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesHvacMaintenance_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesHvacMaintenance] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesHvacMaintenance]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesHvacMaintenance_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesHvacMaintenance]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesHvacMaintenance_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    ALTER TABLE [dbo].[SolicitudesHvacMaintenance]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesHvacMaintenance_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesHvacMaintenance_UserId_Priority_Estado]
        ON [dbo].[SolicitudesHvacMaintenance] ([UserId], [HomeCarePriorityId], [Estado]);

    PRINT 'Table SolicitudesHvacMaintenance created.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosHvacMaintenance', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosHvacMaintenance](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudHvacMaintenanceId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosHvacMaintenance_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosHvacMaintenance] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosHvacMaintenance]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosHvacMaintenance_SolicitudesHvacMaintenance]
        FOREIGN KEY([SolicitudHvacMaintenanceId]) REFERENCES [dbo].[SolicitudesHvacMaintenance] ([Id]) ON DELETE CASCADE;

    PRINT 'Table ArchivosHvacMaintenance created.';
END
GO

DECLARE @HvacPriorityId INT = (
    SELECT TOP 1 Id FROM dbo.HomeCarePriorities
    WHERE Nombre = N'HVAC maintenance'
    ORDER BY Id
);

IF @HvacPriorityId IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET LinkController = N'HvacMaintenance',
        LinkAction = N'HvacMaintenanceService',
        LinkUrl = NULL
    WHERE Id = @HvacPriorityId;

    IF NOT EXISTS (SELECT 1 FROM dbo.HvacMaintenanceServicioLanding WHERE HomeCarePriorityId = @HvacPriorityId)
    BEGIN
        INSERT INTO dbo.HvacMaintenanceServicioLanding (
            HomeCarePriorityId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, PrecioTexto, IncluyeItems, IncluyeIconos, PreviewItems, PreviewIconos, InfoBoxTexto, CtaTexto, Activo)
        VALUES (
            @HvacPriorityId,
            N'HVAC Tune-Up',
            N'HVAC Tune-Up',
            N'Recommended yearly',
            N'Yearly preventive maintenance to keep your air conditioning system running efficiently and reliably.',
            N'/inspeccion5.jpeg',
            89,
            N'From $89',
            N'System inspection|Filter check|Performance test|Basic tune-up',
            N'fa-screwdriver-wrench|fa-filter|fa-gauge-high|fa-fan',
            N'AC serial number|Last maintenance date (if known)|Preferred visit time',
            N'fa-barcode|fa-calendar|fa-clock',
            N'We''ll match you with the right HVAC pro based on your system details.',
            N'Start tune-up request',
            1);
        PRINT 'HvacMaintenanceServicioLanding seeded for HVAC maintenance.';
    END
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "HVAC maintenance" not found — run CreateHomeCarePrioritiesTables.sql first.';
END
GO

PRINT 'HVAC Maintenance flow schema ready.';
GO
