/*
  OPCIONAL — Datos de demo para INDOR PRO (dashboard, leads, jobs, invoices, etc.).

  NO ejecutar durante el setup inicial de tablas si aún no hay proveedores registrados.
  Los registros reales se crean cuando un proveedor completa el registro en la app.

  Orden recomendado en desarrollo:
    1) CreateProviderPortalTables.sql (si aplica)
    2) CreateProviderOperationsTables.sql + todos los AlterProvider*.sql (incl. InvoicesExtended + InvoicesPayment)
    3) Registrar un proveedor en la aplicación (flujo ProviderRegistration)
    4) ENTONCES ejecutar este script para cargar datos de prueba en ESE proveedor

  El portal funciona sin este seed: mostrará métricas en cero y listas vacías.

  Opcional: asignar @ProveedorEmail al correo del proveedor que registraste.
  Opcional: asignar @ProveedorId manualmente si ya conoces el Id.
*/

DECLARE @ProveedorEmail NVARCHAR(256) = NULL;  -- ej. N'tu-correo@empresa.com'
DECLARE @ProveedorId INT = NULL;               -- ej. 1 (dejar NULL para auto-detectar)

IF @ProveedorId IS NULL AND @ProveedorEmail IS NOT NULL
    SELECT TOP 1 @ProveedorId = Id
    FROM dbo.IndorProveedores
    WHERE Email = @ProveedorEmail
    ORDER BY FechaCreacion DESC;

IF @ProveedorId IS NULL
    SELECT TOP 1 @ProveedorId = Id
    FROM dbo.IndorProveedores
    WHERE UserId IS NOT NULL
    ORDER BY FechaCreacion DESC;

IF @ProveedorId IS NULL
    SELECT TOP 1 @ProveedorId = Id FROM dbo.IndorProveedores ORDER BY FechaCreacion DESC;

IF @ProveedorId IS NULL
BEGIN
    PRINT '----------------------------------------------------------------';
    PRINT 'Seed omitido: no hay proveedores en IndorProveedores.';
    PRINT 'Esto es normal si aún no has registrado ningún proveedor en la app.';
    PRINT '';
    PRINT 'Qué hacer:';
    PRINT '  1) Deja las tablas creadas (scripts Create + Alter).';
    PRINT '  2) Registra un proveedor desde la aplicación.';
    PRINT '  3) Vuelve a ejecutar este script (opcional, solo datos de demo).';
    PRINT '----------------------------------------------------------------';
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.IndorProveedorLeads WHERE ProveedorId = @ProveedorId AND LeadCode = N'L-1042')
BEGIN
    PRINT 'Provider PRO operations already seeded for ProveedorId = ' + CAST(@ProveedorId AS nvarchar(20)) + '. Skipping.';
    RETURN;
END

PRINT 'Seeding Provider PRO operations for ProveedorId = ' + CAST(@ProveedorId AS nvarchar(20));

DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @MonthStart DATE = DATEFROMPARTS(YEAR(@Today), MONTH(@Today), 1);
DECLARE @ClienteJamesId INT;
DECLARE @LeadRoofId INT;

-- Provider score shown on dashboard
UPDATE dbo.IndorProveedores
SET ExamScorePercent = 87, ExamPassed = 1
WHERE Id = @ProveedorId;

-- Customers (32 homes protected)
INSERT INTO dbo.IndorProveedorClientes (ProveedorId, Name, Email, Phone, CityState, Address, IsPropertyVerified, IsAppConnected)
VALUES
(@ProveedorId, N'James Smith', N'james.smith@email.com', N'(512) 555-0142', N'Austin, TX', N'456 Oak Dr, Austin, TX 78704', 1, 1),
(@ProveedorId, N'Maria Lopez', N'maria@email.com', N'(512) 555-2201', N'Austin, TX', N'123 Main St, Austin, TX 78701', 1, 1),
(@ProveedorId, N'Robert Kim', N'robert@email.com', N'(512) 555-3302', N'Austin, TX', N'789 Pine Ave, Austin, TX 78702', 1, 0);

SET @ClienteJamesId = (SELECT Id FROM dbo.IndorProveedorClientes WHERE ProveedorId = @ProveedorId AND Name = N'James Smith');

DECLARE @i INT = 4;
WHILE @i <= 32
BEGIN
    INSERT INTO dbo.IndorProveedorClientes (ProveedorId, Name, Address, IsPropertyVerified, Activo)
    VALUES (@ProveedorId, N'Customer ' + CAST(@i AS nvarchar(10)), N'Austin, TX', 1, 1);
    SET @i += 1;
