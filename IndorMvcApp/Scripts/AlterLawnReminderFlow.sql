/*
  Always Perfect Lawn — reminder flow + DB-driven catalog options.
  Run after CreateLawnFlowTables.sql. Safe to run multiple times.
*/

DECLARE @LawnMicroId INT = (
    SELECT TOP 1 Id FROM dbo.Microservicios WHERE Nombre = N'Always Perfect Lawn' ORDER BY Id
);
IF @LawnMicroId IS NULL SET @LawnMicroId = 2;

/* --- LawnServicioLanding reminder copy --- */
IF COL_LENGTH(N'dbo.LawnServicioLanding', N'ReminderBannerTitulo') IS NULL
    ALTER TABLE dbo.LawnServicioLanding ADD ReminderBannerTitulo NVARCHAR(80) NULL;
IF COL_LENGTH(N'dbo.LawnServicioLanding', N'ReminderBannerTexto') IS NULL
    ALTER TABLE dbo.LawnServicioLanding ADD ReminderBannerTexto NVARCHAR(300) NULL;
IF COL_LENGTH(N'dbo.LawnServicioLanding', N'ReminderDefaultOn') IS NULL
    ALTER TABLE dbo.LawnServicioLanding ADD ReminderDefaultOn BIT NOT NULL
        CONSTRAINT DF_LawnServicioLanding_ReminderDefault DEFAULT (1);
IF COL_LENGTH(N'dbo.LawnServicioLanding', N'RemindOnlyCtaTexto') IS NULL
    ALTER TABLE dbo.LawnServicioLanding ADD RemindOnlyCtaTexto NVARCHAR(60) NULL;
GO

UPDATE dbo.LawnServicioLanding
SET ReminderBannerTitulo = ISNULL(ReminderBannerTitulo, N'Automatic reminder'),
    ReminderBannerTexto = ISNULL(ReminderBannerTexto, N'Remind me every 15 days to mow the lawn. We will send you a notification to schedule or repeat the service.'),
    RemindOnlyCtaTexto = ISNULL(RemindOnlyCtaTexto, N'Only remind me')
WHERE MicroservicioId = (SELECT TOP 1 Id FROM dbo.Microservicios WHERE Nombre = N'Always Perfect Lawn' ORDER BY Id);
GO

/* --- SolicitudesLawn reminder / mode --- */
IF COL_LENGTH(N'dbo.SolicitudesLawn', N'ModoServicio') IS NULL
    ALTER TABLE dbo.SolicitudesLawn ADD ModoServicio NVARCHAR(20) NOT NULL
        CONSTRAINT DF_SolicitudesLawn_ModoServicio DEFAULT (N'FullService');
IF COL_LENGTH(N'dbo.SolicitudesLawn', N'RecordatorioActivo') IS NULL
    ALTER TABLE dbo.SolicitudesLawn ADD RecordatorioActivo BIT NOT NULL
        CONSTRAINT DF_SolicitudesLawn_RecordatorioActivo DEFAULT (0);
IF COL_LENGTH(N'dbo.SolicitudesLawn', N'RecordatorioAvisoDias') IS NULL
    ALTER TABLE dbo.SolicitudesLawn ADD RecordatorioAvisoDias INT NOT NULL
        CONSTRAINT DF_SolicitudesLawn_RecordatorioAvisoDias DEFAULT (1);
IF COL_LENGTH(N'dbo.SolicitudesLawn', N'RecordatorioCanales') IS NULL
    ALTER TABLE dbo.SolicitudesLawn ADD RecordatorioCanales NVARCHAR(100) NULL;
IF COL_LENGTH(N'dbo.SolicitudesLawn', N'ProximoRecordatorioUtc') IS NULL
    ALTER TABLE dbo.SolicitudesLawn ADD ProximoRecordatorioUtc DATETIME2(7) NULL;
GO

