/*
  Stress-Free Trash microservice flow — landing + solicitudes.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.TrashServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TrashServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MicroservicioId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_TrashServicioLanding_PageTitle] DEFAULT (N'Trash Day Assistant'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](400) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_TrashServicioLanding_PrecioDesde] DEFAULT (20),
        [PrecioTexto] [nvarchar](120) NULL,
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [InfoBoxTexto] [nvarchar](400) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_TrashServicioLanding_CtaTexto] DEFAULT (N'Activate service'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_TrashServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_TrashServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_TrashServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_TrashServicioLanding_MicroservicioId]
        ON dbo.TrashServicioLanding ([MicroservicioId]);

    ALTER TABLE [dbo].[TrashServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_TrashServicioLanding_Microservicios]
        FOREIGN KEY([MicroservicioId]) REFERENCES [dbo].[Microservicios] ([Id]);

    PRINT 'Table TrashServicioLanding created.';
END
ELSE
BEGIN
    PRINT 'Table TrashServicioLanding already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesTrash', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesTrash](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MicroservicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [BinsSeleccionados] [nvarchar](120) NULL,
        [CantidadBins] [nvarchar](10) NULL,
        [Frecuencia] [nvarchar](20) NULL,
        [DiaRecoleccion] [nvarchar](10) NULL,
        [TipoAyuda] [nvarchar](30) NULL,
        [RecordatorioCuando] [nvarchar](20) NULL,
        [VentanaRecoleccion] [nvarchar](30) NULL,
        [NotasEspeciales] [nvarchar](500) NULL,
        [PrecioMensual] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesTrash_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesTrash_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesTrash] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesTrash]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesTrash_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesTrash]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesTrash_Microservicios]
        FOREIGN KEY([MicroservicioId]) REFERENCES [dbo].[Microservicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesTrash]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesTrash_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesTrash_UserId_Microservicio_Estado]
        ON [dbo].[SolicitudesTrash] ([UserId], [MicroservicioId], [Estado]);

    PRINT 'Table SolicitudesTrash created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesTrash already exists.';
END
GO

DECLARE @TrashMicroId INT = (
    SELECT TOP 1 Id FROM dbo.Microservicios WHERE Nombre = N'Stress-Free Trash' ORDER BY Id
);

IF @TrashMicroId IS NULL
BEGIN
    SET @TrashMicroId = 3;
END

IF @TrashMicroId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.TrashServicioLanding WHERE MicroservicioId = @TrashMicroId)
    BEGIN
        INSERT INTO dbo.TrashServicioLanding (
            MicroservicioId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, PrecioTexto, IncluyeItems, IncluyeIconos, InfoBoxTexto, CtaTexto, Activo)
        VALUES (
            @TrashMicroId,
            N'Trash Day Assistant',
            N'Stress-Free Trash',
            N'Never forget collection day again.',
            N'Forget fines, bad odors, or missed pickups. We make sure your trash is ready on the right day and return bins to their place.',
            N'/basura.jpeg',
            20,
            N'From $20 /mo',
            N'Place bins out on pickup day|Return bins to place|Punctual, reliable service',
            N'fa-check|fa-check|fa-check',
            N'You can edit your pickup day anytime. Changes will apply to future collections.',
            N'Activate service',
            1);
        PRINT 'TrashServicioLanding seeded for Stress-Free Trash.';
    END
END
GO

PRINT 'Stress-Free Trash flow schema ready.';
GO
