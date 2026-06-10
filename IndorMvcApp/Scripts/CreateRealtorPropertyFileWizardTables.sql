/*
  INDOR — Create Property File wizard (drafts + file items).
  Run after CreateRealtorRegistrationTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorRealtorPropertyFileDrafts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorPropertyFileDrafts (
        Id                  INT            IDENTITY(1,1) NOT NULL,
        RealtorId           INT            NOT NULL,
        CurrentStep         INT            NOT NULL CONSTRAINT DF_IndorPropDraft_Step DEFAULT (1),
        Status              NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorPropDraft_Status DEFAULT (N'Draft'),
        SourcePropertyId    INT            NULL,
        Title               NVARCHAR(150)  NULL,
        Address             NVARCHAR(250)  NULL,
        CityRegion          NVARCHAR(120)  NULL,
        ClientName          NVARCHAR(120)  NULL,
        PhotoUrl            NVARCHAR(500)  NULL,
        FilePhase           NVARCHAR(40)   NULL,
        NoteText            NVARCHAR(1000) NULL,
        CreateAndContinueLater BIT         NOT NULL CONSTRAINT DF_IndorPropDraft_Later DEFAULT (0),
        FechaCreacion       DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorPropDraft_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion  DATETIME2(7)   NULL,
        CONSTRAINT PK_IndorRealtorPropertyFileDrafts PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorPropDraft_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorPropDraft_Realtor ON dbo.IndorRealtorPropertyFileDrafts(RealtorId);
    PRINT 'Table IndorRealtorPropertyFileDrafts created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorPropertyFileDraftCategories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorPropertyFileDraftCategories (
        Id              INT            IDENTITY(1,1) NOT NULL,
        DraftId         INT            NOT NULL,
        CategoryType    NVARCHAR(40)   NOT NULL,
        CONSTRAINT PK_IndorRealtorPropertyFileDraftCategories PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorPropDraftCat_Draft FOREIGN KEY (DraftId) REFERENCES dbo.IndorRealtorPropertyFileDrafts(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX UX_IndorPropDraftCat ON dbo.IndorRealtorPropertyFileDraftCategories(DraftId, CategoryType);
    PRINT 'Table IndorRealtorPropertyFileDraftCategories created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorPropertyFileDraftItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorPropertyFileDraftItems (
        Id              INT            IDENTITY(1,1) NOT NULL,
        DraftId         INT            NOT NULL,
        CategoryType    NVARCHAR(40)   NOT NULL,
        ItemLabel       NVARCHAR(200)  NOT NULL,
        FileUrl         NVARCHAR(500)  NULL,
        NoteText        NVARCHAR(1000) NULL,
        FileSizeBytes   BIGINT         NULL,
        ExpirationUtc   DATETIME2(7)   NULL,
        UploadedUtc     DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorPropDraftItem_Upload DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorRealtorPropertyFileDraftItems PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorPropDraftItem_Draft FOREIGN KEY (DraftId) REFERENCES dbo.IndorRealtorPropertyFileDrafts(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorPropDraftItem_Draft ON dbo.IndorRealtorPropertyFileDraftItems(DraftId);
    PRINT 'Table IndorRealtorPropertyFileDraftItems created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorPropertyFileItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorPropertyFileItems (
        Id              INT            IDENTITY(1,1) NOT NULL,
        PropertyFileId  INT            NOT NULL,
        CategoryType    NVARCHAR(40)   NOT NULL,
        ItemLabel       NVARCHAR(200)  NOT NULL,
        FileUrl         NVARCHAR(500)  NULL,
        NoteText        NVARCHAR(1000) NULL,
        FileSizeBytes   BIGINT         NULL,
        ExpirationUtc   DATETIME2(7)   NULL,
        UploadedUtc     DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorPropFileItem_Upload DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorRealtorPropertyFileItems PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorPropFileItem_File FOREIGN KEY (PropertyFileId) REFERENCES dbo.IndorRealtorPropertyFiles(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorPropFileItem_File ON dbo.IndorRealtorPropertyFileItems(PropertyFileId);
    PRINT 'Table IndorRealtorPropertyFileItems created.';
END
GO

PRINT 'Property file wizard tables ready.';
