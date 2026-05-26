/*
  SolicitudesInspeccionStructural — structural inspection multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionStructural', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionStructural](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [MotivoRevision] [nvarchar](40) NOT NULL,
        [TipoPreocupacion] [nvarchar](40) NOT NULL,
        [TiposPreocupacion] [nvarchar](300) NULL,
        [AreaPreocupacion] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [DanoVisible] [nvarchar](20) NOT NULL,
        [SignosVisibles] [nvarchar](300) NULL,
        [SeveridadApariencia] [nvarchar](20) NULL,
        [UbicacionEspecifica] [nvarchar](200) NULL,
        [CuandoNotadoTexto] [nvarchar](50) NULL,
        [DuracionProblema] [nvarchar](20) NULL,
        [Severidad] [nvarchar](20) NULL,
        [ReparacionesPrevias] [nvarchar](20) NULL,
        [CondicionesInseguras] [nvarchar](200) NULL,
        [MejorHorarioVisita] [nvarchar](20) NULL,
        [TipoPropiedad] [nvarchar](30) NULL,
        [TipoFundacion] [nvarchar](20) NULL,
        [TieneReporte] [nvarchar](10) NULL,
        [CambiosRecientes] [nvarchar](200) NULL,
        [AccesoPreferido] [nvarchar](30) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [CuandoNotado] [nvarchar](20) NULL,
        [EdadPropiedad] [nvarchar](20) NULL,
        [RemodelReciente] [nvarchar](20) NULL,
        [DescripcionProblema] [nvarchar](500) NULL,
        [NotasOpcionales] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionStructural] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_MotivoRevision]
        DEFAULT (N'BeforePurchase') FOR [MotivoRevision];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_TipoPreocupacion]
        DEFAULT (N'FoundationCrack') FOR [TipoPreocupacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_AreaPreocupacion]
        DEFAULT (N'Foundation') FOR [AreaPreocupacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_DanoVisible]
        DEFAULT (N'Yes') FOR [DanoVisible];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionStructural_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionStructural_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionStructural_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionStructural_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionStructural] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionStructural created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionStructural already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionStructural', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionStructural](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionStructuralId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionStructural] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionStructural]
        ADD CONSTRAINT [DF_ArchivosInspeccionStructural_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionStructural]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionStructural_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionStructuralId]) REFERENCES [dbo].[SolicitudesInspeccionStructural] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionStructural_SolicitudId]
        ON [dbo].[ArchivosInspeccionStructural] ([SolicitudInspeccionStructuralId]);

    PRINT 'Table ArchivosInspeccionStructural created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionStructural already exists.';
END
GO
