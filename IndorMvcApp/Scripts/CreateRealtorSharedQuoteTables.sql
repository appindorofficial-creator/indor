IF OBJECT_ID(N'dbo.IndorRealtorSharedQuotes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorSharedQuotes (
        Id INT IDENTITY(1,1) NOT NULL,
        RealtorId INT NOT NULL,
        QuoteId INT NOT NULL,
        BidId INT NOT NULL,
        ShareToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Token DEFAULT NEWID(),
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Status DEFAULT N'Draft',
        HomeownerName NVARCHAR(120) NOT NULL,
        HomeownerEmail NVARCHAR(256) NULL,
        HomeownerPhone NVARCHAR(30) NULL,
        ShareProviderInfo BIT NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Prov DEFAULT (1),
        ShareFullPriceBreakdown BIT NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Price DEFAULT (0),
        ShareScopeOfWork BIT NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Scope DEFAULT (1),
        ShareWarranty BIT NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Warr DEFAULT (1),
        ShareIncludedRepairs BIT NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Rep DEFAULT (1),
        ShareTimeline BIT NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Time DEFAULT (1),
        PricingDisplayMode NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Pricing DEFAULT N'TotalOnly',
        MessageToHomeowner NVARCHAR(500) NULL,
        InternalNotes NVARCHAR(500) NULL,
        DeliveryMethod NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Delivery DEFAULT N'InApp',
        SentUtc DATETIME2(7) NULL,
        DeliveredUtc DATETIME2(7) NULL,
        ViewedUtc DATETIME2(7) NULL,
        AcceptedUtc DATETIME2(7) NULL,
        FechaCreacion DATETIME2(7) NOT NULL CONSTRAINT DF_IndorRealtorSharedQuote_Created DEFAULT SYSUTCDATETIME(),
        FechaActualizacion DATETIME2(7) NULL,
        CONSTRAINT PK_IndorRealtorSharedQuotes PRIMARY KEY CLUSTERED (Id)
    );
    CREATE INDEX IX_IndorRealtorSharedQuote_Realtor ON dbo.IndorRealtorSharedQuotes(RealtorId);
    CREATE INDEX IX_IndorRealtorSharedQuote_Quote ON dbo.IndorRealtorSharedQuotes(QuoteId, BidId);
    CREATE UNIQUE INDEX UX_IndorRealtorSharedQuote_Token ON dbo.IndorRealtorSharedQuotes(ShareToken);
    PRINT 'Table IndorRealtorSharedQuotes created.';
END
