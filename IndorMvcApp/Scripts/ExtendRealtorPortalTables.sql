/*
  INDOR — Extend Realtor portal tables for Home, Clients, Files, Quotes, Profile screens.
  Run after CreateRealtorRegistrationTables.sql. Safe to run multiple times.
*/

-- Realtor profile photo
IF COL_LENGTH('dbo.IndorRealtors', 'ProfilePhotoUrl') IS NULL
    ALTER TABLE dbo.IndorRealtors ADD ProfilePhotoUrl NVARCHAR(500) NULL;
GO

-- Property file extras
IF COL_LENGTH('dbo.IndorRealtorPropertyFiles', 'ClientName') IS NULL
    ALTER TABLE dbo.IndorRealtorPropertyFiles ADD ClientName NVARCHAR(120) NULL;
IF COL_LENGTH('dbo.IndorRealtorPropertyFiles', 'FilePhase') IS NULL
    ALTER TABLE dbo.IndorRealtorPropertyFiles ADD FilePhase NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.IndorRealtorPropertyFiles', 'RepairItemsCount') IS NULL
    ALTER TABLE dbo.IndorRealtorPropertyFiles ADD RepairItemsCount INT NOT NULL CONSTRAINT DF_IndorRealtorProp_Repair DEFAULT (0);
IF COL_LENGTH('dbo.IndorRealtorPropertyFiles', 'QuotesReceivedCount') IS NULL
    ALTER TABLE dbo.IndorRealtorPropertyFiles ADD QuotesReceivedCount INT NOT NULL CONSTRAINT DF_IndorRealtorProp_Quotes DEFAULT (0);
IF COL_LENGTH('dbo.IndorRealtorPropertyFiles', 'UpdatedUtc') IS NULL
    ALTER TABLE dbo.IndorRealtorPropertyFiles ADD UpdatedUtc DATETIME2(7) NULL;
GO

-- Quote extras
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'ClientName') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD ClientName NVARCHAR(120) NULL;
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'PhotoUrl') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD PhotoUrl NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'ProviderQuotesReceived') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD ProviderQuotesReceived INT NOT NULL CONSTRAINT DF_IndorRealtorQuote_Recv DEFAULT (0);
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'FooterNote') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD FooterNote NVARCHAR(120) NULL;
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'UpdatedUtc') IS NULL
    ALTER TABLE dbo.IndorRealtorQuotes ADD UpdatedUtc DATETIME2(7) NULL;
GO

IF OBJECT_ID(N'dbo.IndorRealtorClients', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorClients (
        Id              INT            IDENTITY(1,1) NOT NULL,
        RealtorId       INT            NOT NULL,
        FullName        NVARCHAR(120)  NOT NULL,
        Email           NVARCHAR(256)  NULL,
        ClientRole      NVARCHAR(20)   NOT NULL,
        ProfileImageUrl NVARCHAR(500)  NULL,
        PropertyAddress NVARCHAR(250)  NULL,
        StatusSummary   NVARCHAR(120)  NULL,
        LastActiveUtc   DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorRealtorClient_Active DEFAULT (SYSUTCDATETIME()),
        FechaCreacion   DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorRealtorClient_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorRealtorClients PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRealtorClient_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorRealtorClient_Realtor ON dbo.IndorRealtorClients(RealtorId);
    PRINT 'Table IndorRealtorClients created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorInvitations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorInvitations (
        Id              INT            IDENTITY(1,1) NOT NULL,
        RealtorId       INT            NOT NULL,
        FullName        NVARCHAR(120)  NOT NULL,
        Email           NVARCHAR(256)  NOT NULL,
        Status          NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorRealtorInv_Status DEFAULT (N'Sent'),
        SentUtc         DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorRealtorInv_Sent DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorRealtorInvitations PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRealtorInv_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorRealtorInv_Realtor ON dbo.IndorRealtorInvitations(RealtorId);
    PRINT 'Table IndorRealtorInvitations created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorActivities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorActivities (
        Id              INT            IDENTITY(1,1) NOT NULL,
        RealtorId       INT            NOT NULL,
        ActivityType    NVARCHAR(30)   NOT NULL,
        Description     NVARCHAR(300)  NOT NULL,
        CategoryTag     NVARCHAR(30)   NOT NULL,
        OccurredUtc     DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorRealtorAct_Occurred DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorRealtorActivities PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRealtorAct_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorRealtorAct_Realtor ON dbo.IndorRealtorActivities(RealtorId);
    PRINT 'Table IndorRealtorActivities created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorQuoteBids', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorQuoteBids (
        Id              INT            IDENTITY(1,1) NOT NULL,
        QuoteId         INT            NOT NULL,
        ProviderName    NVARCHAR(120)  NOT NULL,
        Amount          DECIMAL(12,2)  NOT NULL,
        Rating          DECIMAL(2,1)   NOT NULL CONSTRAINT DF_IndorRealtorBid_Rating DEFAULT (4.5),
        SortOrder       INT            NOT NULL CONSTRAINT DF_IndorRealtorBid_Sort DEFAULT (0),
        CONSTRAINT PK_IndorRealtorQuoteBids PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRealtorBid_Quote FOREIGN KEY (QuoteId) REFERENCES dbo.IndorRealtorQuotes(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorRealtorBid_Quote ON dbo.IndorRealtorQuoteBids(QuoteId);
    PRINT 'Table IndorRealtorQuoteBids created.';
END
GO

PRINT 'Realtor portal tables extended.';
