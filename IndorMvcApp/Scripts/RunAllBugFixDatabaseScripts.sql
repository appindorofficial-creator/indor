/*
  =============================================================================
  INDOR — Unified database script for recent bug-fix deployments
  =============================================================================
  Run manually in SSMS or Azure Data Studio against IndorDB (Azure or local).

  Safe to run multiple times (idempotent).

  Covers DB changes for:
    • Forgot password flow
    • Provider Edit Profile columns
    • Provider Insurance Quote wizard
    • Provider Export Report + Report Templates
    • Property Administrator (Multi-Property Owner) registration
    • Realtor Edit Profile columns

  Prerequisites (must already exist):
    - dbo.AspNetUsers
    - dbo.Propiedades
    - dbo.IndorProveedores
    - dbo.IndorProveedorJobs      (for report photos FK)
    - dbo.IndorProveedorClientes  (for reports FK)
    - dbo.IndorRealtors           (for realtor profile ALTER)

  Most UI/validation bug fixes (languages, ZIP, back button, packing grid, etc.)
  do NOT require database changes — only code deploy.
  =============================================================================
*/

SET NOCOUNT ON;
PRINT '=== INDOR Bug-Fix Database Script — START ===';
PRINT CONVERT(VARCHAR(30), SYSUTCDATETIME(), 120) + ' UTC';
GO

------------------------------------------------------------
-- 0) Prerequisites
------------------------------------------------------------
IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
BEGIN
    RAISERROR('Missing table dbo.AspNetUsers. Run Identity/base scripts first.', 16, 1);
    RETURN;
END

IF OBJECT_ID(N'dbo.IndorProveedores', N'U') IS NULL
BEGIN
    RAISERROR('Missing table dbo.IndorProveedores. Run CreateProviderPortalTables.sql first.', 16, 1);
    RETURN;
END

PRINT '=== 1/8 Provider Edit Profile columns ===';
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'BusinessAddress') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD BusinessAddress NVARCHAR(300) NULL;
    PRINT '  + IndorProveedores.BusinessAddress';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'ServiceDescription') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD ServiceDescription NVARCHAR(200) NULL;
    PRINT '  + IndorProveedores.ServiceDescription';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'Latitude') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD Latitude DECIMAL(9,6) NULL;
    PRINT '  + IndorProveedores.Latitude';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'Longitude') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD Longitude DECIMAL(9,6) NULL;
    PRINT '  + IndorProveedores.Longitude';
END
GO

PRINT '=== 2/8 Password reset codes ===';
GO

IF OBJECT_ID(N'dbo.IndorPasswordResetCodes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPasswordResetCodes (
        Id            INT             IDENTITY(1,1) NOT NULL,
        UserId        NVARCHAR(450)   NOT NULL,
        Email         NVARCHAR(256)   NOT NULL,
        Code          NVARCHAR(10)    NOT NULL,
        ResetToken    NVARCHAR(2000)  NOT NULL,
        ExpiresUtc    DATETIME2       NOT NULL,
        Used          BIT             NOT NULL CONSTRAINT DF_IndorPwdReset_Used    DEFAULT (0),
        UsedUtc       DATETIME2       NULL,
        Attempts      INT             NOT NULL CONSTRAINT DF_IndorPwdReset_Attempts DEFAULT (0),
        FechaCreacion DATETIME2       NOT NULL CONSTRAINT DF_IndorPwdReset_Created  DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorPasswordResetCodes PRIMARY KEY CLUSTERED (Id)
    );
    CREATE INDEX IX_IndorPwdReset_Lookup ON dbo.IndorPasswordResetCodes(Email, Code, Used);
    CREATE INDEX IX_IndorPwdReset_User   ON dbo.IndorPasswordResetCodes(UserId, Used);
    PRINT '  + Table IndorPasswordResetCodes created';
END
ELSE
    PRINT '  = IndorPasswordResetCodes already exists';
GO

PRINT '=== 3/8 Property Administrator registration ===';
GO

