/*
  INDOR — Request Quote wizard tables + provider catalog.
  Run after CreateRealtorRegistrationTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorRealtorQuoteProviders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorQuoteProviders (
        Id              INT            IDENTITY(1,1) NOT NULL,
        CompanyName     NVARCHAR(120)  NOT NULL,
        LogoUrl         NVARCHAR(500)  NULL,
        Categories      NVARCHAR(200)  NOT NULL,
        Rating          DECIMAL(2,1)   NOT NULL CONSTRAINT DF_IndorQuoteProv_Rating DEFAULT (4.5),
        DistanceMiles   DECIMAL(4,1)   NOT NULL CONSTRAINT DF_IndorQuoteProv_Dist DEFAULT (5.0),
        BadgeLabel      NVARCHAR(40)   NULL,
        IsVerified      BIT            NOT NULL CONSTRAINT DF_IndorQuoteProv_Verified DEFAULT (1),
        IsRecommended   BIT            NOT NULL CONSTRAINT DF_IndorQuoteProv_Recommended DEFAULT (0),
        IsActive        BIT            NOT NULL CONSTRAINT DF_IndorQuoteProv_Active DEFAULT (1),
        CONSTRAINT PK_IndorRealtorQuoteProviders PRIMARY KEY CLUSTERED (Id)
    );
    PRINT 'Table IndorRealtorQuoteProviders created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorQuoteRequestDrafts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorQuoteRequestDrafts (
        Id                      INT            IDENTITY(1,1) NOT NULL,
        RealtorId               INT            NOT NULL,
        CurrentStep             INT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_Step DEFAULT (1),
        Status                  NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorQuoteDraft_Status DEFAULT (N'Draft'),
        PropertyFileId          INT            NULL,
        Address                 NVARCHAR(250)  NULL,
        CityRegion              NVARCHAR(120)  NULL,
        ClientName              NVARCHAR(120)  NULL,
        PhotoUrl                NVARCHAR(500)  NULL,
        FilePhase               NVARCHAR(40)   NULL,
        ServiceType             NVARCHAR(120)  NULL,
        RequestType             NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorQuoteDraft_ReqType DEFAULT (N'EntireFile'),
        SharePhotosVideos       BIT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_Photos DEFAULT (1),
        ShareInspectionReport   BIT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_Insp DEFAULT (1),
        ShareRepairItems        BIT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_Repair DEFAULT (1),
        ShareNotes              BIT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_Notes DEFAULT (1),
        ResponseDeadlineHours   INT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_Deadline DEFAULT (48),
        ProviderSelectionMode   NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorQuoteDraft_ProvMode DEFAULT (N'Manual'),
        ProviderCountTarget     INT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_ProvCount DEFAULT (3),
        VerifiedOnly            BIT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_Verified DEFAULT (1),
        Priority                NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorQuoteDraft_Priority DEFAULT (N'FastResponse'),
        CoverageMiles           INT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_Coverage DEFAULT (10),
        SendNow                 BIT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_SendNow DEFAULT (1),
        ScheduledSendUtc        DATETIME2(7)   NULL,
        AllowProviderQuestions  BIT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_Questions DEFAULT (1),
        AllowFullProjectQuote   BIT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_FullQuote DEFAULT (1),
        AllowItemizedQuote      BIT            NOT NULL CONSTRAINT DF_IndorQuoteDraft_ItemQuote DEFAULT (1),
        OptionalMessage         NVARCHAR(500)  NULL,
        FechaCreacion           DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorQuoteDraft_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion      DATETIME2(7)   NULL,
        CONSTRAINT PK_IndorRealtorQuoteRequestDrafts PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorQuoteDraft_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorQuoteDraft_Realtor ON dbo.IndorRealtorQuoteRequestDrafts(RealtorId);
    PRINT 'Table IndorRealtorQuoteRequestDrafts created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorQuoteRequestDraftProviders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorQuoteRequestDraftProviders (
        Id              INT IDENTITY(1,1) NOT NULL,
        DraftId         INT NOT NULL,
        ProviderId      INT NOT NULL,
        CONSTRAINT PK_IndorRealtorQuoteRequestDraftProviders PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorQuoteDraftProv_Draft FOREIGN KEY (DraftId) REFERENCES dbo.IndorRealtorQuoteRequestDrafts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorQuoteDraftProv_Provider FOREIGN KEY (ProviderId) REFERENCES dbo.IndorRealtorQuoteProviders(Id)
    );
    CREATE UNIQUE INDEX UX_IndorQuoteDraftProv ON dbo.IndorRealtorQuoteRequestDraftProviders(DraftId, ProviderId);
    PRINT 'Table IndorRealtorQuoteRequestDraftProviders created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorQuoteSentProviders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorQuoteSentProviders (
        Id              INT IDENTITY(1,1) NOT NULL,
        QuoteId         INT NOT NULL,
        ProviderId      INT NOT NULL,
        ProviderName    NVARCHAR(120) NOT NULL,
        CONSTRAINT PK_IndorRealtorQuoteSentProviders PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorQuoteSentProv_Quote FOREIGN KEY (QuoteId) REFERENCES dbo.IndorRealtorQuotes(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorQuoteSentProv_Quote ON dbo.IndorRealtorQuoteSentProviders(QuoteId);
    PRINT 'Table IndorRealtorQuoteSentProviders created.';
END
GO

-- Extend IndorRealtorQuotes for request wizard data
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'PropertyFileId') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD PropertyFileId INT NULL;
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'RequestType') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD RequestType NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'ResponseDeadlineHours') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD ResponseDeadlineHours INT NULL;
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'ProviderSelectionMode') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD ProviderSelectionMode NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'OptionalMessage') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD OptionalMessage NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'SentUtc') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD SentUtc DATETIME2(7) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.IndorRealtorQuoteProviders)
BEGIN
    INSERT INTO dbo.IndorRealtorQuoteProviders (CompanyName, Categories, Rating, DistanceMiles, BadgeLabel, IsVerified, IsRecommended)
    VALUES
        (N'Safe HVAC Solution', N'HVAC / Electrical', 4.9, 4.2, N'Verified', 1, 1),
        (N'Prime Mechanical', N'HVAC / Plumbing', 4.8, 6.1, N'Available', 1, 1),
        (N'CoolAir Pros', N'HVAC Specialist', 4.9, 5.0, N'Fast Response', 1, 1),
        (N'Elite Roofing Co', N'Roofing / Gutters', 4.7, 8.3, N'Verified', 1, 0),
        (N'Quick Fix Plumbing', N'Plumbing', 4.6, 3.5, N'Nearby', 0, 0);
    PRINT 'Quote providers seeded.';
END
GO

PRINT 'Quote request wizard tables ready.';
