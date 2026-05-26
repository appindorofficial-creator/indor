/*
  SolicitudesInspeccionInvestor — Investor Inspection multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesInspeccionInvestor', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionInvestor](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoInversion] [nvarchar](30) NOT NULL,
        [EnfoquesInversion] [nvarchar](300) NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [Ocupacion] [nvarchar](20) NULL,
        [NivelRehab] [nvarchar](20) NULL,
        [AreasRevision] [nvarchar](200) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionInvestor] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        ADD CONSTRAINT [DF_SolicitudesInspeccionInvestor_TipoInversion]
        DEFAULT (N'Flip') FOR [TipoInversion];

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        ADD CONSTRAINT [DF_SolicitudesInspeccionInvestor_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        ADD CONSTRAINT [DF_SolicitudesInspeccionInvestor_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        ADD CONSTRAINT [DF_SolicitudesInspeccionInvestor_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionInvestor_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionInvestor_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionInvestor_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionInvestor_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionInvestor] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionInvestor created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionInvestor already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionInvestor', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionInvestor](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionInvestorId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionInvestor] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionInvestor]
        ADD CONSTRAINT [DF_ArchivosInspeccionInvestor_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionInvestor]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionInvestor_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionInvestorId]) REFERENCES [dbo].[SolicitudesInspeccionInvestor] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionInvestor_SolicitudId]
        ON [dbo].[ArchivosInspeccionInvestor] ([SolicitudInspeccionInvestorId]);

    PRINT 'Table ArchivosInspeccionInvestor created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionInvestor already exists.';
END
GO