END

-- Leads (8 new) — one INSERT per row to avoid column-count mismatch (Msg 10709)
INSERT INTO dbo.IndorProveedorLeads (
    ProveedorId, Address, ServiceType, Urgency, Status, CustomerName, CustomerEmail, CustomerPhone,
    LeadCode, IsHomeownerVerified, ProblemDescription, ImageUrl, PhotosJson, DistanceMiles, TimelineNote,
    HomeType, SquareFeet, YearBuilt, Stories, AccessNotes,
    SuggestedScopeItemsJson, SuggestedLaborAmount, SuggestedMaterialsAmount,
    SuggestedTimeline, SuggestedWarranty, SuggestedHomeownerNotes,
    DefaultVisitType, DefaultAssignedTechnician, DefaultVisitNotes, DefaultVisitAt, DefaultVisitTimeLabel,
    DefaultChecklistJson, DefaultMaterialsUsedJson, DefaultLaborWarranty
)
VALUES (
    @ProveedorId, N'456 Oak Dr, Austin, TX 78704', N'Roof Repair', N'High Urgency', N'New',
    N'James Smith', N'james.smith@email.com', N'(512) 555-0142', N'L-1042', 1,
    N'Active roof leak near chimney flashing after recent storms.', N'/welcome-house.png',
    N'[{"url":"/welcome-house.png","label":"Before"},{"url":"/welcome-house.png","label":"Damage"}]',
    4.2, N'Repair within 1 week', N'Single Family', 2450, 1998, 2, N'Gate code 4521.',
    N'[{"label":"Leak Inspection","amount":250},{"label":"Shingle Repair","amount":1200},{"label":"Flashing Seal","amount":400}]',
    750, 1100, N'1 day', N'Workmanship Warranty: 1 Year Included', N'Estimate based on photos provided.',
    N'Verification Visit', N'Mike Johnson', N'Inspect roof leak and verify scope.',
    DATEADD(hour, 10, DATEADD(day, 1, CAST(@Today AS datetime2))), N'10:30 AM',
    N'[{"label":"Arrived on site","completed":false},{"label":"Safety check completed","completed":false},{"label":"Inspect chimney flashing","completed":false}]',
    N'[{"name":"Architectural Shingles","quantity":12},{"name":"Flashing Sealant","quantity":1}]',
    N'Labor Warranty: 1 Year'
);

INSERT INTO dbo.IndorProveedorLeads (
    ProveedorId, Address, ServiceType, Urgency, Status, CustomerName, CustomerEmail, CustomerPhone,
    LeadCode, IsHomeownerVerified, ProblemDescription, ImageUrl, PhotosJson, DistanceMiles, TimelineNote,
    HomeType, SquareFeet, YearBuilt, Stories, AccessNotes,
    SuggestedScopeItemsJson, SuggestedLaborAmount, SuggestedMaterialsAmount,
    SuggestedTimeline, SuggestedWarranty, SuggestedHomeownerNotes,
    DefaultVisitType, DefaultAssignedTechnician, DefaultVisitNotes, DefaultVisitAt, DefaultVisitTimeLabel,
    DefaultChecklistJson, DefaultMaterialsUsedJson, DefaultLaborWarranty
)
VALUES (
    @ProveedorId, N'892 Maple Ln, Austin, TX 78745', N'Plumbing', N'Standard', N'New',
    N'Sarah Chen', N'sarah@email.com', N'(512) 555-8821', N'L-1043', 1,
    N'Kitchen sink slow drain.', N'/welcome-house.png', NULL, 6.8, NULL,
    N'Single Family', 1800, 2005, 1, NULL,
    N'[{"label":"Leak Detection","amount":175},{"label":"Pipe Repair","amount":650}]',
    425, 380, N'Half day', N'Workmanship Warranty: 90 Days', NULL,
    N'Estimate Visit', N'Lisa Park', N'Inspect drain line.',
    DATEADD(hour, 14, DATEADD(day, 1, CAST(@Today AS datetime2))), N'2:00 PM',
    NULL, NULL, NULL
);

