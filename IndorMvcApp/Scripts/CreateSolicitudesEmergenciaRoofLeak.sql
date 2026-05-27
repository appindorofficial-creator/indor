/*
  SolicitudesEmergenciaRoofLeak — Roof leak emergency request multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesEmergenciaRoofLeak', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesEmergenciaRoofLeak](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ServicioEmergenciaId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [UbicacionFuga] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [PuedeColocarCubeta] [nvarchar](20) NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesEmergenciaRoofLeak] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesEmergenciaRoofLeak]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaRoofLeak_TipoProblema]
        DEFAULT (N'ActiveDripping') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesEmergenciaRoofLeak]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaRoofLeak_UbicacionFuga]
        DEFAULT (N'Ceiling') FOR [UbicacionFuga];

    ALTER TABLE [dbo].[SolicitudesEmergenciaRoofLeak]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaRoofLeak_Urgencia]
        DEFAULT (N'Emergency') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesEmergenciaRoofLeak]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaRoofLeak_PuedeColocarCubeta]
        DEFAULT (N'Yes') FOR [PuedeColocarCubeta];

    ALTER TABLE [dbo].[SolicitudesEmergenciaRoofLeak]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaRoofLeak_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesEmergenciaRoofLeak]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaRoofLeak_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesEmergenciaRoofLeak]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaRoofLeak_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesEmergenciaRoofLeak]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaRoofLeak_ServiciosEmergencia]
        FOREIGN KEY([ServicioEmergenciaId]) REFERENCES [dbo].[ServiciosEmergencia] ([Id]);

    ALTER TABLE [dbo].[SolicitudesEmergenciaRoofLeak]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaRoofLeak_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesEmergenciaRoofLeak_UserId_ServicioEmergenciaId]
        ON [dbo].[SolicitudesEmergenciaRoofLeak] ([UserId], [ServicioEmergenciaId], [Estado]);

    PRINT 'Table SolicitudesEmergenciaRoofLeak created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesEmergenciaRoofLeak already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosEmergenciaRoofLeak', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosEmergenciaRoofLeak](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudEmergenciaRoofLeakId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosEmergenciaRoofLeak] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosEmergenciaRoofLeak]
        ADD CONSTRAINT [DF_ArchivosEmergenciaRoofLeak_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosEmergenciaRoofLeak]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosEmergenciaRoofLeak_SolicitudesEmergenciaRoofLeak]
        FOREIGN KEY([SolicitudEmergenciaRoofLeakId])
        REFERENCES [dbo].[SolicitudesEmergenciaRoofLeak] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosEmergenciaRoofLeak_SolicitudId]
        ON [dbo].[ArchivosEmergenciaRoofLeak] ([SolicitudEmergenciaRoofLeakId]);

    PRINT 'Table ArchivosEmergenciaRoofLeak created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosEmergenciaRoofLeak already exists.';
END
GO
