/*
  SolicitudesEmergenciaWaterHeater — Water Heater emergency request 4-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesEmergenciaWaterHeater', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesEmergenciaWaterHeater](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ServicioEmergenciaId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TiposProblema] [nvarchar](300) NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [UnidadFuncionando] [nvarchar](20) NOT NULL,
        [UbicacionProblema] [nvarchar](30) NULL,
        [TipoUnidad] [nvarchar](20) NULL,
        [SintomasVisibles] [nvarchar](300) NULL,
        [NotaCorta] [nvarchar](250) NULL,
        [DetallesAcceso] [nvarchar](500) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesEmergenciaWaterHeater] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaWaterHeater_TipoProblema]
        DEFAULT (N'NoHotWater') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaWaterHeater_Urgencia]
        DEFAULT (N'Emergency') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaWaterHeater_UnidadFuncionando]
        DEFAULT (N'No') FOR [UnidadFuncionando];

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaWaterHeater_UbicacionProblema]
        DEFAULT (N'Garage') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaWaterHeater_TipoUnidad]
        DEFAULT (N'Gas') FOR [TipoUnidad];

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaWaterHeater_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaWaterHeater_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaWaterHeater_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaWaterHeater_ServiciosEmergencia]
        FOREIGN KEY([ServicioEmergenciaId]) REFERENCES [dbo].[ServiciosEmergencia] ([Id]);

    ALTER TABLE [dbo].[SolicitudesEmergenciaWaterHeater]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaWaterHeater_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesEmergenciaWaterHeater_UserId_ServicioEmergenciaId]
        ON [dbo].[SolicitudesEmergenciaWaterHeater] ([UserId], [ServicioEmergenciaId], [Estado]);

    PRINT 'Table SolicitudesEmergenciaWaterHeater created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesEmergenciaWaterHeater already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosEmergenciaWaterHeater', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosEmergenciaWaterHeater](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudEmergenciaWaterHeaterId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosEmergenciaWaterHeater] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosEmergenciaWaterHeater]
        ADD CONSTRAINT [DF_ArchivosEmergenciaWaterHeater_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosEmergenciaWaterHeater]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosEmergenciaWaterHeater_SolicitudesEmergenciaWaterHeater]
        FOREIGN KEY([SolicitudEmergenciaWaterHeaterId])
        REFERENCES [dbo].[SolicitudesEmergenciaWaterHeater] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosEmergenciaWaterHeater_SolicitudId]
        ON [dbo].[ArchivosEmergenciaWaterHeater] ([SolicitudEmergenciaWaterHeaterId]);

    PRINT 'Table ArchivosEmergenciaWaterHeater created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosEmergenciaWaterHeater already exists.';
END
GO
