/*
  SolicitudesEmergenciaElectrical — Electrical emergency request multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesEmergenciaElectrical', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesEmergenciaElectrical](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ServicioEmergenciaId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [PuedeApagarBreaker] [nvarchar](20) NOT NULL,
        [UbicacionProblema] [nvarchar](30) NULL,
        [SintomasNotados] [nvarchar](300) NULL,
        [EnergiaEncendida] [nvarchar](20) NULL,
        [PuedeAlejarse] [nvarchar](20) NULL,
        [NotaCorta] [nvarchar](250) NULL,
        [TelefonoContacto] [nvarchar](30) NULL,
        [AceptaTextos] [nvarchar](10) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesEmergenciaElectrical] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaElectrical_TipoProblema]
        DEFAULT (N'BreakerTripping') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaElectrical_Urgencia]
        DEFAULT (N'Emergency') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaElectrical_PuedeApagarBreaker]
        DEFAULT (N'NotSure') FOR [PuedeApagarBreaker];

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaElectrical_UbicacionProblema]
        DEFAULT (N'Garage') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaElectrical_EnergiaEncendida]
        DEFAULT (N'Yes') FOR [EnergiaEncendida];

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaElectrical_PuedeAlejarse]
        DEFAULT (N'Yes') FOR [PuedeAlejarse];

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaElectrical_AceptaTextos]
        DEFAULT (N'Yes') FOR [AceptaTextos];

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaElectrical_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaElectrical_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaElectrical_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaElectrical_ServiciosEmergencia]
        FOREIGN KEY([ServicioEmergenciaId]) REFERENCES [dbo].[ServiciosEmergencia] ([Id]);

    ALTER TABLE [dbo].[SolicitudesEmergenciaElectrical]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaElectrical_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesEmergenciaElectrical_UserId_ServicioEmergenciaId]
        ON [dbo].[SolicitudesEmergenciaElectrical] ([UserId], [ServicioEmergenciaId], [Estado]);

    PRINT 'Table SolicitudesEmergenciaElectrical created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesEmergenciaElectrical already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosEmergenciaElectrical', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosEmergenciaElectrical](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudEmergenciaElectricalId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosEmergenciaElectrical] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosEmergenciaElectrical]
        ADD CONSTRAINT [DF_ArchivosEmergenciaElectrical_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosEmergenciaElectrical]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosEmergenciaElectrical_SolicitudesEmergenciaElectrical]
        FOREIGN KEY([SolicitudEmergenciaElectricalId])
        REFERENCES [dbo].[SolicitudesEmergenciaElectrical] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosEmergenciaElectrical_SolicitudId]
        ON [dbo].[ArchivosEmergenciaElectrical] ([SolicitudEmergenciaElectricalId]);

    PRINT 'Table ArchivosEmergenciaElectrical created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosEmergenciaElectrical already exists.';
END
GO
