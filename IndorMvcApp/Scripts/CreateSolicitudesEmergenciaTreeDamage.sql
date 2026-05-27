/*
  SolicitudesEmergenciaTreeDamage — Tree damage emergency request multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesEmergenciaTreeDamage', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesEmergenciaTreeDamage](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ServicioEmergenciaId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [UbicacionDanio] [nvarchar](30) NOT NULL,
        [PeligroInmediato] [nvarchar](20) NOT NULL,
        [RiesgoUtilidad] [nvarchar](30) NOT NULL,
        [AccesoCasa] [nvarchar](20) NULL,
        [EntradaBloqueada] [nvarchar](20) NULL,
        [PuedeAlejarse] [nvarchar](20) NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [TelefonoContacto] [nvarchar](30) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesEmergenciaTreeDamage] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaTreeDamage_TipoProblema]
        DEFAULT (N'FallenBranch') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaTreeDamage_UbicacionDanio]
        DEFAULT (N'FrontYard') FOR [UbicacionDanio];

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaTreeDamage_PeligroInmediato]
        DEFAULT (N'NotSure') FOR [PeligroInmediato];

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaTreeDamage_RiesgoUtilidad]
        DEFAULT (N'NotSure') FOR [RiesgoUtilidad];

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaTreeDamage_AccesoCasa]
        DEFAULT (N'Yes') FOR [AccesoCasa];

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaTreeDamage_EntradaBloqueada]
        DEFAULT (N'NotSure') FOR [EntradaBloqueada];

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaTreeDamage_PuedeAlejarse]
        DEFAULT (N'Yes') FOR [PuedeAlejarse];

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaTreeDamage_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaTreeDamage_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaTreeDamage_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaTreeDamage_ServiciosEmergencia]
        FOREIGN KEY([ServicioEmergenciaId]) REFERENCES [dbo].[ServiciosEmergencia] ([Id]);

    ALTER TABLE [dbo].[SolicitudesEmergenciaTreeDamage]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaTreeDamage_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesEmergenciaTreeDamage_UserId_ServicioEmergenciaId]
        ON [dbo].[SolicitudesEmergenciaTreeDamage] ([UserId], [ServicioEmergenciaId], [Estado]);

    PRINT 'Table SolicitudesEmergenciaTreeDamage created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesEmergenciaTreeDamage already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosEmergenciaTreeDamage', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosEmergenciaTreeDamage](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudEmergenciaTreeDamageId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosEmergenciaTreeDamage] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosEmergenciaTreeDamage]
        ADD CONSTRAINT [DF_ArchivosEmergenciaTreeDamage_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosEmergenciaTreeDamage]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosEmergenciaTreeDamage_SolicitudesEmergenciaTreeDamage]
        FOREIGN KEY([SolicitudEmergenciaTreeDamageId])
        REFERENCES [dbo].[SolicitudesEmergenciaTreeDamage] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosEmergenciaTreeDamage_SolicitudId]
        ON [dbo].[ArchivosEmergenciaTreeDamage] ([SolicitudEmergenciaTreeDamageId]);

    PRINT 'Table ArchivosEmergenciaTreeDamage created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosEmergenciaTreeDamage already exists.';
END
GO
