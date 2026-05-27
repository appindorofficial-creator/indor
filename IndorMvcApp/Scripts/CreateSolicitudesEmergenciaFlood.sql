/*
  SolicitudesEmergenciaFlood — Flood emergency request multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesEmergenciaFlood', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesEmergenciaFlood](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ServicioEmergenciaId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [CausaAgua] [nvarchar](40) NOT NULL,
        [UbicacionAgua] [nvarchar](30) NOT NULL,
        [AguaActiva] [nvarchar](20) NOT NULL,
        [PuedeCerrarAgua] [nvarchar](20) NULL,
        [UbicacionCierreAgua] [nvarchar](20) NULL,
        [PuedeApagarElectricidad] [nvarchar](20) NULL,
        [CantidadAgua] [nvarchar](20) NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesEmergenciaFlood] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaFlood_CausaAgua]
        DEFAULT (N'UnknownSource') FOR [CausaAgua];

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaFlood_UbicacionAgua]
        DEFAULT (N'FirstFloor') FOR [UbicacionAgua];

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaFlood_AguaActiva]
        DEFAULT (N'Yes') FOR [AguaActiva];

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaFlood_PuedeCerrarAgua]
        DEFAULT (N'NotSure') FOR [PuedeCerrarAgua];

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaFlood_UbicacionCierreAgua]
        DEFAULT (N'DontKnow') FOR [UbicacionCierreAgua];

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaFlood_PuedeApagarElectricidad]
        DEFAULT (N'NotSure') FOR [PuedeApagarElectricidad];

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaFlood_CantidadAgua]
        DEFAULT (N'OneRoom') FOR [CantidadAgua];

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaFlood_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaFlood_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaFlood_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaFlood_ServiciosEmergencia]
        FOREIGN KEY([ServicioEmergenciaId]) REFERENCES [dbo].[ServiciosEmergencia] ([Id]);

    ALTER TABLE [dbo].[SolicitudesEmergenciaFlood]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaFlood_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesEmergenciaFlood_UserId_ServicioEmergenciaId]
        ON [dbo].[SolicitudesEmergenciaFlood] ([UserId], [ServicioEmergenciaId], [Estado]);

    PRINT 'Table SolicitudesEmergenciaFlood created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesEmergenciaFlood already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosEmergenciaFlood', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosEmergenciaFlood](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudEmergenciaFloodId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosEmergenciaFlood] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosEmergenciaFlood]
        ADD CONSTRAINT [DF_ArchivosEmergenciaFlood_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosEmergenciaFlood]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosEmergenciaFlood_SolicitudesEmergenciaFlood]
        FOREIGN KEY([SolicitudEmergenciaFloodId])
        REFERENCES [dbo].[SolicitudesEmergenciaFlood] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosEmergenciaFlood_SolicitudId]
        ON [dbo].[ArchivosEmergenciaFlood] ([SolicitudEmergenciaFloodId]);

    PRINT 'Table ArchivosEmergenciaFlood created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosEmergenciaFlood already exists.';
END
GO