IF OBJECT_ID(N'dbo.IndorPropertyAdministrators', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdministrators (
        Id                          INT              IDENTITY(1,1) NOT NULL,
        UserId                      NVARCHAR(450)    NULL,
        RegistrationToken           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IndorPropAdmin_Token DEFAULT (NEWID()),
        RegistrationStatus          NVARCHAR(30)     NOT NULL CONSTRAINT DF_IndorPropAdmin_Status DEFAULT (N'Draft'),
        CurrentStep                 INT              NOT NULL CONSTRAINT DF_IndorPropAdmin_Step DEFAULT (1),
        DisplayName                 NVARCHAR(120)    NULL,
        Email                       NVARCHAR(256)    NULL,
        Phone                       NVARCHAR(30)     NULL,
        PortfolioBusinessName       NVARCHAR(200)    NULL,
        TermsAccepted               BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_Terms DEFAULT (0),
        MarketingOptIn              BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_Marketing DEFAULT (0),
        TermsAcceptedUtc            DATETIME2(7)     NULL,
        PropertyCountRange          NVARCHAR(20)     NULL,
        PortfolioType               NVARCHAR(40)     NULL,
        OwnershipType               NVARCHAR(40)     NULL,
        PrimaryMarket               NVARCHAR(120)    NULL,
        ManagementStyle             NVARCHAR(40)     NULL,
        ToolMaintenanceRequests     BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolMaint DEFAULT (0),
        ToolTurnoverCleaning        BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolClean DEFAULT (0),
        ToolGuestMessaging          BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolMsg DEFAULT (0),
        ToolInvoicesPayments        BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolInv DEFAULT (0),
        ToolDocumentsWarranties     BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolDoc DEFAULT (0),
        ToolServiceProviders        BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolProv DEFAULT (0),
        ToolTeamAccess              BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolTeam DEFAULT (0),
        NotifyUrgentMaintenance     BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_NotifyUrgent DEFAULT (1),
        NotifyWeeklySummary         BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_NotifyWeekly DEFAULT (1),
        NotifyBookingLeaseUpdates   BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_NotifyLease DEFAULT (1),
        PlatformTermsAccepted       BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_PlatformTerms DEFAULT (0),
        RegistrationCompletedUtc    DATETIME2(7)     NULL,
        FechaCreacion               DATETIME2(7)     NOT NULL CONSTRAINT DF_IndorPropAdmin_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion          DATETIME2(7)     NULL,
        CONSTRAINT PK_IndorPropertyAdministrators PRIMARY KEY CLUSTERED (Id)
    );
    CREATE UNIQUE INDEX UX_IndorPropAdmin_Token ON dbo.IndorPropertyAdministrators (RegistrationToken);
    CREATE INDEX IX_IndorPropAdmin_UserId ON dbo.IndorPropertyAdministrators (UserId);
    ALTER TABLE dbo.IndorPropertyAdministrators WITH CHECK ADD CONSTRAINT FK_IndorPropAdmin_AspNetUsers
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id);
    PRINT '  + Table IndorPropertyAdministrators created';
END
ELSE
    PRINT '  = IndorPropertyAdministrators already exists';
GO