INSERT INTO dbo.IndorProveedorLeads (
    ProveedorId, Address, ServiceType, Urgency, Status, CustomerName, CustomerEmail, CustomerPhone,
    LeadCode, IsHomeownerVerified, ProblemDescription, ImageUrl, PhotosJson, DistanceMiles, TimelineNote,
    HomeType, SquareFeet, YearBuilt, Stories, AccessNotes,
    SuggestedScopeItemsJson, SuggestedLaborAmount, SuggestedMaterialsAmount,
    SuggestedTimeline, SuggestedWarranty, SuggestedHomeownerNotes,
    DefaultVisitType, DefaultAssignedTechnician, DefaultVisitNotes, DefaultVisitAt, DefaultVisitTimeLabel,
    DefaultChecklistJson, DefaultMaterialsUsedJson, DefaultLaborWarranty
)
VALUES (
    @ProveedorId, N'123 Main St, Austin, TX 78701', N'Roof Repair', N'High Urgency', N'New',
    N'Maria Lopez', N'maria@email.com', N'(512) 555-2201', N'L-1044', 1,
    N'Missing shingles on south slope.', N'/welcome-house.png', NULL, 2.1, NULL,
    NULL, NULL, NULL, NULL, NULL,
    N'[{"label":"Shingle Replacement","amount":1800}]', 600, 900, N'1 day', N'1 Year Warranty', NULL,
    N'Estimate Visit', N'Mike Johnson', NULL, NULL, NULL, NULL, NULL, NULL
);

INSERT INTO dbo.IndorProveedorLeads (
    ProveedorId, Address, ServiceType, Urgency, Status, CustomerName, CustomerEmail, CustomerPhone,
    LeadCode, IsHomeownerVerified, ProblemDescription, ImageUrl, PhotosJson, DistanceMiles, TimelineNote,
    HomeType, SquareFeet, YearBuilt, Stories, AccessNotes,
    SuggestedScopeItemsJson, SuggestedLaborAmount, SuggestedMaterialsAmount,
    SuggestedTimeline, SuggestedWarranty, SuggestedHomeownerNotes,
    DefaultVisitType, DefaultAssignedTechnician, DefaultVisitNotes, DefaultVisitAt, DefaultVisitTimeLabel,
    DefaultChecklistJson, DefaultMaterialsUsedJson, DefaultLaborWarranty
)
VALUES (
    @ProveedorId, N'210 Cedar Ct, Austin, TX 78703', N'HVAC', N'Medium Urgency', N'New',
    N'Robert Kim', N'robert@email.com', N'(512) 555-3302', N'L-1045', 1,
    N'AC not cooling upstairs.', N'/welcome-house.png', NULL, 5.5, NULL,
    NULL, NULL, NULL, NULL, NULL,
    N'[{"label":"Diagnostic","amount":150},{"label":"Refrigerant recharge","amount":400}]',
    300, 250, N'Same day', N'90 Day Warranty', NULL,
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
);

INSERT INTO dbo.IndorProveedorLeads (
    ProveedorId, Address, ServiceType, Urgency, Status, CustomerName, CustomerEmail, CustomerPhone,
    LeadCode, IsHomeownerVerified, ProblemDescription, ImageUrl, PhotosJson, DistanceMiles, TimelineNote,
    HomeType, SquareFeet, YearBuilt, Stories, AccessNotes,
    SuggestedScopeItemsJson, SuggestedLaborAmount, SuggestedMaterialsAmount,
    SuggestedTimeline, SuggestedWarranty, SuggestedHomeownerNotes,
    DefaultVisitType, DefaultAssignedTechnician, DefaultVisitNotes, DefaultVisitAt, DefaultVisitTimeLabel,
    DefaultChecklistJson, DefaultMaterialsUsedJson, DefaultLaborWarranty
)
VALUES (
    @ProveedorId, N'55 Birch Way, Austin, TX 78746', N'Gutter Cleaning', N'Standard', N'New',
    N'Anna Reed', NULL, N'(512) 555-4410', N'L-1046', 0,
    N'Annual gutter cleaning requested.', N'/welcome-house.png', NULL, 8.0, NULL,
    NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
);

INSERT INTO dbo.IndorProveedorLeads (
    ProveedorId, Address, ServiceType, Urgency, Status, CustomerName, CustomerEmail, CustomerPhone,
    LeadCode, IsHomeownerVerified, ProblemDescription, ImageUrl, PhotosJson, DistanceMiles, TimelineNote,
    HomeType, SquareFeet, YearBuilt, Stories, AccessNotes,
    SuggestedScopeItemsJson, SuggestedLaborAmount, SuggestedMaterialsAmount,
    SuggestedTimeline, SuggestedWarranty, SuggestedHomeownerNotes,
    DefaultVisitType, DefaultAssignedTechnician, DefaultVisitNotes, DefaultVisitAt, DefaultVisitTimeLabel,
    DefaultChecklistJson, DefaultMaterialsUsedJson, DefaultLaborWarranty
)
VALUES (
    @ProveedorId, N'901 Elm St, Austin, TX 78705', N'Electrical', N'Standard', N'New',
    N'Tom Hayes', NULL, N'(512) 555-5520', N'L-1047', 1,
    N'Outlet not working in garage.', N'/welcome-house.png', NULL, 3.2, NULL,
    NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
);

