/*
  INDOR — Post a Request wizard (categories, photos, offers, extended request fields).
  Run after CreateNeighborRequestTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorNeighborRequestCategories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborRequestCategories (
        Id            INT           IDENTITY(1,1) NOT NULL,
        Code          NVARCHAR(40)  NOT NULL,
        LabelEn       NVARCHAR(80)  NOT NULL,
        DescriptionEn NVARCHAR(200) NULL,
        IconClass     NVARCHAR(60)  NOT NULL CONSTRAINT DF_IndorNbrReqCat_Icon DEFAULT (N'fa-circle'),
        SortOrder     INT           NOT NULL CONSTRAINT DF_IndorNbrReqCat_Sort DEFAULT (0),
        IsActive      BIT           NOT NULL CONSTRAINT DF_IndorNbrReqCat_Active DEFAULT (1),
        CONSTRAINT PK_IndorNeighborRequestCategories PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_IndorNbrReqCat_Code UNIQUE (Code)
    );
    PRINT 'Table IndorNeighborRequestCategories created.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.IndorNeighborRequestCategories)
BEGIN
    INSERT INTO dbo.IndorNeighborRequestCategories (Code, LabelEn, DescriptionEn, IconClass, SortOrder)
    VALUES
        (N'home-improvements', N'Home Improvement', N'Repairs, maintenance, upgrades', N'fa-house', 10),
        (N'yard-patio',        N'Lawn & Yard',      N'Mowing, landscaping, cleanup', N'fa-seedling', 20),
        (N'cleaning',          N'Cleaning',           N'Home, windows, gutters', N'fa-wand-magic-sparkles', 30),
        (N'moving-hauling',    N'Moving & Hauling',   N'Moving help, junk removal', N'fa-dolly', 40),
        (N'tech-internet',     N'Tech & Internet',    N'Wi-Fi, devices, smart home', N'fa-wifi', 50),
        (N'other',             N'Other',              N'Something else', N'fa-ellipsis', 60);
    PRINT 'Neighbor request categories seeded.';
END
GO

UPDATE dbo.IndorNeighborRequestCategories SET LabelEn = N'Home Improvement', DescriptionEn = N'Repairs, maintenance, upgrades', IconClass = N'fa-house', SortOrder = 10 WHERE Code = N'home-improvements';
UPDATE dbo.IndorNeighborRequestCategories SET LabelEn = N'Lawn & Yard', DescriptionEn = N'Mowing, landscaping, cleanup', IconClass = N'fa-seedling', SortOrder = 20 WHERE Code = N'yard-patio';
UPDATE dbo.IndorNeighborRequestCategories SET LabelEn = N'Cleaning', DescriptionEn = N'Home, windows, gutters', IconClass = N'fa-wand-magic-sparkles', SortOrder = 30 WHERE Code = N'cleaning';
UPDATE dbo.IndorNeighborRequestCategories SET LabelEn = N'Moving & Hauling', DescriptionEn = N'Moving help, junk removal', IconClass = N'fa-dolly', SortOrder = 40 WHERE Code = N'moving-hauling';
UPDATE dbo.IndorNeighborRequestCategories SET LabelEn = N'Tech & Internet', DescriptionEn = N'Wi-Fi, devices, smart home', IconClass = N'fa-wifi', SortOrder = 50 WHERE Code = N'tech-internet';
UPDATE dbo.IndorNeighborRequestCategories SET LabelEn = N'Other', DescriptionEn = N'Something else', IconClass = N'fa-ellipsis', SortOrder = 60 WHERE Code = N'other';
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'CategoryId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD CategoryId INT NULL;
    PRINT 'Column IndorNeighborRequests.CategoryId added.';
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'LocationAddress') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD LocationAddress NVARCHAR(500) NULL;
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'NeededByDate') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD NeededByDate DATE NULL;
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'TimelineCode') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD TimelineCode NVARCHAR(30) NOT NULL CONSTRAINT DF_IndorNbrReq_Timeline DEFAULT (N'ThisWeek');
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'AudienceCode') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD AudienceCode NVARCHAR(30) NOT NULL CONSTRAINT DF_IndorNbrReq_Audience DEFAULT (N'Neighbors');
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'BudgetAmount') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD BudgetAmount DECIMAL(12,2) NULL;
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'Status') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD Status NVARCHAR(30) NOT NULL CONSTRAINT DF_IndorNbrReq_Status DEFAULT (N'Active');
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'PublishedUtc') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD PublishedUtc DATETIME2(7) NULL;
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'UpdatedUtc') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD UpdatedUtc DATETIME2(7) NULL;
END
GO

UPDATE r
SET r.CategoryId = c.Id
FROM dbo.IndorNeighborRequests r
CROSS JOIN (SELECT TOP 1 Id FROM dbo.IndorNeighborRequestCategories WHERE Code = N'other' ORDER BY Id) c
WHERE r.CategoryId IS NULL;
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'CategoryId') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.IndorNeighborRequests WHERE CategoryId IS NULL)
BEGIN
    UPDATE r SET CategoryId = (SELECT TOP 1 Id FROM dbo.IndorNeighborRequestCategories ORDER BY SortOrder, Id)
    FROM dbo.IndorNeighborRequests r WHERE r.CategoryId IS NULL;
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'CategoryId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_IndorNbrReq_Category')
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests
        ADD CONSTRAINT FK_IndorNbrReq_Category
        FOREIGN KEY (CategoryId) REFERENCES dbo.IndorNeighborRequestCategories(Id);
END
GO

UPDATE dbo.IndorNeighborRequests
SET PublishedUtc = CreatedUtc, Status = N'Active'
WHERE PublishedUtc IS NULL AND IsActive = 1;
GO

IF OBJECT_ID(N'dbo.IndorNeighborRequestPhotos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborRequestPhotos (
        Id         INT           IDENTITY(1,1) NOT NULL,
        RequestId  INT           NOT NULL,
        FilePath   NVARCHAR(500) NOT NULL,
        SortOrder  INT           NOT NULL CONSTRAINT DF_IndorNbrReqPhoto_Sort DEFAULT (0),
        CreatedUtc DATETIME2(7)  NOT NULL CONSTRAINT DF_IndorNbrReqPhoto_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorNeighborRequestPhotos PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNbrReqPhoto_Request FOREIGN KEY (RequestId) REFERENCES dbo.IndorNeighborRequests(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorNbrReqPhoto_Request ON dbo.IndorNeighborRequestPhotos(RequestId, SortOrder);
    PRINT 'Table IndorNeighborRequestPhotos created.';
END
GO

IF OBJECT_ID(N'dbo.IndorNeighborRequestOffers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNeighborRequestOffers (
        Id               INT           IDENTITY(1,1) NOT NULL,
        RequestId        INT           NOT NULL,
        OfferType        NVARCHAR(30)  NOT NULL,
        ProviderId       INT           NULL,
        ResponderUserId  NVARCHAR(450) NULL,
        OffererName      NVARCHAR(120) NOT NULL,
        OffererPhotoUrl  NVARCHAR(500) NULL,
        Message          NVARCHAR(500) NULL,
        PriceAmount      DECIMAL(12,2) NULL,
        DistanceMiles    DECIMAL(5,2)  NULL,
        IsVerified       BIT           NOT NULL CONSTRAINT DF_IndorNbrReqOffer_Verified DEFAULT (0),
        Status           NVARCHAR(30)  NOT NULL CONSTRAINT DF_IndorNbrReqOffer_Status DEFAULT (N'Pending'),
        CreatedUtc       DATETIME2(7)  NOT NULL CONSTRAINT DF_IndorNbrReqOffer_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorNeighborRequestOffers PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNbrReqOffer_Request FOREIGN KEY (RequestId) REFERENCES dbo.IndorNeighborRequests(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorNbrReqOffer_Request ON dbo.IndorNeighborRequestOffers(RequestId, Status, CreatedUtc DESC);
    PRINT 'Table IndorNeighborRequestOffers created.';
END
GO
