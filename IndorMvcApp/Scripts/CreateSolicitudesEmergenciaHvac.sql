/*
  SolicitudesEmergenciaHvac — HVAC emergency request multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesEmergenciaHvac', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesEmergenciaHvac](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ServicioEmergenciaId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [SucedeAhora] [nvarchar](20) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [TelefonoContacto] [nvarchar](30) NULL,
        [PuedeLlamarYa] [nvarchar](20) NULL,
        [EnCasaAhora] [nvarchar](20) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesEmergenciaHvac] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaHvac_TipoProblema]
        DEFAULT (N'NotCooling') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaHvac_SucedeAhora]
        DEFAULT (N'Yes') FOR [SucedeAhora];

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaHvac_Urgencia]
        DEFAULT (N'Emergency') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaHvac_PuedeLlamarYa]
        DEFAULT (N'Yes') FOR [PuedeLlamarYa];

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaHvac_EnCasaAhora]
        DEFAULT (N'Yes') FOR [EnCasaAhora];

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaHvac_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaHvac_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaHvac_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaHvac_ServiciosEmergencia]
        FOREIGN KEY([ServicioEmergenciaId]) REFERENCES [dbo].[ServiciosEmergencia] ([Id]);

    ALTER TABLE [dbo].[SolicitudesEmergenciaHvac]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaHvac_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesEmergenciaHvac_UserId_ServicioEmergenciaId]
        ON [dbo].[SolicitudesEmergenciaHvac] ([UserId], [ServicioEmergenciaId], [Estado]);

    PRINT 'Table SolicitudesEmergenciaHvac created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesEmergenciaHvac already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosEmergenciaHvac', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosEmergenciaHvac](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudEmergenciaHvacId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosEmergenciaHvac] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosEmergenciaHvac]
        ADD CONSTRAINT [DF_ArchivosEmergenciaHvac_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosEmergenciaHvac]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosEmergenciaHvac_SolicitudesEmergenciaHvac]
        FOREIGN KEY([SolicitudEmergenciaHvacId])
        REFERENCES [dbo].[SolicitudesEmergenciaHvac] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosEmergenciaHvac_SolicitudId]
        ON [dbo].[ArchivosEmergenciaHvac] ([SolicitudEmergenciaHvacId]);

    PRINT 'Table ArchivosEmergenciaHvac created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosEmergenciaHvac already exists.';
END
GO