INSERT INTO dbo.IndorProveedorLeads (
    ProveedorId, Address, ServiceType, Urgency, Status, CustomerName, CustomerEmail, CustomerPhone,
    LeadCode, IsHomeownerVerified, ProblemDescription, ImageUrl, PhotosJson, DistanceMiles, TimelineNote,
    HomeType, SquareFeet, YearBuilt, Stories, AccessNotes,
    SuggestedScopeItemsJson, SuggestedLaborAmount, SuggestedMaterialsAmount,
    SuggestedTimeline, SuggestedWarranty, SuggestedHomeownerNotes,
    DefaultVisitType, DefaultAssignedTechnician, DefaultVisitNotes, DefaultVisitAt, DefaultVisitTimeLabel,
    DefaultChecklistJson, DefaultMaterialsUsedJson, DefaultLaborWarranty
)
VALUES (
    @ProveedorId, N'44 Willow Rd, Austin, TX 78748', N'Painting', N'Standard', N'New',
    N'Lisa Tran', NULL, N'(512) 555-6630', N'L-1048', 1,
    N'Exterior trim touch-up.', N'/welcome-house.png', NULL, 11.0, NULL,
    NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
);

INSERT INTO dbo.IndorProveedorLeads (
    ProveedorId, Address, ServiceType, Urgency, Status, CustomerName, CustomerEmail, CustomerPhone,
    LeadCode, IsHomeownerVerified, ProblemDescription, ImageUrl, PhotosJson, DistanceMiles, TimelineNote,
    HomeType, SquareFeet, YearBuilt, Stories, AccessNotes,
    SuggestedScopeItemsJson, SuggestedLaborAmount, SuggestedMaterialsAmount,
    SuggestedTimeline, SuggestedWarranty, SuggestedHomeownerNotes,
    DefaultVisitType, DefaultAssignedTechnician, DefaultVisitNotes, DefaultVisitAt, DefaultVisitTimeLabel,
    DefaultChecklistJson, DefaultMaterialsUsedJson, DefaultLaborWarranty
)
VALUES (
    @ProveedorId, N'300 Lakeview Dr, Austin, TX 78732', N'Landscaping', N'Standard', N'New',
    N'Chris Bell', NULL, N'(512) 555-7740', N'L-1049', 0,
    N'Spring cleanup and mulch.', N'/welcome-house.png', NULL, 14.5, NULL,
    NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
);

SET @LeadRoofId = (SELECT Id FROM dbo.IndorProveedorLeads WHERE ProveedorId = @ProveedorId AND LeadCode = N'L-1042');

-- Jobs today (6)
INSERT INTO dbo.IndorProveedorJobs (
    ProveedorId, ClienteId, JobCode, Title, Address, Status, ServiceType, AssignedTechnician, Priority,
    ScopeOfWork, ImageUrl, ScheduledAt, ChecklistJson, MaterialsUsedJson, PhotoUrlsJson, LaborWarranty, IsDraft
)
VALUES
(@ProveedorId, @ClienteJamesId, N'J-TODAY-01', N'HVAC Tune-Up', N'123 Main St, Austin, TX 78701', N'InProgress', N'HVAC', N'Mike Johnson', N'Medium',
 N'Annual HVAC tune-up and filter replacement.', N'/welcome-house.png', DATEADD(hour, 8, CAST(@Today AS datetime2)),
 N'[{"label":"Arrived on site","completed":true},{"label":"Safety check completed","completed":true},{"label":"Inspect unit","completed":false}]',
 N'[{"name":"HVAC Filter","quantity":2}]', N'[{"url":"/welcome-house.png","label":"Before"}]', N'Labor Warranty: 1 Year', 0),

