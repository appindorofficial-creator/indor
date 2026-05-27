/*
  SolicitudesEmergenciaSmokeDetector — Smoke detector emergency request multi-step flow.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesEmergenciaSmokeDetector', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesEmergenciaSmokeDetector](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [ServicioEmergenciaId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TiposProblema] [nvarchar](200) NOT NULL,
        [UbicacionesDetectores] [nvarchar](200) NOT NULL,
        [SituacionActual] [nvarchar](30) NOT NULL,
        [PuedePermanecerAdentro] [nvarchar](20) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [AccesoPropiedad] [nvarchar](30) NULL,
        [TelefonoContacto] [nvarchar](30) NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesEmergenciaSmokeDetector] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaSmokeDetector_TiposProblema]
        DEFAULT (N'SmokeDetectorBeeping') FOR [TiposProblema];

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaSmokeDetector_UbicacionesDetectores]
        DEFAULT (N'Hallway') FOR [UbicacionesDetectores];

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaSmokeDetector_SituacionActual]
        DEFAULT (N'IntermittentChirp') FOR [SituacionActual];

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaSmokeDetector_PuedePermanecerAdentro]
        DEFAULT (N'Yes') FOR [PuedePermanecerAdentro];

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaSmokeDetector_Urgencia]
        DEFAULT (N'Emergency') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaSmokeDetector_AccesoPropiedad]
        DEFAULT (N'AdultHomeNow') FOR [AccesoPropiedad];

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaSmokeDetector_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        ADD CONSTRAINT [DF_SolicitudesEmergenciaSmokeDetector_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaSmokeDetector_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaSmokeDetector_ServiciosEmergencia]
        FOREIGN KEY([ServicioEmergenciaId]) REFERENCES [dbo].[ServiciosEmergencia] ([Id]);

    ALTER TABLE [dbo].[SolicitudesEmergenciaSmokeDetector]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesEmergenciaSmokeDetector_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesEmergenciaSmokeDetector_UserId_ServicioEmergenciaId]
        ON [dbo].[SolicitudesEmergenciaSmokeDetector] ([UserId], [ServicioEmergenciaId], [Estado]);

    PRINT 'Table SolicitudesEmergenciaSmokeDetector created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesEmergenciaSmokeDetector already exists.';
END
GO
