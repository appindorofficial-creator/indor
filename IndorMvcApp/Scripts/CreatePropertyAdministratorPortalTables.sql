/*
  INDOR — Multi-Property Owner portal (Home, Calendar, Properties, Services, Tasks).
  Run after CreatePropertyAdministratorRegistrationTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorPropertyAdminServiceRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdminServiceRequests (
        Id                  INT IDENTITY(1,1) NOT NULL,
        AdministratorId     INT NOT NULL,
        PortfolioPropertyId INT NULL,
        Title               NVARCHAR(150) NOT NULL,
        PropertyName        NVARCHAR(200) NOT NULL,
        Location            NVARCHAR(200) NOT NULL,
        Status              NVARCHAR(30)  NOT NULL CONSTRAINT DF_PropAdminReq_Status DEFAULT (N'Open'),
        Category            NVARCHAR(30)  NOT NULL CONSTRAINT DF_PropAdminReq_Cat DEFAULT (N'General'),
        ScheduledUtc        DATETIME2(7)  NULL,
        EtaLabel            NVARCHAR(80)  NULL,
        TeamLabel           NVARCHAR(80)  NULL,
        ImageUrl            NVARCHAR(300) NULL,
        IsEmergency         BIT NOT NULL CONSTRAINT DF_PropAdminReq_Emergency DEFAULT (0),
        FechaCreacion       DATETIME2(7)  NOT NULL CONSTRAINT DF_PropAdminReq_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorPropertyAdminServiceRequests PRIMARY KEY (Id),
        CONSTRAINT FK_PropAdminReq_Admin FOREIGN KEY (AdministratorId)
            REFERENCES dbo.IndorPropertyAdministrators(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_PropAdminReq_Admin ON dbo.IndorPropertyAdminServiceRequests(AdministratorId);
    PRINT 'Table IndorPropertyAdminServiceRequests created.';
END
GO

IF OBJECT_ID(N'dbo.IndorPropertyAdminHomecarePlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdminHomecarePlans (
        Id              INT IDENTITY(1,1) NOT NULL,
        AdministratorId INT NOT NULL,
        PlanName        NVARCHAR(120) NOT NULL,
        Frequency       NVARCHAR(60)  NOT NULL,
        HomesCovered    INT NOT NULL,
        NextDueDate     DATE NULL,
        IconClass       NVARCHAR(50)  NOT NULL CONSTRAINT DF_PropAdminPlan_Icon DEFAULT (N'fa-wrench'),
        ToneClass       NVARCHAR(30)  NOT NULL CONSTRAINT DF_PropAdminPlan_Tone DEFAULT (N'tone-blue'),
        Activo          BIT NOT NULL CONSTRAINT DF_PropAdminPlan_Activo DEFAULT (1),
        Orden           INT NOT NULL CONSTRAINT DF_PropAdminPlan_Orden DEFAULT (0),
        CONSTRAINT PK_IndorPropertyAdminHomecarePlans PRIMARY KEY (Id),
        CONSTRAINT FK_PropAdminPlan_Admin FOREIGN KEY (AdministratorId)
            REFERENCES dbo.IndorPropertyAdministrators(Id) ON DELETE CASCADE
    );
    PRINT 'Table IndorPropertyAdminHomecarePlans created.';
END
GO

IF OBJECT_ID(N'dbo.IndorPropertyAdminScheduledVisits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdminScheduledVisits (
        Id              INT IDENTITY(1,1) NOT NULL,
        AdministratorId INT NOT NULL,
        Title           NVARCHAR(150) NOT NULL,
        PropertyName    NVARCHAR(150) NOT NULL,
        VisitDate       DATE NOT NULL,
        TimeWindow      NVARCHAR(60) NOT NULL,
        ImageUrl        NVARCHAR(300) NULL,
        CONSTRAINT PK_IndorPropertyAdminScheduledVisits PRIMARY KEY (Id),
        CONSTRAINT FK_PropAdminVisit_Admin FOREIGN KEY (AdministratorId)
            REFERENCES dbo.IndorPropertyAdministrators(Id) ON DELETE CASCADE
    );
    PRINT 'Table IndorPropertyAdminScheduledVisits created.';
END
GO

IF OBJECT_ID(N'dbo.IndorPropertyAdminServiceCatalog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdminServiceCatalog (
        Id              INT IDENTITY(1,1) NOT NULL,
        CategoryKey     NVARCHAR(40)  NOT NULL,
        CategoryTitle   NVARCHAR(80)  NOT NULL,
        CategoryOrder   INT NOT NULL,
        ServiceName     NVARCHAR(100) NOT NULL,
        ServiceSlug     NVARCHAR(80)  NOT NULL,
        IconClass       NVARCHAR(50)  NOT NULL,
        ToneClass       NVARCHAR(30)  NOT NULL,
        LinkController  NVARCHAR(80)  NULL,
        LinkAction      NVARCHAR(80)  NULL,
        LinkRouteId     INT NULL,
        Activo          BIT NOT NULL CONSTRAINT DF_PropAdminCatalog_Activo DEFAULT (1),
        Orden           INT NOT NULL,
        CONSTRAINT PK_IndorPropertyAdminServiceCatalog PRIMARY KEY (Id)
    );
    CREATE INDEX IX_PropAdminCatalog_Category ON dbo.IndorPropertyAdminServiceCatalog(CategoryKey, Orden);
    PRINT 'Table IndorPropertyAdminServiceCatalog created.';
END
GO

-- Global service catalog seed (idempotent by ServiceSlug)
IF NOT EXISTS (SELECT 1 FROM dbo.IndorPropertyAdminServiceCatalog)
BEGIN
    INSERT INTO dbo.IndorPropertyAdminServiceCatalog
        (CategoryKey, CategoryTitle, CategoryOrder, ServiceName, ServiceSlug, IconClass, ToneClass, LinkController, LinkAction, Orden)
    VALUES
    (N'emergency', N'Emergency Services', 1, N'Emergency AC', N'emergency-ac', N'fa-snowflake', N'tone-red', N'Administrador', N'EmergencyAcDetails', 1),
    (N'emergency', N'Emergency Services', 1, N'Emergency Plumbing', N'emergency-plumbing', N'fa-droplet', N'tone-red', N'Administrador', N'EmergencyPlumbingDetails', 2),
    (N'emergency', N'Emergency Services', 1, N'Emergency Electrical', N'emergency-electrical', N'fa-bolt', N'tone-red', N'Administrador', N'EmergencyElectricalDetails', 3),
    (N'emergency', N'Emergency Services', 1, N'Emergency Flood', N'emergency-flood', N'fa-water', N'tone-red', N'Administrador', N'EmergencyFloodDetails', 4),
    (N'homecare', N'Homecare & Maintenance', 2, N'Preventive Maintenance', N'preventive-maintenance', N'fa-screwdriver-wrench', N'tone-blue', N'Administrador', N'PreventiveMaintenanceServices', 1),
    (N'homecare', N'Homecare & Maintenance', 2, N'HVAC Filter Change', N'hvac-filter', N'fa-fan', N'tone-blue', N'Administrador', N'AirFilterDetails', 2),
    (N'homecare', N'Homecare & Maintenance', 2, N'Smoke Detector Check', N'smoke-detector', N'fa-bell', N'tone-green', N'Administrador', N'SmokeDetectorDetails', 3),
    (N'cleaning', N'Cleaning & Turnover', 3, N'Turnover Cleaning', N'turnover-cleaning', N'fa-broom', N'tone-purple', N'Administrador', N'TurnoverCleaningDetails', 1),
    (N'cleaning', N'Cleaning & Turnover', 3, N'Standard Cleaning', N'standard-cleaning', N'fa-spray-can-sparkles', N'tone-purple', N'Administrador', N'StandardCleaningDetails', 2),
    (N'cleaning', N'Cleaning & Turnover', 3, N'Pet Deep Clean', N'pet-deep-clean', N'fa-paw', N'tone-purple', N'Administrador', N'PetDeepCleanDetails', 3),
    (N'cleaning', N'Cleaning & Turnover', 3, N'Linen / Supply Restock', N'linen-restock', N'fa-box', N'tone-purple', NULL, NULL, 4),
    (N'cleaning', N'Cleaning & Turnover', 3, N'Trashout', N'trashout', N'fa-trash', N'tone-purple', N'Trash', N'TrashService', 5),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Lawn Care / Grass Cutting', N'lawn-care', N'fa-seedling', N'tone-green', N'Lawn', N'LawnService', 1),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Landscaping', N'landscaping', N'fa-leaf', N'tone-green', NULL, NULL, 2),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Pressure Washing', N'pressure-washing', N'fa-spray-can', N'tone-green', N'PowerWash', N'PowerWashService', 3),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Pest Control', N'pest-control', N'fa-bug', N'tone-green', N'PestControl', N'PestControlService', 4),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Pool / Hot Tub Service', N'pool-hot-tub', N'fa-water-ladder', N'tone-green', NULL, NULL, 5),
    (N'moving', N'Moving & Logistics', 5, N'Moving Help', N'moving-help', N'fa-truck', N'tone-blue', N'Moving', N'MovingService', 1);
    PRINT 'Service catalog seeded.';
END
GO

-- Outdoor & Exterior + catalog updates (idempotent — safe if seed already ran)
DECLARE @LawnMicroId INT = (SELECT TOP 1 Id FROM dbo.Microservicios WHERE Nombre = N'Always Perfect Lawn' ORDER BY Id);
DECLARE @PowerWashPriorityId INT = (SELECT TOP 1 Id FROM dbo.HomeCarePriorities WHERE Nombre = N'Power wash exterior' ORDER BY Id);
DECLARE @PestPriorityId INT = (SELECT TOP 1 Id FROM dbo.HomeCarePriorities WHERE Nombre = N'Pest control' ORDER BY Id);

MERGE dbo.IndorPropertyAdminServiceCatalog AS t
USING (VALUES
    (N'outdoor', N'Outdoor & Exterior', 4, N'Lawn Care / Grass Cutting', N'lawn-care', N'fa-seedling', N'tone-green', N'Lawn', N'LawnService', @LawnMicroId, 1),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Landscaping', N'landscaping', N'fa-leaf', N'tone-green', NULL, NULL, NULL, 2),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Pressure Washing', N'pressure-washing', N'fa-spray-can', N'tone-green', N'PowerWash', N'PowerWashService', @PowerWashPriorityId, 3),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Pest Control', N'pest-control', N'fa-bug', N'tone-green', N'PestControl', N'PestControlService', @PestPriorityId, 4),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Pool / Hot Tub Service', N'pool-hot-tub', N'fa-water-ladder', N'tone-green', NULL, NULL, NULL, 5),
    (N'cleaning', N'Cleaning & Turnover', 3, N'Linen / Supply Restock', N'linen-restock', N'fa-box', N'tone-purple', NULL, NULL, NULL, 4)
) AS s (CategoryKey, CategoryTitle, CategoryOrder, ServiceName, ServiceSlug, IconClass, ToneClass, LinkController, LinkAction, LinkRouteId, Orden)
ON t.ServiceSlug = s.ServiceSlug
WHEN NOT MATCHED THEN
    INSERT (CategoryKey, CategoryTitle, CategoryOrder, ServiceName, ServiceSlug, IconClass, ToneClass, LinkController, LinkAction, LinkRouteId, Orden)
    VALUES (s.CategoryKey, s.CategoryTitle, s.CategoryOrder, s.ServiceName, s.ServiceSlug, s.IconClass, s.ToneClass, s.LinkController, s.LinkAction, s.LinkRouteId, s.Orden);

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET CategoryKey = N'cleaning', CategoryTitle = N'Cleaning & Turnover', CategoryOrder = 3, Orden = 5
WHERE ServiceSlug = N'trashout' AND CategoryKey = N'moving';

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET CategoryOrder = 5
WHERE CategoryKey = N'moving';

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Lawn', LinkAction = N'LawnService', LinkRouteId = @LawnMicroId
WHERE ServiceSlug = N'lawn-care' AND @LawnMicroId IS NOT NULL;

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'PowerWash', LinkAction = N'PowerWashService', LinkRouteId = @PowerWashPriorityId
WHERE ServiceSlug = N'pressure-washing' AND @PowerWashPriorityId IS NOT NULL;

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'PestControl', LinkAction = N'PestControlService', LinkRouteId = @PestPriorityId
WHERE ServiceSlug = N'pest-control' AND @PestPriorityId IS NOT NULL;

PRINT 'Outdoor & Exterior catalog rows ensured.';
GO
