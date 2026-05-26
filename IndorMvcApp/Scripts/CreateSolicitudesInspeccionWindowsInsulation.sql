/*
  SolicitudesInspeccionWindowsInsulation — Windows and Insulation inspection multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionWindowsInsulation', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionWindowsInsulation](
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
        [DanoHumedadVisible] [nvarchar](20) NOT NULL,
        [MotivosRevision] [nvarchar](200) NULL,
        [MotivoRevision] [nvarchar](30) NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [NumeroPisos] [nvarchar](10) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [TipoVentana] [nvarchar](20) NULL,
        [AccesoAtticCrawlSpace] [nvarchar](20) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionWindowsInsulation] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_TipoProblema]
        DEFAULT (N'DraftAir') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_UbicacionProblema]
        DEFAULT (N'LivingRoom') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_DanoHumedadVisible]
        DEFAULT (N'No') FOR [DanoHumedadVisible];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionWindowsInsulation_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionWindowsInsulation_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionWindowsInsulation_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionWindowsInsulation_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionWindowsInsulation] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionWindowsInsulation created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionWindowsInsulation already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionWindowsInsulation', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionWindowsInsulation](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionWindowsInsulationId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionWindowsInsulation] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_ArchivosInspeccionWindowsInsulation_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionWindowsInsulation]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionWindowsInsulation_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionWindowsInsulationId]) REFERENCES [dbo].[SolicitudesInspeccionWindowsInsulation] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionWindowsInsulation_SolicitudId]
        ON [dbo].[ArchivosInspeccionWindowsInsulation] ([SolicitudInspeccionWindowsInsulationId]);

    PRINT 'Table ArchivosInspeccionWindowsInsulation created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionWindowsInsulation already exists.';
END
GO
