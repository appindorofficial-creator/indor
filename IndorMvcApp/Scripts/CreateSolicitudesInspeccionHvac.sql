/*
  SolicitudesInspeccionHvac — HVAC inspection multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionHvac', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionHvac](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [ParteAtencion] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [SistemaFuncionando] [nvarchar](20) NOT NULL,
        [TipoEquipo] [nvarchar](30) NULL,
        [CantidadSistemas] [nvarchar](10) NULL,
        [ComponentesRevision] [nvarchar](200) NULL,
        [EdadSistema] [nvarchar](20) NULL,
        [FiltroCambiado] [nvarchar](20) NULL,
        [TipoTermostato] [nvarchar](20) NULL,
        [DescripcionProblema] [nvarchar](500) NULL,
        [NotasOpcionales] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionHvac] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_TipoProblema]
        DEFAULT (N'NotCooling') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_ParteAtencion]
        DEFAULT (N'WholeSystem') FOR [ParteAtencion];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_SistemaFuncionando]
        DEFAULT (N'Yes') FOR [SistemaFuncionando];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHvac_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHvac_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHvac_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionHvac_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionHvac] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionHvac created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionHvac already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionHvac', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionHvac](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionHvacId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionHvac] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionHvac]
        ADD CONSTRAINT [DF_ArchivosInspeccionHvac_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionHvac]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionHvac_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionHvacId]) REFERENCES [dbo].[SolicitudesInspeccionHvac] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionHvac_SolicitudId]
        ON [dbo].[ArchivosInspeccionHvac] ([SolicitudInspeccionHvacId]);

    PRINT 'Table ArchivosInspeccionHvac created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionHvac already exists.';
END
GO
