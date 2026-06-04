-- Optional: membership plans aligned with INDOR mockups (Free, Filter, Home Care, Premium).
-- Run after CreateMoreSectionTables.sql if you want mockup plan names and prices.

IF OBJECT_ID(N'dbo.PlanesMembresia', N'U') IS NULL
BEGIN
    RAISERROR('Run CreateMoreSectionTables.sql first.', 16, 1);
    RETURN;
END
GO

DELETE FROM dbo.MembresiasUsuario;
DELETE FROM dbo.PlanesMembresia;
DBCC CHECKIDENT ('dbo.PlanesMembresia', RESEED, 0);
GO

INSERT INTO dbo.PlanesMembresia (Nombre, Subtitulo, Descripcion, PrecioMensual, Moneda, Caracteristicas, Orden, Activo, Recomendado)
VALUES
(N'Free', N'Basic reminders only.', N'Get started with maintenance reminders at no cost.',
 0, N'USD', N'Maintenance reminders', 1, 1, 0),
(N'Filter Plan', N'Best for easy home care.', N'Filter delivery and maintenance reminders.',
 10, N'USD', N'Filter delivery every 3 months|Maintenance reminders', 2, 1, 0),
(N'Home Care Plan', N'Your home always protected.', N'Everything in Filter Plan plus service discounts.',
 15, N'USD', N'Everything in Filter Plan|5% discount on services', 3, 1, 0),
(N'Premium Care Plan', N'Best for proactive homeowners.', N'Full coverage with inspection and priority support.',
 30, N'USD', N'Everything in Home Care Plan|10% discount on services|1 annual home inspection', 4, 1, 1);
-- Home Care = default selection in UI; Premium = "Most popular" badge
UPDATE dbo.PlanesMembresia SET Recomendado = 0;
UPDATE dbo.PlanesMembresia SET Recomendado = 1 WHERE Nombre = N'Premium Care Plan';
GO

PRINT 'INDOR membership plans seeded.';
