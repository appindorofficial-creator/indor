/*
  SolicitudesInspeccionElectrica — electrical inspection details + media upload flow.

  Run against your Indor database after backup.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionElectrica', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionElectrica](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [MotivoRevision] [nvarchar](40) NOT NULL,
        [PreocupacionPrincipal] [nvarchar](40) NOT NULL,
        [OcurreAhora] [nvarchar](20) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionElectrica] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        ADD CONSTRAINT [DF_SolicitudesInspeccionElectrica_MotivoRevision]
        DEFAULT (N'BuyingHome') FOR [MotivoRevision];

    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        ADD CONSTRAINT [DF_SolicitudesInspeccionElectrica_PreocupacionPrincipal]
        DEFAULT (N'GeneralReview') FOR [PreocupacionPrincipal];

    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        ADD CONSTRAINT [DF_SolicitudesInspeccionElectrica_OcurreAhora]
        DEFAULT (N'No') FOR [OcurreAhora];

    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        ADD CONSTRAINT [DF_SolicitudesInspeccionElectrica_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        ADD CONSTRAINT [DF_SolicitudesInspeccionElectrica_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        ADD CONSTRAINT [DF_SolicitudesInspeccionElectrica_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionElectrica_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionElectrica_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionElectrica_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionElectrica_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionElectrica] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionElectrica created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionElectrica already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionElectrica', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionElectrica](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionElectricaId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionElectrica] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionElectrica]
        ADD CONSTRAINT [DF_ArchivosInspeccionElectrica_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionElectrica]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionElectrica_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionElectricaId]) REFERENCES [dbo].[SolicitudesInspeccionElectrica] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionElectrica_SolicitudId]
        ON [dbo].[ArchivosInspeccionElectrica] ([SolicitudInspeccionElectricaId]);

    PRINT 'Table ArchivosInspeccionElectrica created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionElectrica already exists.';
END
GO
