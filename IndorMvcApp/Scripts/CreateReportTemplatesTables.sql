/*
  INDOR PRO — Report Templates flow persistence.
  Creates the templates catalog (header + sections) and seeds the
  four system "Most Used" templates.

  - ProveedorId NULL  => global/system template (shown to everyone).
  - ProveedorId set   => provider-owned custom template ("My Templates").

  Safe to re-run: objects/seed are created only if missing.
  Run order: after CreateProviderOperationsTables.sql.
*/

------------------------------------------------------------
-- 1) Templates header table
------------------------------------------------------------
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
END
GO

------------------------------------------------------------
-- 2) Template sections table
------------------------------------------------------------
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
END
GO

------------------------------------------------------------
-- 3) Seed system "Most Used" templates (only if not present)
------------------------------------------------------------
DECLARE @tplId INT;

-- Completion Report --------------------------------------------------
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
END

-- Inspection Report --------------------------------------------------
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
END

-- Daily Report -------------------------------------------------------
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
END

-- Photo Report -------------------------------------------------------
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
END
GO