IF OBJECT_ID(N'dbo.IndorPropertyAdminPortfolioProperties', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdminPortfolioProperties (
        Id              INT            IDENTITY(1,1) NOT NULL,
        AdministratorId INT            NOT NULL,
        PropertyName    NVARCHAR(150)  NOT NULL,
        Location        NVARCHAR(200)  NOT NULL,
        PropertyType    NVARCHAR(40)   NOT NULL,
        ImageUrl        NVARCHAR(300)  NULL,
        PropiedadId     INT            NULL,
        Status          NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorPropAdminProp_Status DEFAULT (N'Added'),
        FechaCreacion   DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorPropAdminProp_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorPropertyAdminPortfolioProperties PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorPropAdminProp_Admin FOREIGN KEY (AdministratorId)
            REFERENCES dbo.IndorPropertyAdministrators(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorPropAdminProp_Propiedad FOREIGN KEY (PropiedadId)
            REFERENCES dbo.Propiedades(Id)
    );
    CREATE INDEX IX_IndorPropAdminProp_Admin ON dbo.IndorPropertyAdminPortfolioProperties(AdministratorId);
    PRINT '  + Table IndorPropertyAdminPortfolioProperties created';
END
ELSE
    PRINT '  = IndorPropertyAdminPortfolioProperties already exists';
GO

PRINT '=== 4/8 Provider reports (prerequisite for export flow) ===';
GO

IF OBJECT_ID(N'dbo.IndorProveedorReports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorReports (
        Id                INT            IDENTITY(1,1) NOT NULL,
        ProveedorId       INT            NOT NULL,
        JobId             INT            NULL,
        ClienteId         INT            NULL,
        ReportCode        NVARCHAR(30)   NOT NULL,
        Title             NVARCHAR(150)  NOT NULL,
        Address           NVARCHAR(250)  NOT NULL,
        CustomerName      NVARCHAR(120)  NULL,
        ServiceType       NVARCHAR(80)   NULL,
        Status            NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorProvRpt_Status DEFAULT (N'Draft'),
        PhotosCount       INT            NOT NULL CONSTRAINT DF_IndorProvRpt_Photos DEFAULT (0),
        HasChecklist      BIT            NOT NULL CONSTRAINT DF_IndorProvRpt_Checklist DEFAULT (0),
        HasWarranty       BIT            NOT NULL CONSTRAINT DF_IndorProvRpt_Warranty DEFAULT (0),
        HasDocuments      BIT            NOT NULL CONSTRAINT DF_IndorProvRpt_Docs DEFAULT (0),
        AddedToHouseFacts BIT            NOT NULL CONSTRAINT DF_IndorProvRpt_HouseFacts DEFAULT (0),
        CompletedUtc      DATETIME2      NULL,
        FechaCreacion     DATETIME2      NOT NULL CONSTRAINT DF_IndorProvRpt_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion DATETIME2     NULL,
        CONSTRAINT PK_IndorProveedorReports PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvRpt_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorProvRpt_Job FOREIGN KEY (JobId) REFERENCES dbo.IndorProveedorJobs(Id),
        CONSTRAINT FK_IndorProvRpt_Cliente FOREIGN KEY (ClienteId) REFERENCES dbo.IndorProveedorClientes(Id)
    );
    CREATE INDEX IX_IndorProvRpt_Proveedor ON dbo.IndorProveedorReports(ProveedorId, Status);
    PRINT '  + Table IndorProveedorReports created';
END
ELSE
    PRINT '  = IndorProveedorReports already exists';
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'ReportType') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD ReportType NVARCHAR(40) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'Summary') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Summary NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'WorkCompleted') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD WorkCompleted NVARCHAR(1000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'MaterialsUsed') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD MaterialsUsed NVARCHAR(1000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'WarrantyInfo') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD WarrantyInfo NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'Recommendations') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Recommendations NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'InternalNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD InternalNotes NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'SendToHomeowner') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD SendToHomeowner BIT NOT NULL
        CONSTRAINT DF_IndorProvRpt_SendHomeowner DEFAULT (1);
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'RequestApproval') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD RequestApproval BIT NOT NULL
        CONSTRAINT DF_IndorProvRpt_RequestApproval DEFAULT (0);
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'AttachToHouseFacts') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD AttachToHouseFacts BIT NOT NULL
        CONSTRAINT DF_IndorProvRpt_AttachHF DEFAULT (1);
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'PhotoUrlsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD PhotoUrlsJson NVARCHAR(2000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'DocumentsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD DocumentsJson NVARCHAR(2000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'FilesCount') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD FilesCount INT NOT NULL
        CONSTRAINT DF_IndorProvRpt_FilesCount DEFAULT (0);
GO

PRINT '=== 5/8 Export Report fields + photos ===';
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'ReportDate') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD ReportDate DATE NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'PreparedBy') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD PreparedBy NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'Category') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Category NVARCHAR(60) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'LocationDetail') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD LocationDetail NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'Priority') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Priority NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorReports', 'Weather') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Weather NVARCHAR(40) NULL;
GO