(@ProveedorId, @ClienteJamesId, N'J-TODAY-02', N'Roof Repair', N'456 Oak Dr, Austin, TX 78704', N'Scheduled', N'Roof Repair', N'Mike Johnson', N'High',
 N'Repair chimney flashing and replace damaged shingles.', N'/welcome-house.png', DATEADD(hour, 10, DATEADD(minute, 30, CAST(@Today AS datetime2))),
 N'[{"label":"Arrived on site","completed":false},{"label":"Inspect chimney flashing","completed":false}]',
 N'[{"name":"Architectural Shingles","quantity":12}]', NULL, N'Labor Warranty: 1 Year', 0),

(@ProveedorId, @ClienteJamesId, N'J-TODAY-03', N'Plumbing Repair', N'789 Pine Ave, Austin, TX 78702', N'Confirmed', N'Plumbing', N'Lisa Park', N'Medium',
 N'Fix kitchen sink drain backup.', N'/welcome-house.png', DATEADD(hour, 14, CAST(@Today AS datetime2)),
 NULL, N'[{"name":"P-Trap Assembly","quantity":1}]', NULL, N'Labor Warranty: 90 Days', 0),

(@ProveedorId, @ClienteJamesId, N'J-TODAY-04', N'Gutter Cleaning', N'55 Birch Way, Austin, TX 78746', N'Scheduled', N'Gutter Cleaning', N'Mike Johnson', N'Low',
 N'Clean gutters and downspouts.', N'/welcome-house.png', DATEADD(hour, 9, CAST(@Today AS datetime2)), NULL, NULL, NULL, NULL, 0),

(@ProveedorId, @ClienteJamesId, N'J-TODAY-05', N'Electrical Repair', N'901 Elm St, Austin, TX 78705', N'Scheduled', N'Electrical', N'Lisa Park', N'Medium',
 N'Replace faulty garage outlet.', N'/welcome-house.png', DATEADD(hour, 15, CAST(@Today AS datetime2)), NULL, NULL, NULL, NULL, 0),

(@ProveedorId, @ClienteJamesId, N'J-TODAY-06', N'Exterior Painting', N'44 Willow Rd, Austin, TX 78748', N'Confirmed', N'Painting', N'Mike Johnson', N'Low',
 N'Touch up exterior trim.', N'/welcome-house.png', DATEADD(hour, 16, CAST(@Today AS datetime2)), NULL, NULL, NULL, NULL, 0);

-- Jobs tomorrow + day after (calendar)
INSERT INTO dbo.IndorProveedorJobs (ProveedorId, ClienteId, JobCode, Title, Address, Status, ServiceType, ScheduledAt, IsDraft)
VALUES
(@ProveedorId, @ClienteJamesId, N'J-TMRW-01', N'Roof Inspection', N'123 Main St, Austin, TX 78701', N'Scheduled', N'Roof Repair', DATEADD(hour, 9, DATEADD(day, 1, CAST(@Today AS datetime2))), 0),
(@ProveedorId, @ClienteJamesId, N'J-TMRW-02', N'HVAC Service', N'456 Oak Dr, Austin, TX 78704', N'Scheduled', N'HVAC', DATEADD(hour, 11, DATEADD(day, 1, CAST(@Today AS datetime2))), 0),
(@ProveedorId, @ClienteJamesId, N'J-TMRW-03', N'Plumbing Visit', N'789 Pine Ave, Austin, TX 78702', N'Scheduled', N'Plumbing', DATEADD(hour, 14, DATEADD(day, 1, CAST(@Today AS datetime2))), 0),
(@ProveedorId, @ClienteJamesId, N'J-D2-01', N'Landscaping', N'300 Lakeview Dr, Austin, TX 78732', N'Scheduled', N'Landscaping', DATEADD(hour, 10, DATEADD(day, 2, CAST(@Today AS datetime2))), 0),
(@ProveedorId, @ClienteJamesId, N'J-D2-02', N'Painting', N'44 Willow Rd, Austin, TX 78748', N'Scheduled', N'Painting', DATEADD(hour, 13, DATEADD(day, 2, CAST(@Today AS datetime2))), 0);

DECLARE @JobHvacId INT = (SELECT Id FROM dbo.IndorProveedorJobs WHERE ProveedorId = @ProveedorId AND JobCode = N'J-TODAY-01');
DECLARE @JobRoofId INT = (SELECT Id FROM dbo.IndorProveedorJobs WHERE ProveedorId = @ProveedorId AND JobCode = N'J-TODAY-02');

