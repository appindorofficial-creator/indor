/*
  SolicitudesInspeccionCompleta — Complete Home Inspection review + media upload flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionCompleta', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionCompleta](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [MotivoInspeccion] [nvarchar](40) NOT NULL,
        [AreasEnfoque] [nvarchar](200) NOT NULL,
        [TamanoPropiedad] [nvarchar](50) NULL,
        [EsUrgente] [nvarchar](20) NOT NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionCompleta] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionCompleta]
        ADD CONSTRAINT [DF_SolicitudesInspeccionCompleta_MotivoInspeccion]
        DEFAULT (N'BuyingHome') FOR [MotivoInspeccion];

    ALTER TABLE [dbo].[SolicitudesInspeccionCompleta]
        ADD CONSTRAINT [DF_SolicitudesInspeccionCompleta_AreasEnfoque]
        DEFAULT (N'Electrical|HVAC|GeneralStructure') FOR [AreasEnfoque];

    ALTER TABLE [dbo].[SolicitudesInspeccionCompleta]
        ADD CONSTRAINT [DF_SolicitudesInspeccionCompleta_EsUrgente]
        DEFAULT (N'No') FOR [EsUrgente];

    ALTER TABLE [dbo].[SolicitudesInspeccionCompleta]
        ADD CONSTRAINT [DF_SolicitudesInspeccionCompleta_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionCompleta]
        ADD CONSTRAINT [DF_SolicitudesInspeccionCompleta_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionCompleta]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionCompleta_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionCompleta]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionCompleta_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionCompleta]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionCompleta_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionCompleta_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionCompleta] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionCompleta created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionCompleta already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionCompleta', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionCompleta](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionCompletaId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionCompleta] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionCompleta]
        ADD CONSTRAINT [DF_ArchivosInspeccionCompleta_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionCompleta]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionCompleta_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionCompletaId]) REFERENCES [dbo].[SolicitudesInspeccionCompleta] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionCompleta_SolicitudId]
        ON [dbo].[ArchivosInspeccionCompleta] ([SolicitudInspeccionCompletaId]);

    PRINT 'Table ArchivosInspeccionCompleta created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionCompleta already exists.';
END
GO
