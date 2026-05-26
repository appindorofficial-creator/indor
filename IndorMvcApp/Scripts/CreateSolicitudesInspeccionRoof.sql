/*
  SolicitudesInspeccionRoof — Roof inspection multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionRoof', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionRoof](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TiposProblema] [nvarchar](200) NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [UbicacionProblema] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [MotivoRevision] [nvarchar](30) NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [NumeroPisos] [nvarchar](10) NULL,
        [MaterialTecho] [nvarchar](20) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionRoof] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_TipoProblema]
        DEFAULT (N'ActiveLeak') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_UbicacionProblema]
        DEFAULT (N'MainRoof') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionRoof_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionRoof_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionRoof_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionRoof_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionRoof] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionRoof created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionRoof already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionRoof', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionRoof](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionRoofId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionRoof] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionRoof]
        ADD CONSTRAINT [DF_ArchivosInspeccionRoof_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionRoof]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionRoof_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionRoofId]) REFERENCES [dbo].[SolicitudesInspeccionRoof] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionRoof_SolicitudId]
        ON [dbo].[ArchivosInspeccionRoof] ([SolicitudInspeccionRoofId]);

    PRINT 'Table ArchivosInspeccionRoof created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionRoof already exists.';
END
GO