INSERT INTO dbo.IndorProveedorJobs (
    ProveedorId, ClienteId, JobCode, Title, Address, Status, ServiceType,
    CompletedAt, FechaActualizacion, FechaCreacion, IsDraft
)
VALUES (
    @ProveedorId, @ClienteJamesId, N'JOB-10234', N'Water Heater Install', N'456 Oak Dr, Austin, TX 78704',
    N'Completed', N'Plumbing', DATEADD(day, -8, CAST(@Today AS datetime2)), DATEADD(day, -8, CAST(@Today AS datetime2)),
    DATEADD(day, -10, CAST(@Today AS datetime2)), 0
);

DECLARE @JobWaterHeaterId INT = (SELECT Id FROM dbo.IndorProveedorJobs WHERE ProveedorId = @ProveedorId AND JobCode = N'JOB-10234');

-- Pending estimates (5 sent + 1 draft)
INSERT INTO dbo.IndorProveedorEstimates (
    ProveedorId, LeadId, EstimateCode, Amount, Address, Status, ServiceType, CustomerName,
    LaborAmount, MaterialsAmount, ScopeItemsJson, Timeline, Warranty, HomeownerNotes, SentUtc
)
VALUES
(@ProveedorId, @LeadRoofId, N'10240', 4200.00, N'123 Main St, Austin, TX 78701', N'Sent', N'Roof Repair', N'Maria Lopez', 1800, 2400,
 N'[{"label":"Shingle Replacement","amount":3200},{"label":"Flashing Repair","amount":1000}]', N'2 days', N'1 Year Warranty', NULL, DATEADD(day, -2, SYSUTCDATETIME())),
(@ProveedorId, NULL, N'10315', 3150.00, N'456 Oak Dr, Austin, TX 78704', N'Sent', N'HVAC', N'James Smith', 1200, 1950,
 N'[{"label":"Tune-up","amount":450},{"label":"Parts","amount":2700}]', N'1 day', N'1 Year Warranty', NULL, DATEADD(day, -1, SYSUTCDATETIME())),
(@ProveedorId, NULL, N'10322', 1850.00, N'789 Pine Ave, Austin, TX 78702', N'Sent', N'Plumbing', N'Robert Kim', 750, 1100,
 N'[{"label":"Drain repair","amount":1850}]', N'Half day', N'90 Day Warranty', NULL, DATEADD(day, -3, SYSUTCDATETIME())),
(@ProveedorId, NULL, N'10330', 980.00, N'55 Birch Way, Austin, TX 78746', N'Sent', N'Gutter Cleaning', N'Anna Reed', 400, 580,
 NULL, N'2 hours', N'30 Day Warranty', NULL, DATEADD(day, -4, SYSUTCDATETIME())),
(@ProveedorId, NULL, N'10341', 650.00, N'901 Elm St, Austin, TX 78705', N'Sent', N'Electrical', N'Tom Hayes', 350, 300,
 NULL, N'Same day', N'90 Day Warranty', NULL, DATEADD(day, -5, SYSUTCDATETIME()));

INSERT INTO dbo.IndorProveedorEstimates (
    ProveedorId, LeadId, EstimateCode, Amount, Address, Status, ServiceType, CustomerName,
    LaborAmount, MaterialsAmount, ScopeItemsJson, Timeline, Warranty, LaborWarranty, PartsWarranty,
    SubtotalAmount, TaxRate, TaxAmount, EstimatedDuration, FechaActualizacion
)
VALUES
(@ProveedorId, @LeadRoofId, N'10247', 2165.00, N'456 Oak Dr, Austin, TX 78704', N'Draft', N'Water Heater Install', N'James Smith',
 650, 1350,
 N'[{"label":"Remove & dispose old water heater","amount":150},{"label":"Install new 50-gallon water heater","amount":1350},{"label":"Expansion tank install","amount":350},{"label":"Permit & inspection","amount":150}]',
 N'1 Day', N'1 Year Labor', N'1 Year', N'6 Years',
 2000, 0.0825, 165, N'1 Day', SYSUTCDATETIME()),
(@ProveedorId, NULL, N'10388', 4200.00, N'678 Sunset Blvd, Austin, TX 78704', N'Ready', N'Roof Repair', N'Maria Lopez',
 1800, 2400,
 N'[{"label":"Shingle replacement","amount":3200},{"label":"Flashing repair","amount":1000}]',
 N'2 days', N'2 Years Workmanship', N'2 Years', N'1 Year',
 4200, 0.0825, 0, N'2 days', SYSUTCDATETIME());

UPDATE dbo.IndorProveedorEstimates
SET ViewedUtc = DATEADD(minute, 34, SentUtc)
WHERE ProveedorId = @ProveedorId AND EstimateCode = N'10315' AND ViewedUtc IS NULL;

