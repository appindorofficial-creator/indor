/*
  Always Perfect Lawn microservice flow — landing + solicitudes.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.LawnServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[LawnServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MicroservicioId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_LawnServicioLanding_PageTitle] DEFAULT (N'Always Perfect Lawn'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](400) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_LawnServicioLanding_PrecioDesde] DEFAULT (45),
        [PrecioTexto] [nvarchar](120) NULL,
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [InfoBoxTexto] [nvarchar](400) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_LawnServicioLanding_CtaTexto] DEFAULT (N'Customize service'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_LawnServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_LawnServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_LawnServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_LawnServicioLanding_MicroservicioId]
        ON dbo.LawnServicioLanding ([MicroservicioId]);

    ALTER TABLE [dbo].[LawnServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_LawnServicioLanding_Microservicios]
        FOREIGN KEY([MicroservicioId]) REFERENCES [dbo].[Microservicios] ([Id]);

    PRINT 'Table LawnServicioLanding created.';
END
ELSE
BEGIN
    PRINT 'Table LawnServicioLanding already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesLawn', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesLawn](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MicroservicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [TipoServicio] [nvarchar](20) NOT NULL CONSTRAINT [DF_SolicitudesLawn_TipoServicio] DEFAULT (N'Subscription'),
        [Frecuencia] [nvarchar](20) NULL,
        [AreaServicio] [nvarchar](30) NULL,
        [AddonsSeleccionados] [nvarchar](300) NULL,
        [PreferenciaExtra] [nvarchar](30) NULL,
        [FechaPreferida] [date] NULL,
        [VentanaHorario] [nvarchar](30) NULL,
        [PrecioBase] [decimal](10,2) NULL,
        [PrecioAddons] [decimal](10,2) NULL,
        [DescuentoSuscripcion] [decimal](10,2) NULL,
        [PrecioTotal] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesLawn_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesLawn_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesLawn] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesLawn]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesLawn_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesLawn]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesLawn_Microservicios]
        FOREIGN KEY([MicroservicioId]) REFERENCES [dbo].[Microservicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesLawn]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesLawn_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesLawn_UserId_Microservicio_Estado]
        ON [dbo].[SolicitudesLawn] ([UserId], [MicroservicioId], [Estado]);

    PRINT 'Table SolicitudesLawn created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesLawn already exists.';
END
GO

DECLARE @LawnMicroId INT = (
    SELECT TOP 1 Id FROM dbo.Microservicios WHERE Nombre = N'Always Perfect Lawn' ORDER BY Id
);

IF @LawnMicroId IS NULL
BEGIN
    SET @LawnMicroId = 2;
END

IF @LawnMicroId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.LawnServicioLanding WHERE MicroservicioId = @LawnMicroId)
    BEGIN
        INSERT INTO dbo.LawnServicioLanding (
            MicroservicioId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, PrecioTexto, IncluyeItems, IncluyeIconos, InfoBoxTexto, CtaTexto, Activo)
        VALUES (
            @LawnMicroId,
            N'Always Perfect Lawn',
            N'Always Perfect Lawn',
            N'Your yard, always under control.',
            N'Flexible lawn mowing and yard upkeep for one-time or recurring service.',
            N'/cesped.jpeg',
            45,
            N'From $45 USD',
            N'Front yard mowing|Back yard mowing|Basic cleanup',
            N'fa-check|fa-check|fa-check',
            N'Pricing updates based on selected areas.',
            N'Customize service',
            1);
        PRINT 'LawnServicioLanding seeded for Always Perfect Lawn.';
    END
END
GO

PRINT 'Always Perfect Lawn flow schema ready.';
GO
