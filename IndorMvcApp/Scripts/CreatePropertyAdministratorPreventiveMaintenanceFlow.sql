/*
  INDOR — Preventive Maintenance flow tables + catalog seed.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorPropertyAdminPreventiveServiceCatalog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdminPreventiveServiceCatalog (
        Id               INT IDENTITY(1,1) NOT NULL,
        ServiceKey       NVARCHAR(60)  NOT NULL,
        ServiceName      NVARCHAR(120) NOT NULL,
        DefaultFrequency NVARCHAR(40)  NOT NULL,
        IconClass        NVARCHAR(50)  NOT NULL CONSTRAINT DF_PropAdminPrevSvc_Icon DEFAULT (N'fa-wrench'),
        ToneClass        NVARCHAR(30)  NOT NULL CONSTRAINT DF_PropAdminPrevSvc_Tone DEFAULT (N'tone-blue'),
        Activo           BIT NOT NULL CONSTRAINT DF_PropAdminPrevSvc_Activo DEFAULT (1),
        Orden            INT NOT NULL,
        CONSTRAINT PK_IndorPropertyAdminPreventiveServiceCatalog PRIMARY KEY (Id)
    );
    CREATE UNIQUE INDEX UX_PropAdminPrevSvc_Key ON dbo.IndorPropertyAdminPreventiveServiceCatalog(ServiceKey);
    PRINT 'Table IndorPropertyAdminPreventiveServiceCatalog created.';
END
GO

IF OBJECT_ID(N'dbo.IndorPropertyAdminPreventivePlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdminPreventivePlans (
        Id                   INT IDENTITY(1,1) NOT NULL,
        AdministratorId      INT NOT NULL,
        PortfolioPropertyId  INT NOT NULL,
        Status               NVARCHAR(20)  NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Status DEFAULT (N'Draft'),
        PlanTier             NVARCHAR(30)  NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Tier DEFAULT (N'Basic'),
        MonthlyPrice         DECIMAL(8,2)  NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Monthly DEFAULT (29),
        BundlePrice          DECIMAL(8,2)  NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Bundle DEFAULT (149),
        SelectedServicesJson NVARCHAR(2000) NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Services DEFAULT (N'[]'),
        Frequency            NVARCHAR(30)  NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Freq DEFAULT (N'Every3Months'),
        PreferredTiming      NVARCHAR(30)  NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Time DEFAULT (N'Flexible'),
        PreferredDay         NVARCHAR(20)  NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Day DEFAULT (N'Tue'),
        EntryAccess          NVARCHAR(30)  NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Access DEFAULT (N'HostPresent'),
        UpdateRecipients     NVARCHAR(80)  NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Updates DEFAULT (N'Me'),
        Notes                NVARCHAR(500) NULL,
        AutoReminders        BIT NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Reminders DEFAULT (1),
        NextVisitDate        DATE NULL,
        FechaCreacion        DATETIME2(7) NOT NULL CONSTRAINT DF_PropAdminPrevPlan_Created DEFAULT (SYSUTCDATETIME()),
        ActivatedUtc         DATETIME2(7) NULL,
        CONSTRAINT PK_IndorPropertyAdminPreventivePlans PRIMARY KEY (Id),
        CONSTRAINT FK_PropAdminPrevPlan_Admin FOREIGN KEY (AdministratorId)
            REFERENCES dbo.IndorPropertyAdministrators(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_PropAdminPrevPlan_Admin ON dbo.IndorPropertyAdminPreventivePlans(AdministratorId);
    PRINT 'Table IndorPropertyAdminPreventivePlans created.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.IndorPropertyAdminPreventiveServiceCatalog)
BEGIN
    INSERT INTO dbo.IndorPropertyAdminPreventiveServiceCatalog
        (ServiceKey, ServiceName, DefaultFrequency, IconClass, ToneClass, Orden)
    VALUES
    (N'HvacTuneUp',           N'HVAC Tune-Up',           N'Semi-annual', N'fa-snowflake', N'tone-blue',  1),
    (N'HvacFilterChange',    N'HVAC Filter Change',     N'Quarterly',   N'fa-fan',       N'tone-blue',  2),
    (N'WaterHeaterFlush',    N'Water Heater Flush',     N'Annual',      N'fa-droplet',   N'tone-red',   3),
    (N'SmokeDetectorCheck',  N'Smoke Detector Check',   N'Annual',      N'fa-bell',      N'tone-green', 4),
    (N'DryerVentCleaning',   N'Dryer Vent Cleaning',    N'Annual',      N'fa-wind',      N'tone-blue',  5),
    (N'GutterCleaning',      N'Gutter Cleaning',        N'Semi-annual', N'fa-house',     N'tone-blue',  6),
    (N'PlumbingCheck',       N'Plumbing Check',         N'Annual',      N'fa-faucet',    N'tone-blue',  7),
    (N'ElectricalSafety',    N'Electrical Safety Check',N'Annual',      N'fa-bolt',      N'tone-red',   8),
    (N'ApplianceCheck',      N'Appliance Check',        N'Annual',      N'fa-blender',   N'tone-blue',  9),
    (N'PestPrevention',      N'Pest Prevention',        N'Quarterly',   N'fa-bug',       N'tone-green', 10);
    PRINT 'Preventive service catalog seeded.';
END
GO

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'PreventiveMaintenanceServices', LinkRouteId = NULL
WHERE ServiceSlug = N'preventive-maintenance';

PRINT 'Preventive Maintenance catalog link updated.';
GO
