/*
  Seed My Home mockup data (History, Providers, Maintenance, Documents).
  Run AFTER CreateMyHomeTables.sql.

  Uses the most recently created active property by default.
  Or set @PropiedadId manually before running.
*/

DECLARE @PropiedadId INT = (
    SELECT TOP 1 Id FROM dbo.Propiedades WHERE Activo = 1 ORDER BY FechaCreacion DESC
);

IF @PropiedadId IS NULL
BEGIN
    RAISERROR('No active property found in Propiedades.', 16, 1);
    RETURN;
END

PRINT 'Seeding My Home data for PropiedadId = ' + CAST(@PropiedadId AS nvarchar(20));

-- Skip if already seeded for this property
IF EXISTS (SELECT 1 FROM dbo.PropiedadHistorial WHERE PropiedadId = @PropiedadId)
BEGIN
    PRINT 'History already exists for this property. Skipping seed.';
    RETURN;
END

DECLARE @KitchenProId INT, @RoofingId INT, @HvacId INT, @PaintId INT, @PlumbingId INT;

INSERT INTO dbo.PropiedadProveedores (PropiedadId, Name, ServiceCategory, Phone, Source)
VALUES
(@PropiedadId, N'Kitchen Pro LLC', N'Kitchen remodeling', N'(407) 555-1200', N'Seed'),
(@PropiedadId, N'Roofing Solutions', N'Roofs', N'(407) 555-3344', N'Seed'),
(@PropiedadId, N'Cool Air Conditioning', N'HVAC', N'(407) 555-8877', N'Seed'),
(@PropiedadId, N'Color World Inc.', N'Painting', N'(407) 555-4432', N'Seed'),
(@PropiedadId, N'Sunshine Plumbing', N'Plumbing', N'(407) 555-7654', N'Seed');

SELECT @KitchenProId = Id FROM dbo.PropiedadProveedores WHERE PropiedadId = @PropiedadId AND Name = N'Kitchen Pro LLC';
SELECT @RoofingId = Id FROM dbo.PropiedadProveedores WHERE PropiedadId = @PropiedadId AND Name = N'Roofing Solutions';
SELECT @HvacId = Id FROM dbo.PropiedadProveedores WHERE PropiedadId = @PropiedadId AND Name = N'Cool Air Conditioning';
SELECT @PaintId = Id FROM dbo.PropiedadProveedores WHERE PropiedadId = @PropiedadId AND Name = N'Color World Inc.';

INSERT INTO dbo.PropiedadHistorial (PropiedadId, RecordType, Title, ProviderName, PropiedadProveedorId, CompletionDate, TotalCost, Description, WarrantyStatus, Source)
VALUES
(@PropiedadId, N'Improvement', N'Kitchen remodel', N'Kitchen Pro LLC', @KitchenProId, '2024-05-15', 28500.00,
 N'Complete kitchen renovation including cabinets, quartz countertops, appliances, and floors.', N'Active', N'Seed'),
(@PropiedadId, N'Repair', N'New roof', N'Roofing Solutions', @RoofingId, '2023-01-20', 12400.00,
 N'Full roof replacement with architectural shingles.', N'Active', N'Seed'),
(@PropiedadId, N'Improvement', N'AC installation', N'Cool Air Conditioning', @HvacId, '2021-07-10', 8900.00,
 N'New central air conditioning system installed.', N'Active', N'Seed'),
(@PropiedadId, N'Improvement', N'Exterior painting', N'Color World Inc.', @PaintId, '2020-03-05', 4200.00,
 N'Exterior paint refresh for all siding and trim.', N'Expired', N'Seed');

INSERT INTO dbo.PropiedadMantenimiento (PropiedadId, Title, DueDate, Status, Notes)
VALUES
(@PropiedadId, N'Check AC system', '2024-06-15', N'Upcoming', N'Annual HVAC tune-up'),
(@PropiedadId, N'Clean gutters', '2024-06-30', N'Upcoming', NULL),
(@PropiedadId, N'Roof inspection', '2024-07-12', N'Upcoming', NULL),
(@PropiedadId, N'Heating service', '2024-08-20', N'Upcoming', NULL),
(@PropiedadId, N'Change HVAC filter', '2024-08-28', N'Upcoming', NULL),
(@PropiedadId, N'Smoke detector check', '2024-09-10', N'Upcoming', NULL);

