/*
  SolicitudesInspeccionMoldMoisture — Mold and Moisture inspection multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionMoldMoisture', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionMoldMoisture](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TiposProblema] [nvarchar](300) NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [UbicacionProblema] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [HumedadActiva] [nvarchar](20) NOT NULL,
        [MotivoRevision] [nvarchar](30) NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [UbicacionPrincipal] [nvarchar](30) NULL,
        [IntrusionAguaReciente] [nvarchar](20) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](500) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionMoldMoisture] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_TipoProblema]
        DEFAULT (N'VisibleMold') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_UbicacionProblema]
        DEFAULT (N'Bathroom') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_HumedadActiva]
        DEFAULT (N'Yes') FOR [HumedadActiva];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionMoldMoisture_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionMoldMoisture_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionMoldMoisture_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionMoldMoisture_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionMoldMoisture] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionMoldMoisture created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionMoldMoisture already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionMoldMoisture', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionMoldMoisture](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionMoldMoistureId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionMoldMoisture] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_ArchivosInspeccionMoldMoisture_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionMoldMoisture]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionMoldMoisture_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionMoldMoistureId]) REFERENCES [dbo].[SolicitudesInspeccionMoldMoisture] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionMoldMoisture_SolicitudId]
        ON [dbo].[ArchivosInspeccionMoldMoisture] ([SolicitudInspeccionMoldMoistureId]);

    PRINT 'Table ArchivosInspeccionMoldMoisture created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionMoldMoisture already exists.';
END
GO
