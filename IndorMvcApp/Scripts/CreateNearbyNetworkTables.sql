/*
  INDOR — Nearby Network tables for Realtor portal feed (listings, leads, providers, etc.).
  Run after ExtendRealtorPortalTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorNearbyNetworkSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNearbyNetworkSettings (
        Id               INT            IDENTITY(1,1) NOT NULL,
        RealtorId        INT            NOT NULL,
        CenterLabel      NVARCHAR(200)  NOT NULL CONSTRAINT DF_IndorNearbyNetSet_Label DEFAULT (N'Your service area'),
        CenterAddress    NVARCHAR(250)  NULL,
        CenterLatitude   DECIMAL(9,6)   NULL,
        CenterLongitude  DECIMAL(9,6)   NULL,
        RadiusMiles      DECIMAL(4,1)   NOT NULL CONSTRAINT DF_IndorNearbyNetSet_Radius DEFAULT (3.0),
        FechaCreacion    DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorNearbyNetSet_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion DATETIME2(7) NULL,
        CONSTRAINT PK_IndorNearbyNetworkSettings PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNearbyNetSet_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_IndorNearbyNetSet_Realtor UNIQUE (RealtorId)
    );
    CREATE INDEX IX_IndorNearbyNetSet_Realtor ON dbo.IndorNearbyNetworkSettings(RealtorId);
    PRINT 'Table IndorNearbyNetworkSettings created.';
END
GO

IF OBJECT_ID(N'dbo.IndorNearbyNetworkItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorNearbyNetworkItems (
        Id                   INT            IDENTITY(1,1) NOT NULL,
        OwnerRealtorId       INT            NULL,
        CardType             NVARCHAR(30)   NOT NULL,
        FilterCategory       NVARCHAR(30)   NOT NULL,
        BadgeLabel           NVARCHAR(40)   NOT NULL CONSTRAINT DF_IndorNearbyNetItem_Badge DEFAULT (N''),
        BadgeCss             NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorNearbyNetItem_BadgeCss DEFAULT (N'listing'),
        Title                NVARCHAR(200)  NOT NULL,
        Subtitle             NVARCHAR(300)  NULL,
        Price                DECIMAL(12,2)  NULL,
        Bedrooms             DECIMAL(3,1)   NULL,
        Bathrooms            DECIMAL(3,1)   NULL,
        SquareFeet           INT            NULL,
        SpecsLabel           NVARCHAR(200)  NULL,
        ImageUrl             NVARCHAR(500)  NULL,
        IconClass            NVARCHAR(60)   NULL,
        MetaLabel            NVARCHAR(200)  NULL,
        TagsJson             NVARCHAR(500)  NULL,
        DistanceMiles        DECIMAL(5,2)   NULL,
        Latitude             DECIMAL(9,6)   NULL,
        Longitude            DECIMAL(9,6)   NULL,
        StatusBadge          NVARCHAR(40)   NULL,
        StatusCss            NVARCHAR(30)   NULL,
        PrimaryActionLabel   NVARCHAR(60)   NOT NULL CONSTRAINT DF_IndorNearbyNetItem_PrimaryLbl DEFAULT (N'View'),
        PrimaryActionUrl     NVARCHAR(300)  NOT NULL CONSTRAINT DF_IndorNearbyNetItem_PrimaryUrl DEFAULT (N'#'),
        SecondaryActionLabel NVARCHAR(60)   NULL,
        SecondaryActionUrl   NVARCHAR(300)  NULL,
        IsActive             BIT            NOT NULL CONSTRAINT DF_IndorNearbyNetItem_Active DEFAULT (1),
        IsOwnedListing       BIT            NOT NULL CONSTRAINT DF_IndorNearbyNetItem_Owned DEFAULT (0),
        SortOrder            INT            NOT NULL CONSTRAINT DF_IndorNearbyNetItem_Sort DEFAULT (0),
        RelatedClientId      INT            NULL,
        ProviderName         NVARCHAR(120)  NULL,
        CreatedUtc           DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorNearbyNetItem_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedUtc           DATETIME2(7)   NULL,
        ExpiresUtc           DATETIME2(7)   NULL,
        CONSTRAINT PK_IndorNearbyNetworkItems PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorNearbyNetItem_OwnerRealtor FOREIGN KEY (OwnerRealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_IndorNearbyNetItem_Client FOREIGN KEY (RelatedClientId) REFERENCES dbo.IndorRealtorClients(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_IndorNearbyNetItem_Active ON dbo.IndorNearbyNetworkItems(IsActive, FilterCategory, SortOrder);
    CREATE INDEX IX_IndorNearbyNetItem_Owner ON dbo.IndorNearbyNetworkItems(OwnerRealtorId, IsOwnedListing);
    CREATE INDEX IX_IndorNearbyNetItem_CardType ON dbo.IndorNearbyNetworkItems(CardType);
    PRINT 'Table IndorNearbyNetworkItems created.';
END
GO