-- Invoices (payments flow: Paid $18,920 / Pending $7,340 / Overdue $2,450)
INSERT INTO dbo.IndorProveedorInvoices (
    ProveedorId, JobId, InvoiceCode, Address, ServiceType, CustomerName, CustomerEmail, CustomerPhone,
    Amount, Status, DueDate, PaidDate, InvoiceDate, PaymentMethod, NotesToCustomer, LineItemsJson
)
VALUES
(@ProveedorId, @JobWaterHeaterId, N'2047', N'456 Oak Dr, Austin, TX 78704', N'Water Heater Install', N'Emily Johnson',
 N'emily.johnson@email.com', N'(512) 555-0198', 2450.00, N'Overdue', DATEADD(day, -5, @Today), NULL, DATEADD(day, -14, @Today), N'Unpaid',
 N'Thank you for choosing Rivera Services. Payment is due by the date shown above. Please contact us with any questions.',
 N'[{"description":"Labor - Water Heater Installation","qty":1,"rate":1250,"amount":1250},{"description":"Water Heater Unit (50 Gallon)","qty":1,"rate":850,"amount":850},{"description":"Plumbing Materials & Fittings","qty":1,"rate":200,"amount":200},{"description":"Permit & Disposal Fee","qty":1,"rate":150,"amount":150}]'),

(@ProveedorId, @JobRoofId, N'2048', N'789 Pine Ave, Austin, TX 78702', N'Roof Replacement', N'Robert Kim',
 N'robert@email.com', N'(512) 555-3302', 1650.00, N'Pending', DATEADD(day, 10, @Today), NULL, DATEADD(day, -3, @Today), N'Unpaid', NULL, NULL),

(@ProveedorId, @JobHvacId, N'2049', N'910 Cedar St, Austin, TX 78745', N'HVAC Tune-Up', N'Anna Reed',
 N'anna@email.com', N'(512) 555-4401', 580.00, N'Paid', DATEADD(day, -5, @Today), DATEADD(day, -2, @Today), DATEADD(day, -12, @Today), N'Paid', NULL, NULL),

(@ProveedorId, NULL, N'2050', N'1206 Briarwood Dr, Austin, TX 78746', N'Roof Repair', N'Sarah Chen',
 N'sarah@email.com', N'(512) 555-8821', 3200.00, N'Pending', DATEADD(day, 12, @Today), NULL, DATEADD(day, -2, @Today), N'Unpaid', NULL, NULL),

(@ProveedorId, NULL, N'2052', N'300 Lakeview Dr, Austin, TX 78732', N'Landscaping', N'Customer 12',
 NULL, NULL, 2490.00, N'Pending', DATEADD(day, 18, @Today), NULL, DATEADD(day, -1, @Today), N'Unpaid', NULL, NULL),

(@ProveedorId, @JobHvacId, N'2053', N'123 Main St, Austin, TX 78701', N'HVAC System Replacement', N'Maria Lopez',
 N'maria@email.com', N'(512) 555-2201', 4200.00, N'Paid', DATEADD(day, -10, @Today), DATEADD(day, -5, @Today), DATEADD(day, -14, @Today), N'Paid', NULL, NULL),

(@ProveedorId, @JobRoofId, N'2054', N'456 Oak Dr, Austin, TX 78704', N'Roof Repair', N'James Smith',
 N'james.smith@email.com', N'(512) 555-0142', 3150.00, N'Paid', DATEADD(day, -8, @Today), DATEADD(day, -3, @Today), DATEADD(day, -11, @Today), N'Paid', NULL, NULL),

(@ProveedorId, NULL, N'2055', N'55 Birch Way, Austin, TX 78746', N'Gutter Cleaning', N'Anna Reed',
 NULL, NULL, 4800.00, N'Paid', DATEADD(day, -15, @Today), DATEADD(day, -2, @Today), DATEADD(day, -16, @Today), N'Paid', NULL, NULL),

(@ProveedorId, NULL, N'2056', N'901 Elm St, Austin, TX 78705', N'Electrical Repair', N'Tom Hayes',
 NULL, NULL, 3720.00, N'Paid', DATEADD(day, -12, @Today), DATEADD(day, -1, @Today), DATEADD(day, -13, @Today), N'Paid', NULL, NULL),