IF OBJECT_ID(N'dbo.IndorProveedorReportPhotos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorReportPhotos (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ReportId      INT            NOT NULL,
        ProveedorId   INT            NOT NULL,
        JobId         INT            NULL,
        Category      NVARCHAR(40)   NULL,
        FileUrl       NVARCHAR(500)  NOT NULL,
        FileName      NVARCHAR(260)  NULL,
        Caption       NVARCHAR(250)  NULL,
        SortOrder     INT            NOT NULL CONSTRAINT DF_IndorRptPhoto_Sort DEFAULT (0),
        FechaCreacion DATETIME2      NOT NULL CONSTRAINT DF_IndorRptPhoto_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorReportPhotos PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRptPhoto_Report    FOREIGN KEY (ReportId)
            REFERENCES dbo.IndorProveedorReports(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorRptPhoto_Proveedor FOREIGN KEY (ProveedorId)
            REFERENCES dbo.IndorProveedores(Id),
        CONSTRAINT FK_IndorRptPhoto_Job       FOREIGN KEY (JobId)
            REFERENCES dbo.IndorProveedorJobs(Id)
    );
    CREATE INDEX IX_IndorRptPhoto_Report ON dbo.IndorProveedorReportPhotos(ReportId, SortOrder);
    CREATE INDEX IX_IndorRptPhoto_Proveedor ON dbo.IndorProveedorReportPhotos(ProveedorId);
    PRINT '  + Table IndorProveedorReportPhotos created';
END
ELSE
    PRINT '  = IndorProveedorReportPhotos already exists';
GO

PRINT '=== 6/8 Report Templates ===';
GO

IF OBJECT_ID(N'dbo.IndorProveedorReportTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorReportTemplates (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ProveedorId   INT            NULL,
        TemplateKey   NVARCHAR(60)   NOT NULL,
        Name          NVARCHAR(120)  NOT NULL,
        Description   NVARCHAR(300)  NULL,
        Icon          NVARCHAR(40)   NOT NULL CONSTRAINT DF_IndorRptTpl_Icon  DEFAULT (N'fa-clipboard'),
        Color         NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorRptTpl_Color DEFAULT (N'blue'),
        Badge         NVARCHAR(40)   NULL,
        Category      NVARCHAR(40)   NOT NULL CONSTRAINT DF_IndorRptTpl_Cat   DEFAULT (N'Reports'),
        IsSystem      BIT            NOT NULL CONSTRAINT DF_IndorRptTpl_Sys    DEFAULT (0),
        IsCustom      BIT            NOT NULL CONSTRAINT DF_IndorRptTpl_Cust   DEFAULT (0),
        IsFavorite    BIT            NOT NULL CONSTRAINT DF_IndorRptTpl_Fav    DEFAULT (0),
        SortOrder     INT            NOT NULL CONSTRAINT DF_IndorRptTpl_Sort   DEFAULT (0),
        Activo        BIT            NOT NULL CONSTRAINT DF_IndorRptTpl_Activo DEFAULT (1),
        FechaCreacion DATETIME2      NOT NULL CONSTRAINT DF_IndorRptTpl_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorReportTemplates PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRptTpl_Proveedor FOREIGN KEY (ProveedorId)
            REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorRptTpl_Proveedor ON dbo.IndorProveedorReportTemplates(ProveedorId, Activo);
    PRINT '  + Table IndorProveedorReportTemplates created';
END
ELSE
    PRINT '  = IndorProveedorReportTemplates already exists';
GO

IF OBJECT_ID(N'dbo.IndorProveedorReportTemplateSections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorReportTemplateSections (
        Id         INT           IDENTITY(1,1) NOT NULL,
        TemplateId INT           NOT NULL,
        Label      NVARCHAR(80)  NOT NULL,
        Icon       NVARCHAR(40)  NOT NULL CONSTRAINT DF_IndorRptTplSec_Icon DEFAULT (N'fa-circle'),
        IsIncluded BIT           NOT NULL CONSTRAINT DF_IndorRptTplSec_Inc  DEFAULT (1),
        SortOrder  INT           NOT NULL CONSTRAINT DF_IndorRptTplSec_Sort DEFAULT (0),
        CONSTRAINT PK_IndorProveedorReportTemplateSections PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRptTplSec_Template FOREIGN KEY (TemplateId)
            REFERENCES dbo.IndorProveedorReportTemplates(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorRptTplSec_Template ON dbo.IndorProveedorReportTemplateSections(TemplateId, SortOrder);
    PRINT '  + Table IndorProveedorReportTemplateSections created';
END
ELSE
    PRINT '  = IndorProveedorReportTemplateSections already exists';
GO

DECLARE @tplId INT;

IF NOT EXISTS (SELECT 1 FROM dbo.IndorProveedorReportTemplates WHERE ProveedorId IS NULL AND TemplateKey = N'completion')
BEGIN
    INSERT dbo.IndorProveedorReportTemplates (ProveedorId, TemplateKey, Name, Description, Icon, Color, Badge, IsSystem, SortOrder)
    VALUES (NULL, N'completion', N'Completion Report', N'Finished work, photos, notes, and signatures.', N'fa-clipboard-check', N'blue', N'Most Used', 1, 1);
    SET @tplId = SCOPE_IDENTITY();
    INSERT dbo.IndorProveedorReportTemplateSections (TemplateId, Label, Icon, SortOrder) VALUES
        (@tplId, N'Job Info',       N'fa-briefcase',     1),
        (@tplId, N'Customer Info',  N'fa-user',          2),
        (@tplId, N'Scope of Work',  N'fa-file-lines',    3),
        (@tplId, N'Before Photos',  N'fa-camera',        4),
        (@tplId, N'After Photos',   N'fa-camera',        5),
        (@tplId, N'Materials Used', N'fa-box',           6),
        (@tplId, N'Notes',          N'fa-note-sticky',   7),
        (@tplId, N'Signature',      N'fa-signature',     8);
    PRINT '  + Seeded template: Completion Report';
END

IF NOT EXISTS (SELECT 1 FROM dbo.IndorProveedorReportTemplates WHERE ProveedorId IS NULL AND TemplateKey = N'inspection')
BEGIN
    INSERT dbo.IndorProveedorReportTemplates (ProveedorId, TemplateKey, Name, Description, Icon, Color, IsSystem, SortOrder)
    VALUES (NULL, N'inspection', N'Inspection Report', N'Findings, issues, and recommendations.', N'fa-magnifying-glass', N'green', 1, 2);
    SET @tplId = SCOPE_IDENTITY();
    INSERT dbo.IndorProveedorReportTemplateSections (TemplateId, Label, Icon, SortOrder) VALUES
        (@tplId, N'Job Info',         N'fa-briefcase',             1),
        (@tplId, N'Customer Info',    N'fa-user',                  2),
        (@tplId, N'Findings',         N'fa-triangle-exclamation',  3),
        (@tplId, N'Recommendations',  N'fa-lightbulb',             4),
        (@tplId, N'Photos',           N'fa-camera',                5),
        (@tplId, N'Notes',            N'fa-note-sticky',           6),
        (@tplId, N'Signature',        N'fa-signature',             7);
    PRINT '  + Seeded template: Inspection Report';
END

IF NOT EXISTS (SELECT 1 FROM dbo.IndorProveedorReportTemplates WHERE ProveedorId IS NULL AND TemplateKey = N'daily')
BEGIN
    INSERT dbo.IndorProveedorReportTemplates (ProveedorId, TemplateKey, Name, Description, Icon, Color, IsSystem, SortOrder)
    VALUES (NULL, N'daily', N'Daily Report', N'Crew activity, work completed, and site notes.', N'fa-calendar-days', N'orange', 1, 3);
    SET @tplId = SCOPE_IDENTITY();
    INSERT dbo.IndorProveedorReportTemplateSections (TemplateId, Label, Icon, SortOrder) VALUES
        (@tplId, N'Job Info',        N'fa-briefcase',   1),
        (@tplId, N'Crew',            N'fa-users',       2),
        (@tplId, N'Work Completed',  N'fa-list-check',  3),
        (@tplId, N'Site Notes',      N'fa-note-sticky', 4),
        (@tplId, N'Photos',          N'fa-camera',      5);
    PRINT '  + Seeded template: Daily Report';
END

IF NOT EXISTS (SELECT 1 FROM dbo.IndorProveedorReportTemplates WHERE ProveedorId IS NULL AND TemplateKey = N'photo')
BEGIN
    INSERT dbo.IndorProveedorReportTemplates (ProveedorId, TemplateKey, Name, Description, Icon, Color, IsSystem, SortOrder)
    VALUES (NULL, N'photo', N'Photo Report', N'Before-and-after photos with captions.', N'fa-camera', N'purple', 1, 4);
    SET @tplId = SCOPE_IDENTITY();
    INSERT dbo.IndorProveedorReportTemplateSections (TemplateId, Label, Icon, SortOrder) VALUES
        (@tplId, N'Job Info',       N'fa-briefcase',          1),
        (@tplId, N'Before Photos',  N'fa-camera',             2),
        (@tplId, N'After Photos',   N'fa-camera',             3),
        (@tplId, N'Captions',       N'fa-closed-captioning',  4),
        (@tplId, N'Notes',          N'fa-note-sticky',        5);
    PRINT '  + Seeded template: Photo Report';
END
GO

PRINT '=== 7/8 Provider Insurance Quotes ===';
GO

IF OBJECT_ID(N'dbo.IndorProviderInsuranceQuotes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProviderInsuranceQuotes (
        Id                   INT            IDENTITY(1,1) NOT NULL,
        ProveedorId          INT            NOT NULL,
        QuoteCode            NVARCHAR(40)   NOT NULL,
        Plan                 NVARCHAR(40)   NULL,
        Coverages            NVARCHAR(300)  NULL,
        Trade                NVARCHAR(120)  NULL,
        BusinessName         NVARCHAR(200)  NULL,
        BusinessAddress      NVARCHAR(300)  NULL,
        OwnerName            NVARCHAR(160)  NULL,
        OwnerDateOfBirth     DATE           NULL,
        OwnerPhone           NVARCHAR(40)   NULL,
        OwnerEmail           NVARCHAR(256)  NULL,
        NumberOfEmployees    INT            NULL,
        EmployeePayroll      DECIMAL(18,2)  NULL,
        CompanyGrossRevenue  DECIMAL(18,2)  NULL,
        YearsInBusiness      NVARCHAR(40)   NULL,
        ZipCode              NVARCHAR(20)   NULL,
        WorksAtCustomerHomes BIT            NULL,
        UsesSubcontractors   BIT            NULL,
        ConfirmedAccurate    BIT            NOT NULL CONSTRAINT DF_IndorInsQuote_Confirmed DEFAULT (0),
        Status               NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorInsQuote_Status    DEFAULT (N'Submitted'),
        SubmittedUtc         DATETIME2      NULL,
        FechaCreacion        DATETIME2      NOT NULL CONSTRAINT DF_IndorInsQuote_Created   DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProviderInsuranceQuotes PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorInsQuote_Proveedor FOREIGN KEY (ProveedorId)
            REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorInsQuote_Proveedor ON dbo.IndorProviderInsuranceQuotes(ProveedorId, Status);
    PRINT '  + Table IndorProviderInsuranceQuotes created';
END
ELSE
    PRINT '  = IndorProviderInsuranceQuotes already exists';
GO

IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'City') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD City NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'State') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD [State] NVARCHAR(40) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'NeedsCOI') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD NeedsCOI BIT NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PayTodayAmount') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PayTodayAmount DECIMAL(18,2) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'MonthlyAmount') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD MonthlyAmount DECIMAL(18,2) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PaymentMethod') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PaymentMethod NVARCHAR(40) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'CardLast4') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD CardLast4 NVARCHAR(8) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'AutoPayMonthly') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD AutoPayMonthly BIT NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'FirstBillingDate') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD FirstBillingDate DATE NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PaymentStatus') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PaymentStatus NVARCHAR(30) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PaymentAuthorized') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PaymentAuthorized BIT NOT NULL CONSTRAINT DF_IndorInsQuote_PayAuth DEFAULT (0);
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PaidUtc') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PaidUtc DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'ReceiptNumber') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD ReceiptNumber NVARCHAR(60) NULL;
GO

