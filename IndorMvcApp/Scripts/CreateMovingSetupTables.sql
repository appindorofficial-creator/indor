/*
  Moving Setup — section config, service grid, and quick links for Home / Services.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.MovingSetupConfig', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MovingSetupConfig](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Titulo] [nvarchar](80) NOT NULL,
        [Subtitulo] [nvarchar](200) NOT NULL,
        [IconoClase] [nvarchar](50) NOT NULL CONSTRAINT [DF_MovingSetupConfig_IconoClase] DEFAULT (N'fa-box-open'),
        [ViewAllTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_MovingSetupConfig_ViewAllTexto] DEFAULT (N'View all'),
        [ViewAllUrl] [nvarchar](200) NULL,
        [FeaturedEtiqueta] [nvarchar](40) NOT NULL CONSTRAINT [DF_MovingSetupConfig_FeaturedEtiqueta] DEFAULT (N'FEATURED'),
        [FeaturedTitulo] [nvarchar](120) NOT NULL,
        [FeaturedDescripcion] [nvarchar](300) NOT NULL,
        [FeaturedImagenUrl] [nvarchar](300) NULL,
        [FeaturedCaracteristicas] [nvarchar](500) NULL,
        [FeaturedIconosCaracteristicas] [nvarchar](200) NULL,
        [FeaturedCtaTexto] [nvarchar](80) NOT NULL CONSTRAINT [DF_MovingSetupConfig_FeaturedCtaTexto] DEFAULT (N'Start moving setup'),
        [FeaturedCtaController] [nvarchar](80) NULL,
        [FeaturedCtaAction] [nvarchar](80) NULL,
        [FeaturedCtaRouteId] [int] NULL,
        [Activo] [bit] NOT NULL CONSTRAINT [DF_MovingSetupConfig_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_MovingSetupConfig_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_MovingSetupConfig] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Table MovingSetupConfig created.';
END
GO

IF OBJECT_ID(N'dbo.MovingSetupServicios', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MovingSetupServicios](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](80) NOT NULL,
        [IconoClase] [nvarchar](50) NOT NULL CONSTRAINT [DF_MovingSetupServicios_IconoClase] DEFAULT (N'fa-house'),
        [LinkController] [nvarchar](80) NULL,
        [LinkAction] [nvarchar](80) NULL,
        [LinkRouteId] [int] NULL,
        [Orden] [int] NOT NULL CONSTRAINT [DF_MovingSetupServicios_Orden] DEFAULT (0),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_MovingSetupServicios_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_MovingSetupServicios_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_MovingSetupServicios] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_MovingSetupServicios_Nombre] ON dbo.MovingSetupServicios ([Nombre]);
    PRINT 'Table MovingSetupServicios created.';
END
GO

IF OBJECT_ID(N'dbo.MovingSetupEnlacesRapidos', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MovingSetupEnlacesRapidos](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](80) NOT NULL,
        [IconoClase] [nvarchar](50) NOT NULL CONSTRAINT [DF_MovingSetupEnlacesRapidos_IconoClase] DEFAULT (N'fa-clipboard-list'),
        [LinkController] [nvarchar](80) NULL,
        [LinkAction] [nvarchar](80) NULL,
        [LinkRouteId] [int] NULL,
        [LinkUrl] [nvarchar](300) NULL,
        [Orden] [int] NOT NULL CONSTRAINT [DF_MovingSetupEnlacesRapidos_Orden] DEFAULT (0),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_MovingSetupEnlacesRapidos_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_MovingSetupEnlacesRapidos_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_MovingSetupEnlacesRapidos] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_MovingSetupEnlacesRapidos_Nombre] ON dbo.MovingSetupEnlacesRapidos ([Nombre]);
    PRINT 'Table MovingSetupEnlacesRapidos created.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MovingSetupConfig)
BEGIN
    INSERT INTO dbo.MovingSetupConfig (
        Titulo, Subtitulo, IconoClase, ViewAllTexto, FeaturedTitulo, FeaturedDescripcion,
        FeaturedImagenUrl, FeaturedCaracteristicas, FeaturedIconosCaracteristicas,
        FeaturedCtaTexto, FeaturedCtaController, FeaturedCtaAction, Activo)
    VALUES (
        N'Moving Setup',
        N'Everything you need before, during, and after your move.',
        N'fa-box-open',
        N'View all',
        N'Moving Assistant',
        N'Book moving, cleaning, setup, and utility help in one place.',
        N'/inspeccion2.jpeg',
        N'Fast booking|Trusted pros|Transparent pricing',
        N'fa-bolt|fa-shield-check|fa-tags',
        N'Start moving setup',
        N'MovingSetup',
        N'Index',
        1);
    PRINT 'MovingSetupConfig seeded.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MovingSetupServicios)
BEGIN
    INSERT INTO dbo.MovingSetupServicios (Nombre, IconoClase, LinkController, LinkAction, Orden, Activo)
    VALUES
    (N'Moving', N'fa-truck-ramp-box', N'MovingSetup', N'Index', 1, 1),
    (N'Cleaning', N'fa-broom', N'MovingSetup', N'Index', 2, 1),
    (N'Packing Help', N'fa-box-open', N'MovingSetup', N'Index', 3, 1),
    (N'Furniture & Assembly', N'fa-couch', N'MovingSetup', N'Index', 4, 1),
    (N'TV Wall Mounting', N'fa-tv', N'MovingSetup', N'Index', 5, 1),
    (N'Utilities Setup', N'fa-wifi', N'MovingSetup', N'Index', 6, 1),
    (N'General Help', N'fa-user-group', N'MovingSetup', N'Index', 7, 1);
    PRINT 'MovingSetupServicios seeded.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MovingSetupEnlacesRapidos)
BEGIN
    INSERT INTO dbo.MovingSetupEnlacesRapidos (Nombre, IconoClase, LinkController, LinkAction, Orden, Activo)
    VALUES
    (N'Address checklist', N'fa-clipboard-list', N'MovingSetup', N'Index', 1, 1),
    (N'Supplies', N'fa-box', N'MovingSetup', N'Index', 2, 1),
    (N'Donation pickup', N'fa-hand-holding-heart', N'MovingSetup', N'Index', 3, 1),
    (N'Move tips', N'fa-lightbulb', N'MovingSetup', N'Index', 4, 1);
    PRINT 'MovingSetupEnlacesRapidos seeded.';
END
GO

PRINT 'Moving Setup schema ready.';
GO
