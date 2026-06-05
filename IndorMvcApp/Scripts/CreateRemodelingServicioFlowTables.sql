/*
  Remodeling Services flow — quote requests for catalog Servicios (kitchen, bath, etc.).
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesRemodelingServicio', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesRemodelingServicio](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ServicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [AlcanceProyecto] [nvarchar](40) NULL,
        [VentanaTiempo] [nvarchar](30) NULL,
        [PresupuestoEstimado] [nvarchar](30) NULL,
        [Descripcion] [nvarchar](500) NULL,
        [ContactoPreferido] [nvarchar](20) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesRemodelingServicio_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesRemodelingServicio_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesRemodelingServicio] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesRemodelingServicio]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesRemodelingServicio_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesRemodelingServicio]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesRemodelingServicio_Servicios]
        FOREIGN KEY([ServicioId]) REFERENCES [dbo].[Servicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesRemodelingServicio]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesRemodelingServicio_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesRemodelingServicio_UserId_Servicio_Estado]
        ON [dbo].[SolicitudesRemodelingServicio] ([UserId], [ServicioId], [Estado]);

    PRINT 'Table SolicitudesRemodelingServicio created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesRemodelingServicio already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosRemodelingServicio', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosRemodelingServicio](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudRemodelingServicioId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosRemodelingServicio_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosRemodelingServicio] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosRemodelingServicio]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosRemodelingServicio_SolicitudesRemodelingServicio]
        FOREIGN KEY([SolicitudRemodelingServicioId]) REFERENCES [dbo].[SolicitudesRemodelingServicio] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ArchivosRemodelingServicio]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosRemodelingServicio_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    PRINT 'Table ArchivosRemodelingServicio created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosRemodelingServicio already exists.';
END
GO

PRINT 'Remodeling Services flow schema ready.';
GO