INSERT INTO dbo.PropiedadDocumentos (PropiedadId, Category, Title, FileName, ContentType, SizeBytes)
VALUES
(@PropiedadId, N'Warranties', N'Kitchen remodel warranty', N'warranty-kitchen.pdf', N'application/pdf', 245760),
(@PropiedadId, N'Warranties', N'Roof warranty', N'warranty-roof.pdf', N'application/pdf', 198000),
(@PropiedadId, N'Warranties', N'HVAC warranty', N'warranty-hvac.pdf', N'application/pdf', 156000),
(@PropiedadId, N'Warranties', N'Paint warranty', N'warranty-paint.pdf', N'application/pdf', 120000),
(@PropiedadId, N'Warranties', N'Appliance warranty', N'warranty-appliance.pdf', N'application/pdf', 98000),
(@PropiedadId, N'Warranties', N'Water heater warranty', N'warranty-water-heater.pdf', N'application/pdf', 87000),
(@PropiedadId, N'Warranties', N'Garage door warranty', N'warranty-garage.pdf', N'application/pdf', 76000),
(@PropiedadId, N'Warranties', N'Window warranty', N'warranty-windows.pdf', N'application/pdf', 65000),
(@PropiedadId, N'Permits', N'Building permit', N'permit-construction.pdf', N'application/pdf', 1228800),
(@PropiedadId, N'Permits', N'Electrical permit', N'permit-electrical.pdf', N'application/pdf', 890000),
(@PropiedadId, N'Permits', N'Plumbing permit', N'permit-plumbing.pdf', N'application/pdf', 760000),
(@PropiedadId, N'Permits', N'Roof permit', N'permit-roof.pdf', N'application/pdf', 540000),
(@PropiedadId, N'Permits', N'HVAC permit', N'permit-hvac.pdf', N'application/pdf', 430000),
(@PropiedadId, N'Invoices', N'Invoice #INV-2024-0515', N'INV-2024-0515.pdf', N'application/pdf', 250880),
(@PropiedadId, N'Invoices', N'Roof invoice 2023', N'roof-invoice.pdf', N'application/pdf', 210000),
(@PropiedadId, N'Invoices', N'HVAC invoice 2021', N'hvac-invoice.pdf', N'application/pdf', 180000),
(@PropiedadId, N'Invoices', N'Paint invoice 2020', N'paint-invoice.pdf', N'application/pdf', 95000),
(@PropiedadId, N'Invoices', N'Plumbing invoice', N'plumbing-invoice.pdf', N'application/pdf', 88000),
(@PropiedadId, N'Invoices', N'Inspection invoice', N'inspection-invoice.pdf', N'application/pdf', 72000),
(@PropiedadId, N'Invoices', N'Landscaping invoice', N'landscape-invoice.pdf', N'application/pdf', 65000),
(@PropiedadId, N'Invoices', N'Gutter cleaning invoice', N'gutter-invoice.pdf', N'application/pdf', 54000),
(@PropiedadId, N'Invoices', N'Pest control invoice', N'pest-invoice.pdf', N'application/pdf', 48000),
(@PropiedadId, N'Invoices', N'Security system invoice', N'security-invoice.pdf', N'application/pdf', 42000),
(@PropiedadId, N'Invoices', N'Pool service invoice', N'pool-invoice.pdf', N'application/pdf', 38000),
(@PropiedadId, N'Invoices', N'Generator service invoice', N'generator-invoice.pdf', N'application/pdf', 35000),
(@PropiedadId, N'Contracts', N'Kitchen contractor agreement', N'contract-kitchen.pdf', N'application/pdf', 320000),
(@PropiedadId, N'Contracts', N'Roofing contract', N'contract-roof.pdf', N'application/pdf', 280000),
(@PropiedadId, N'Contracts', N'HVAC service agreement', N'contract-hvac.pdf', N'application/pdf', 210000),
(@PropiedadId, N'Contracts', N'Painting contract', N'contract-paint.pdf', N'application/pdf', 160000),
(@PropiedadId, N'Contracts', N'Home warranty plan', N'contract-warranty.pdf', N'application/pdf', 140000),
(@PropiedadId, N'Contracts', N'Lawn care contract', N'contract-lawn.pdf', N'application/pdf', 98000),
(@PropiedadId, N'Inspections', N'Home inspection report', N'inspection-home.pdf', N'application/pdf', 890000),
(@PropiedadId, N'Inspections', N'Roof inspection', N'inspection-roof.pdf', N'application/pdf', 540000),
(@PropiedadId, N'Inspections', N'Electrical inspection', N'inspection-electrical.pdf', N'application/pdf', 430000),
(@PropiedadId, N'Inspections', N'4-point inspection', N'inspection-4point.pdf', N'application/pdf', 320000),
(@PropiedadId, N'Manuals', N'HVAC owner manual', N'manual-hvac.pdf', N'application/pdf', 2100000),
(@PropiedadId, N'Manuals', N'Water heater manual', N'manual-water-heater.pdf', N'application/pdf', 1800000),
(@PropiedadId, N'Manuals', N'Garage door manual', N'manual-garage.pdf', N'application/pdf', 1200000),
(@PropiedadId, N'Manuals', N'Smoke detector manual', N'manual-smoke.pdf', N'application/pdf', 890000),
(@PropiedadId, N'Manuals', N'Thermostat manual', N'manual-thermostat.pdf', N'application/pdf', 760000),
(@PropiedadId, N'Manuals', N'Dishwasher manual', N'manual-dishwasher.pdf', N'application/pdf', 650000),
(@PropiedadId, N'Manuals', N'Refrigerator manual', N'manual-fridge.pdf', N'application/pdf', 540000),
(@PropiedadId, N'Other', N'Property photos archive', N'photos.zip', N'application/zip', 18874368),
(@PropiedadId, N'Other', N'Insurance declaration', N'insurance-dec.pdf', N'application/pdf', 320000),
(@PropiedadId, N'Other', N'HOA guidelines', N'hoa-guidelines.pdf', N'application/pdf', 280000);

PRINT 'My Home mockup data seeded successfully.';
GO