/* --- Catalog options --- */
IF OBJECT_ID(N'dbo.LawnCatalogOptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LawnCatalogOptions (
        Id              INT            IDENTITY(1,1) NOT NULL,
        MicroservicioId INT            NOT NULL,
        OptionGroup     NVARCHAR(40)   NOT NULL,
        Code            NVARCHAR(40)   NOT NULL,
        LabelEn         NVARCHAR(80)   NOT NULL,
        DescriptionEn   NVARCHAR(200)  NULL,
        Price           DECIMAL(10,2)  NOT NULL CONSTRAINT DF_LawnCatalogOptions_Price DEFAULT (0),
        IconClass       NVARCHAR(60)   NOT NULL CONSTRAINT DF_LawnCatalogOptions_Icon DEFAULT (N'fa-circle'),
        SortOrder       INT            NOT NULL CONSTRAINT DF_LawnCatalogOptions_Sort DEFAULT (0),
        RequiresQuote   BIT            NOT NULL CONSTRAINT DF_LawnCatalogOptions_Quote DEFAULT (0),
        IsActive        BIT            NOT NULL CONSTRAINT DF_LawnCatalogOptions_Active DEFAULT (1),
        CONSTRAINT PK_LawnCatalogOptions PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_LawnCatalogOptions_Microservicios FOREIGN KEY (MicroservicioId)
            REFERENCES dbo.Microservicios(Id)
    );
    CREATE UNIQUE INDEX UX_LawnCatalogOptions_GroupCode
        ON dbo.LawnCatalogOptions(MicroservicioId, OptionGroup, Code);
    PRINT 'Table LawnCatalogOptions created.';
END
GO

DECLARE @MicroId INT = (SELECT TOP 1 Id FROM dbo.Microservicios WHERE Nombre = N'Always Perfect Lawn' ORDER BY Id);
IF @MicroId IS NULL SET @MicroId = 2;

IF @MicroId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.LawnCatalogOptions WHERE MicroservicioId = @MicroId)
BEGIN
    INSERT INTO dbo.LawnCatalogOptions (MicroservicioId, OptionGroup, Code, LabelEn, DescriptionEn, Price, IconClass, SortOrder, RequiresQuote) VALUES
    (@MicroId, N'Frequency', N'Once',           N'Once',            N'One-time service',           0,  N'fa-calendar-day',     1, 0),
    (@MicroId, N'Frequency', N'Every15Days',   N'Every 15 days',   N'Recurring every 15 days',    0,  N'fa-rotate',           2, 0),
    (@MicroId, N'Frequency', N'Monthly',       N'Monthly',         N'Recurring monthly',          0,  N'fa-calendar',         3, 0),
    (@MicroId, N'Frequency', N'Flexible',      N'Flexible',        N'Flexible schedule',          0,  N'fa-sliders',          4, 0),

    (@MicroId, N'Area', N'FrontYard',    N'Front',           NULL, 45,  N'fa-house',           1, 0),
    (@MicroId, N'Area', N'BackYard',     N'Backyard',        NULL, 45,  N'fa-fence',           2, 0),
    (@MicroId, N'Area', N'FrontBack',    N'Front + Backyard', NULL, 75,  N'fa-house-chimney',   3, 0),

    (@MicroId, N'Addon', N'EdgeBorders',   N'Edging / borders', NULL, 20, N'fa-border-all',    1, 0),
    (@MicroId, N'Addon', N'BushTrimming',  N'Bush trimming',    NULL, 30, N'fa-leaf',          2, 0),
    (@MicroId, N'Addon', N'NoThanks',      N'No thanks',        NULL,  0, N'fa-ban',           3, 0),

    (@MicroId, N'TimeWindow', N'Morning8_11',   N'8–11 AM',     NULL, 0, N'fa-sun',           1, 0),
    (@MicroId, N'TimeWindow', N'Midday11_2',    N'11 AM–2 PM',  NULL, 0, N'fa-cloud-sun',     2, 0),
    (@MicroId, N'TimeWindow', N'Afternoon2_5',  N'2–5 PM',      NULL, 0, N'fa-cloud',         3, 0),
    (@MicroId, N'TimeWindow', N'Evening5_8',    N'5–8 PM',      NULL, 0, N'fa-moon',          4, 0),

    (@MicroId, N'ReminderLead', N'1',  N'1 day before',  NULL, 0, N'fa-bell', 1, 0),
    (@MicroId, N'ReminderLead', N'2',  N'2 days before', NULL, 0, N'fa-bell', 2, 0),
    (@MicroId, N'ReminderLead', N'3',  N'3 days before', NULL, 0, N'fa-bell', 3, 0),

    (@MicroId, N'ReminderChannel', N'Push',  N'Push',  NULL, 0, N'fa-mobile-screen', 1, 0),
    (@MicroId, N'ReminderChannel', N'SMS',   N'SMS',   NULL, 0, N'fa-comment-sms',   2, 0),
    (@MicroId, N'ReminderChannel', N'Email', N'Email', NULL, 0, N'fa-envelope',     3, 0);

    PRINT 'LawnCatalogOptions seeded.';
END
GO

PRINT 'Lawn reminder flow schema ready.';
GO
