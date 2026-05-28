/*
  Cleaning Pro microservice flow — landing + solicitudes.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.CleaningProServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CleaningProServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MicroservicioId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_CleaningProServicioLanding_PageTitle] DEFAULT (N'Cleaning Pro'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](400) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_CleaningProServicioLanding_PrecioDesde] DEFAULT (35),
        [PrecioTexto] [nvarchar](120) NULL,
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [InfoBoxTexto] [nvarchar](400) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_CleaningProServicioLanding_CtaTexto] DEFAULT (N'Customize cleaning'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_CleaningProServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_CleaningProServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_CleaningProServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_CleaningProServicioLanding_MicroservicioId]
        ON dbo.CleaningProServicioLanding ([MicroservicioId]);

    ALTER TABLE [dbo].[CleaningProServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_CleaningProServicioLanding_Microservicios]
        FOREIGN KEY([MicroservicioId]) REFERENCES [dbo].[Microservicios] ([Id]);

    PRINT 'Table CleaningProServicioLanding created.';
END
ELSE
BEGIN
    PRINT 'Table CleaningProServicioLanding already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesCleaningPro', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesCleaningPro](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MicroservicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [Frecuencia] [nvarchar](20) NULL,
        [CantidadLimpiadores] [nvarchar](10) NULL,
        [AreasLimpieza] [nvarchar](300) NULL,
        [HorasEstimadas] [decimal](4,1) NULL,
        [AddonsSeleccionados] [nvarchar](120) NULL,
        [NotasLimpiador] [nvarchar](500) NULL,
        [FechaServicio] [date] NULL,
        [VentanaHorario] [nvarchar](30) NULL,
        [TarifaHoraria] [decimal](10,2) NULL,
        [Subtotal] [decimal](10,2) NULL,
        [ImpuestoVenta] [decimal](10,2) NULL,
        [PrecioTotal] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesCleaningPro_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesCleaningPro_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesCleaningPro] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesCleaningPro]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesCleaningPro_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesCleaningPro]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesCleaningPro_Microservicios]
        FOREIGN KEY([MicroservicioId]) REFERENCES [dbo].[Microservicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesCleaningPro]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesCleaningPro_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesCleaningPro_UserId_Microservicio_Estado]
        ON [dbo].[SolicitudesCleaningPro] ([UserId], [MicroservicioId], [Estado]);

    PRINT 'Table SolicitudesCleaningPro created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesCleaningPro already exists.';
END
GO

DECLARE @CleaningProMicroId INT = (
    SELECT TOP 1 Id FROM dbo.Microservicios
    WHERE Nombre IN (N'Cleaning Pro', N'Spotless Home Pro', N'Hogar Impecable Pro')
    ORDER BY Id
);

IF @CleaningProMicroId IS NULL
BEGIN
    SET @CleaningProMicroId = 4;
END

IF @CleaningProMicroId IS NOT NULL
BEGIN
    UPDATE dbo.Microservicios SET Nombre = N'Cleaning Pro' WHERE Id = @CleaningProMicroId;

    IF NOT EXISTS (SELECT 1 FROM dbo.CleaningProServicioLanding WHERE MicroservicioId = @CleaningProMicroId)
    BEGIN
        INSERT INTO dbo.CleaningProServicioLanding (
            MicroservicioId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, PrecioTexto, IncluyeItems, IncluyeIconos, InfoBoxTexto, CtaTexto, Activo)
        VALUES (
            @CleaningProMicroId,
            N'Cleaning Pro',
            N'Cleaning Pro',
            N'Customized cleaning, your way.',
            N'Flexible booking. Professional cleaners. Spotless results.',
            N'/limpieza.jpeg',
            35,
            N'From $35/hr per cleaner',
            N'Background-checked cleaners|Flexible scheduling|Professional supplies',
            N'fa-user-shield|fa-calendar-check|fa-spray-can-sparkles',
            N'All cleaners are background-checked & professional.',
            N'Customize cleaning',
            1);
        PRINT 'CleaningProServicioLanding seeded for Cleaning Pro.';
    END
END
GO

PRINT 'Cleaning Pro flow schema ready.';
GO
