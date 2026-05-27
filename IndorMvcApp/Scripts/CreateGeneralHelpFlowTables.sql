/*
  General Help flow — solicitudes + photo uploads.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesGeneralHelp', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesGeneralHelp](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [TipoAyuda] [nvarchar](30) NULL,
        [VentanaTiempo] [nvarchar](30) NULL,
        [Urgencia] [nvarchar](20) NULL,
        [Descripcion] [nvarchar](500) NULL,
        [ContactoPreferido] [nvarchar](20) NULL,
        [NotasAcceso] [nvarchar](120) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesGeneralHelp_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesGeneralHelp_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesGeneralHelp] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesGeneralHelp]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesGeneralHelp_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesGeneralHelp]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesGeneralHelp_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesGeneralHelp]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesGeneralHelp_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesGeneralHelp_UserId_Servicio_Estado]
        ON [dbo].[SolicitudesGeneralHelp] ([UserId], [MovingSetupServicioId], [Estado]);

    PRINT 'Table SolicitudesGeneralHelp created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesGeneralHelp already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosGeneralHelp', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosGeneralHelp](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudGeneralHelpId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosGeneralHelp_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosGeneralHelp] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosGeneralHelp]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosGeneralHelp_SolicitudesGeneralHelp]
        FOREIGN KEY([SolicitudGeneralHelpId]) REFERENCES [dbo].[SolicitudesGeneralHelp] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ArchivosGeneralHelp]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosGeneralHelp_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    PRINT 'Table ArchivosGeneralHelp created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosGeneralHelp already exists.';
END
GO

DECLARE @GhServicioId INT = (
    SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'General Help' ORDER BY Id
);

IF @GhServicioId IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'GeneralHelp',
        LinkAction = N'GeneralHelpRequest',
        LinkRouteId = Id
    WHERE Id = @GhServicioId
      AND (LinkController IS NULL OR LinkController = N'MovingSetup');

    PRINT 'MovingSetupServicios link updated for General Help.';
END
GO

PRINT 'General Help flow schema ready.';
GO