PRINT '=== 8/8 Realtor Edit Profile columns ===';
GO

IF OBJECT_ID(N'dbo.IndorRealtors', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.IndorRealtors', N'PublicDisplayName') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD PublicDisplayName NVARCHAR(120) NULL;
    IF COL_LENGTH(N'dbo.IndorRealtors', N'RealtorTitle') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD RealtorTitle NVARCHAR(80) NULL;
    IF COL_LENGTH(N'dbo.IndorRealtors', N'Website') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD Website NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dbo.IndorRealtors', N'OfficeCity') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD OfficeCity NVARCHAR(80) NULL;
    IF COL_LENGTH(N'dbo.IndorRealtors', N'OfficeState') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD OfficeState NVARCHAR(10) NULL;
    IF COL_LENGTH(N'dbo.IndorRealtors', N'OfficeZip') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD OfficeZip NVARCHAR(15) NULL;
    IF COL_LENGTH(N'dbo.IndorRealtors', N'YearsOfExperience') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD YearsOfExperience NVARCHAR(30) NULL;
    IF COL_LENGTH(N'dbo.IndorRealtors', N'SpecialtiesJson') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD SpecialtiesJson NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dbo.IndorRealtors', N'TeamName') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD TeamName NVARCHAR(120) NULL;
    IF COL_LENGTH(N'dbo.IndorRealtors', N'BrokerInCharge') IS NULL
        ALTER TABLE dbo.IndorRealtors ADD BrokerInCharge NVARCHAR(120) NULL;
    PRINT '  = Realtor edit profile columns verified';
END
ELSE
    PRINT '  ! Skipped — dbo.IndorRealtors not found (run CreateRealtorRegistrationTables.sql first)';
GO

------------------------------------------------------------
-- Verification summary
------------------------------------------------------------
PRINT '';
PRINT '=== VERIFICATION — tables ===';

SELECT
    t.ObjectName,
    CASE WHEN OBJECT_ID(t.ObjectName, N'U') IS NOT NULL THEN N'OK' ELSE N'MISSING' END AS [Status]
FROM (VALUES
    (N'dbo.IndorPasswordResetCodes'),
    (N'dbo.IndorPropertyAdministrators'),
    (N'dbo.IndorPropertyAdminPortfolioProperties'),
    (N'dbo.IndorProveedorReports'),
    (N'dbo.IndorProveedorReportPhotos'),
    (N'dbo.IndorProveedorReportTemplates'),
    (N'dbo.IndorProveedorReportTemplateSections'),
    (N'dbo.IndorProviderInsuranceQuotes')
) AS t(ObjectName);

PRINT '=== VERIFICATION — key columns ===';

SELECT
    c.TableName,
    c.ColumnName,
    CASE WHEN COL_LENGTH(N'dbo.' + c.TableName, c.ColumnName) IS NOT NULL THEN N'OK' ELSE N'MISSING' END AS [Status]
FROM (VALUES
    (N'IndorProveedores',             N'BusinessAddress'),
    (N'IndorProveedores',             N'Latitude'),
    (N'IndorProveedorReports',        N'ReportDate'),
    (N'IndorProviderInsuranceQuotes', N'PaymentAuthorized')
) AS c(TableName, ColumnName);

SELECT TemplateKey, Name, IsSystem
FROM dbo.IndorProveedorReportTemplates
WHERE ProveedorId IS NULL
ORDER BY SortOrder;

PRINT '';
PRINT '=== INDOR Bug-Fix Database Script — COMPLETE ===';
GO
