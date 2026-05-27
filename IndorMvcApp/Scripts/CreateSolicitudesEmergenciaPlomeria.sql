/*
  SolicitudesEmergenciaPlomeria — Plumbing emergency request multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesEmergenciaPlomeria', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesEmergenciaPlomeria](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ServicioEmergenciaId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [AguaFluyendo] [nvarchar](20) NOT NULL,
        [PuedeCerrarAgua] [nvarchar](20) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [TelefonoContacto] [nvarchar](30) NULL,
        [AccesoSiAusente] [nvarchar](20) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesEmergenciaPlomeria] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaPlomeria_TipoProblema]
        DEFAULT (N'BurstPipe') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaPlomeria_AguaFluyendo]
        DEFAULT (N'Yes') FOR [AguaFluyendo];

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaPlomeria_PuedeCerrarAgua]
        DEFAULT (N'Yes') FOR [PuedeCerrarAgua];

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaPlomeria_Urgencia]
        DEFAULT (N'Emergency') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaPlomeria_AccesoSiAusente]
        DEFAULT (N'Yes') FOR [AccesoSiAusente];

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaPlomeria_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaPlomeria_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaPlomeria_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaPlomeria_ServiciosEmergencia]
        FOREIGN KEY([ServicioEmergenciaId]) REFERENCES [dbo].[ServiciosEmergencia] ([Id]);

    ALTER TABLE [dbo].[SolicitudesEmergenciaPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaPlomeria_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesEmergenciaPlomeria_UserId_ServicioEmergenciaId]
        ON [dbo].[SolicitudesEmergenciaPlomeria] ([UserId], [ServicioEmergenciaId], [Estado]);

    PRINT 'Table SolicitudesEmergenciaPlomeria created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesEmergenciaPlomeria already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosEmergenciaPlomeria', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosEmergenciaPlomeria](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudEmergenciaPlomeriaId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosEmergenciaPlomeria] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosEmergenciaPlomeria]
        ADD CONSTRAINT [DF_ArchivosEmergenciaPlomeria_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosEmergenciaPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosEmergenciaPlomeria_SolicitudesEmergenciaPlomeria]
        FOREIGN KEY([SolicitudEmergenciaPlomeriaId])
        REFERENCES [dbo].[SolicitudesEmergenciaPlomeria] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosEmergenciaPlomeria_SolicitudId]
        ON [dbo].[ArchivosEmergenciaPlomeria] ([SolicitudEmergenciaPlomeriaId]);

    PRINT 'Table ArchivosEmergenciaPlomeria created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosEmergenciaPlomeria already exists.';
END
GO