(@ProveedorId, NULL, N'2057', N'44 Willow Rd, Austin, TX 78748', N'Exterior Painting', N'Mike Torres',
 NULL, NULL, 2470.00, N'Paid', DATEADD(day, -20, @Today), @Today, DATEADD(day, -21, @Today), N'Paid', NULL, NULL);

UPDATE dbo.IndorProveedorInvoices
SET CustomerNotes = N'Customer asked for invoice copy by email.',
    PropertyType = N'Single Family Home',
    LineItemsJson = N'[{"category":"labor","description":"Roof Replacement Labor (12 hrs)","qty":12,"rate":75,"amount":900},{"category":"materials","description":"Shingles & Underlayment","qty":1,"rate":550,"amount":550},{"category":"permit","description":"Permit & Disposal Fees","qty":1,"rate":200,"amount":200}]'
WHERE ProveedorId = @ProveedorId AND InvoiceCode = N'2048';

UPDATE dbo.IndorProveedorInvoices
SET PropertyType = N'Single Family Home',
    LineItemsJson = N'[{"category":"labor","description":"Roof Repair Labor","qty":1,"rate":1800,"amount":1800},{"category":"materials","description":"Shingles & Flashing","qty":1,"rate":1400,"amount":1400}]'
WHERE ProveedorId = @ProveedorId AND InvoiceCode = N'2050';

-- Paid invoice demo (#2049 — VIEW PAID INVOICE flow)
UPDATE dbo.IndorProveedorInvoices
SET PaidAmount = 580.00,
    PaymentMethod = N'Card',
    PaymentReference = N'4242',
    CustomerNotes = N'Thank you! Great service.',
    PropertyType = N'Single Family Home',
    PaidDate = DATEADD(hour, 9, DATEADD(minute, 14, CAST(DATEADD(day, -2, @Today) AS DATE))),
    LineItemsJson = N'[{"category":"labor","description":"HVAC Tune-Up Labor","qty":1,"rate":380,"amount":380},{"category":"materials","description":"Filter & Supplies","qty":1,"rate":200,"amount":200}]'
WHERE ProveedorId = @ProveedorId AND InvoiceCode = N'2049';

-- Homeowner approvals
INSERT INTO dbo.IndorProveedorApprovals (ProveedorId, JobId, Address, ImageUrl, Status)
VALUES
(@ProveedorId, @JobRoofId, N'456 Oak Dr, Austin, TX 78704', N'/welcome-house.png', N'Pending'),
(@ProveedorId, @JobHvacId, N'123 Main St, Austin, TX 78701', N'/welcome-house.png', N'Pending');

-- Completed jobs last month (16) for +12 delta vs this month
SET @i = 1;
WHILE @i <= 16
BEGIN
    INSERT INTO dbo.IndorProveedorJobs (
        ProveedorId, ClienteId, JobCode, Title, Address, Status, ServiceType,
        CompletedAt, FechaActualizacion, FechaCreacion, IsDraft
    )
    VALUES (
        @ProveedorId, @ClienteJamesId, N'J-PREV-' + RIGHT(N'00' + CAST(@i AS nvarchar(10)), 3),
        N'Prior Service ' + CAST(@i AS nvarchar(10)), N'Austin, TX', N'Completed', N'General',
        DATEADD(day, -(@i + 5), CAST(@MonthStart AS datetime2)),
        DATEADD(day, -(@i + 5), CAST(@MonthStart AS datetime2)),
        DATEADD(day, -(@i + 10), CAST(@MonthStart AS datetime2)),
        0
    );
    SET @i += 1;
END

-- Completed jobs this month (28 home records)
SET @i = 1;
WHILE @i <= 28
BEGIN
    INSERT INTO dbo.IndorProveedorJobs (
        ProveedorId, ClienteId, JobCode, Title, Address, Status, ServiceType,
        CompletedAt, FechaActualizacion, FechaCreacion, IsDraft
    )
    VALUES (
        @ProveedorId, @ClienteJamesId, N'J-DONE-' + RIGHT(N'00' + CAST(@i AS nvarchar(10)), 3),
        N'Service Record ' + CAST(@i AS nvarchar(10)), N'Austin, TX', N'Completed', N'General',
        DATEADD(day, -(@i % 20), CAST(@Today AS datetime2)),
        DATEADD(day, -(@i % 20), CAST(@Today AS datetime2)),
        DATEADD(day, -(@i % 25), CAST(@Today AS datetime2)),
        0
    );
    SET @i += 1;
END

PRINT 'Dashboard seed complete for ProveedorId = ' + CAST(@ProveedorId AS nvarchar(20));
GO
