/*
  SolicitudesInspeccion — purchase details + report upload flow for home inspections.

  Run against your Indor database after backup.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccion', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccion](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [BajoContrato] [bit] NOT NULL,
        [FechaCierreEstimada] [date] NULL,
        [TieneReporteExistente] [bit] NOT NULL,
        [RolComprador] [nvarchar](30) NOT NULL,
        [ObjetivoPrincipal] [nvarchar](50) NOT NULL,
        [NotasRevision] [nvarchar](250) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccion] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccion]
        ADD CONSTRAINT [DF_SolicitudesInspeccion_BajoContrato]
        DEFAULT ((0)) FOR [BajoContrato];

    ALTER TABLE [dbo].[SolicitudesInspeccion]
        ADD CONSTRAINT [DF_SolicitudesInspeccion_TieneReporteExistente]
        DEFAULT ((0)) FOR [TieneReporteExistente];

    ALTER TABLE [dbo].[SolicitudesInspeccion]
        ADD CONSTRAINT [DF_SolicitudesInspeccion_RolComprador]
        DEFAULT (N'Buyer') FOR [RolComprador];

    ALTER TABLE [dbo].[SolicitudesInspeccion]
        ADD CONSTRAINT [DF_SolicitudesInspeccion_ObjetivoPrincipal]
        DEFAULT (N'UnderstandRepairRisks') FOR [ObjetivoPrincipal];

    ALTER TABLE [dbo].[SolicitudesInspeccion]
        ADD CONSTRAINT [DF_SolicitudesInspeccion_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccion]
        ADD CONSTRAINT [DF_SolicitudesInspeccion_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccion]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccion_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccion]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccion_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccion]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccion_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccion_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccion] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccion created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccion already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosReporteInspeccion', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosReporteInspeccion](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosReporteInspeccion] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosReporteInspeccion]
        ADD CONSTRAINT [DF_ArchivosReporteInspeccion_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosReporteInspeccion]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosReporteInspeccion_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionId]) REFERENCES [dbo].[SolicitudesInspeccion] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosReporteInspeccion_SolicitudId]
        ON [dbo].[ArchivosReporteInspeccion] ([SolicitudInspeccionId]);

    PRINT 'Table ArchivosReporteInspeccion created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosReporteInspeccion already exists.';
END
GO
