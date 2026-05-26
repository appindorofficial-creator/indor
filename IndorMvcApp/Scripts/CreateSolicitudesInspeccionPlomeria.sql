/*
  SolicitudesInspeccionPlomeria — plumbing inspection multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionPlomeria', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionPlomeria](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [UbicacionProblema] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [FugaAguaAhora] [nvarchar](20) NOT NULL,
        [SituacionesActuales] [nvarchar](300) NULL,
        [CuandoEmpezo] [nvarchar](20) NULL,
        [AguaCerrada] [nvarchar](20) NULL,
        [DescripcionProblema] [nvarchar](500) NULL,
        [NotasAdicionales] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionPlomeria] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_TipoProblema]
        DEFAULT (N'KitchenIssue') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_UbicacionProblema]
        DEFAULT (N'Kitchen') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_FugaAguaAhora]
        DEFAULT (N'No') FOR [FugaAguaAhora];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionPlomeria_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionPlomeria_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionPlomeria_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionPlomeria_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionPlomeria] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionPlomeria created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionPlomeria already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionPlomeria', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionPlomeria](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionPlomeriaId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionPlomeria] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionPlomeria]
        ADD CONSTRAINT [DF_ArchivosInspeccionPlomeria_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionPlomeria_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionPlomeriaId]) REFERENCES [dbo].[SolicitudesInspeccionPlomeria] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionPlomeria_SolicitudId]
        ON [dbo].[ArchivosInspeccionPlomeria] ([SolicitudInspeccionPlomeriaId]);

    PRINT 'Table ArchivosInspeccionPlomeria created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionPlomeria already exists.';
END
GO
