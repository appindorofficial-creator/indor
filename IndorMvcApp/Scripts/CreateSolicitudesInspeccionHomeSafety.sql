/*
  SolicitudesInspeccionHomeSafety — Home Safety inspection multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionHomeSafety', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionHomeSafety](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TiposProblema] [nvarchar](300) NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [AreasAtencion] [nvarchar](300) NULL,
        [UbicacionProblema] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [RiesgoActivo] [nvarchar](20) NOT NULL,
        [MotivosRevision] [nvarchar](200) NULL,
        [MotivoRevision] [nvarchar](30) NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [NumeroPisos] [nvarchar](10) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [OcupantesHogar] [nvarchar](100) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionHomeSafety] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_TipoProblema]
        DEFAULT (N'SmokeDetectorConcern') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_UbicacionProblema]
        DEFAULT (N'Hallway') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_RiesgoActivo]
        DEFAULT (N'No') FOR [RiesgoActivo];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHomeSafety_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHomeSafety_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHomeSafety_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionHomeSafety_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionHomeSafety] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionHomeSafety created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionHomeSafety already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionHomeSafety', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionHomeSafety](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionHomeSafetyId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionHomeSafety] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionHomeSafety]
        ADD CONSTRAINT [DF_ArchivosInspeccionHomeSafety_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionHomeSafety]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionHomeSafety_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionHomeSafetyId]) REFERENCES [dbo].[SolicitudesInspeccionHomeSafety] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionHomeSafety_SolicitudId]
        ON [dbo].[ArchivosInspeccionHomeSafety] ([SolicitudInspeccionHomeSafetyId]);

    PRINT 'Table ArchivosInspeccionHomeSafety created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionHomeSafety already exists.';
END
GO
