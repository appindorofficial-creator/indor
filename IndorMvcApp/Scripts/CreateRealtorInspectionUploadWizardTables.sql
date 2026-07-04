/*
  INDOR — Upload Inspection Report wizard tables (Realtor).
  Run after CreateRealtorQuoteRequestWizardTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorRealtorInspectionUploadDrafts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorInspectionUploadDrafts (
        Id                      INT            IDENTITY(1,1) NOT NULL,
        RealtorId               INT            NOT NULL,
        CurrentStep             INT            NOT NULL CONSTRAINT DF_IndorInspDraft_Step DEFAULT (1),
        Status                  NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorInspDraft_Status DEFAULT (N'Draft'),
        PropertyFileId          INT            NULL,
        Address                 NVARCHAR(250)  NULL,
        CityRegion              NVARCHAR(120)  NULL,
        ClientName              NVARCHAR(120)  NULL,
        PhotoUrl                NVARCHAR(500)  NULL,
        ReportFileUrl           NVARCHAR(500)  NULL,
        ReportFileName          NVARCHAR(200)  NULL,
        ReportPageCount         INT            NOT NULL CONSTRAINT DF_IndorInspDraft_Pages DEFAULT (0),
        UploadMethod            NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorInspDraft_Method DEFAULT (N'Pdf'),
        AnalysisProgress        INT            NOT NULL CONSTRAINT DF_IndorInspDraft_Progress DEFAULT (0),
        AnalysisStatus          NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorInspDraft_Analysis DEFAULT (N'Pending'),
        ResponseDeadlineHours   INT            NOT NULL CONSTRAINT DF_IndorInspDraft_Deadline DEFAULT (48),
        FechaCreacion           DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorInspDraft_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion      DATETIME2(7)   NULL,
        CONSTRAINT PK_IndorRealtorInspectionUploadDrafts PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorInspDraft_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorInspDraft_Realtor ON dbo.IndorRealtorInspectionUploadDrafts(RealtorId);
    PRINT 'Table IndorRealtorInspectionUploadDrafts created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorInspectionUploadFindings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorInspectionUploadFindings (
        Id              INT            IDENTITY(1,1) NOT NULL,
        DraftId         INT            NOT NULL,
        Title           NVARCHAR(200)  NOT NULL,
        Priority        NVARCHAR(20)   NOT NULL,
        Trade           NVARCHAR(40)   NOT NULL,
        TradeLabel      NVARCHAR(60)   NOT NULL,
        AiScore         INT            NOT NULL CONSTRAINT DF_IndorInspFinding_Score DEFAULT (80),
        ImageUrl        NVARCHAR(500)  NULL,
        SortOrder       INT            NOT NULL CONSTRAINT DF_IndorInspFinding_Sort DEFAULT (0),
        IsSelected      BIT            NOT NULL CONSTRAINT DF_IndorInspFinding_Selected DEFAULT (0),
        CONSTRAINT PK_IndorRealtorInspectionUploadFindings PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorInspFinding_Draft FOREIGN KEY (DraftId) REFERENCES dbo.IndorRealtorInspectionUploadDrafts(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorInspFinding_Draft ON dbo.IndorRealtorInspectionUploadFindings(DraftId);
    PRINT 'Table IndorRealtorInspectionUploadFindings created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorInspectionDraftProviders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorInspectionDraftProviders (
        Id              INT IDENTITY(1,1) NOT NULL,
        DraftId         INT NOT NULL,
        Trade           NVARCHAR(40) NOT NULL,
        ProviderId      INT NOT NULL,
        CONSTRAINT PK_IndorRealtorInspectionDraftProviders PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorInspDraftProv_Draft FOREIGN KEY (DraftId) REFERENCES dbo.IndorRealtorInspectionUploadDrafts(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX UX_IndorInspDraftProv ON dbo.IndorRealtorInspectionDraftProviders(DraftId, Trade, ProviderId);
    PRINT 'Table IndorRealtorInspectionDraftProviders created.';
END
GO

-- NOTE: No sample/placeholder providers are seeded here. Inspection-trade
-- providers come only from real, registered providers (dbo.IndorProveedores).
-- Seeding fictional companies was removed to comply with App Store
-- guideline 2.1(a) (App Completeness — no placeholder content).
GO

PRINT 'Inspection upload wizard tables ready.';
