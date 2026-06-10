/*
  INDOR — Urgente Quote wizard tables (Realtor fast quote flow).
  Run after CreateRealtorQuoteRequestWizardTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorRealtorUrgentQuoteDrafts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorUrgentQuoteDrafts (
        Id                      INT            IDENTITY(1,1) NOT NULL,
        RealtorId               INT            NOT NULL,
        CurrentStep             INT            NOT NULL CONSTRAINT DF_IndorUrgentQuote_Step DEFAULT (1),
        Status                  NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorUrgentQuote_Status DEFAULT (N'Draft'),
        PropertyFileId          INT            NULL,
        Address                 NVARCHAR(250)  NULL,
        CityRegion              NVARCHAR(120)  NULL,
        ClientName              NVARCHAR(120)  NULL,
        PhotoUrl                NVARCHAR(500)  NULL,
        Beds                    INT            NULL,
        Baths                   DECIMAL(3,1)   NULL,
        SqFt                    INT            NULL,
        RequestCategory         NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorUrgentQuote_Cat DEFAULT (N'NeedQuoteToday'),
        ServiceType             NVARCHAR(40)   NOT NULL CONSTRAINT DF_IndorUrgentQuote_Service DEFAULT (N'HVAC'),
        UrgencyLevel            NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorUrgentQuote_Urgency DEFAULT (N'Today'),
        QuickDescription        NVARCHAR(200)  NULL,
        RequestTypeTag          NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorUrgentQuote_Tag DEFAULT (N'NeedQuote'),
        OptionalNote            NVARCHAR(250)  NULL,
        ProviderSelectionMode   NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorUrgentQuote_ProvMode DEFAULT (N'IndorAuto'),
        SendPayload             NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorUrgentQuote_Payload DEFAULT (N'IssueOnly'),
        NotifyClient            BIT            NOT NULL CONSTRAINT DF_IndorUrgentQuote_Notify DEFAULT (0),
        FechaCreacion           DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorUrgentQuote_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion      DATETIME2(7)   NULL,
        CONSTRAINT PK_IndorRealtorUrgentQuoteDrafts PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorUrgentQuote_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorUrgentQuote_Realtor ON dbo.IndorRealtorUrgentQuoteDrafts(RealtorId);
    PRINT 'Table IndorRealtorUrgentQuoteDrafts created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorUrgentQuoteDraftPhotos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorUrgentQuoteDraftPhotos (
        Id              INT            IDENTITY(1,1) NOT NULL,
        DraftId         INT            NOT NULL,
        FileUrl         NVARCHAR(500)  NOT NULL,
        SortOrder       INT            NOT NULL CONSTRAINT DF_IndorUrgentPhoto_Sort DEFAULT (0),
        UploadedUtc     DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorUrgentPhoto_Uploaded DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorRealtorUrgentQuoteDraftPhotos PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorUrgentPhoto_Draft FOREIGN KEY (DraftId) REFERENCES dbo.IndorRealtorUrgentQuoteDrafts(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorUrgentPhoto_Draft ON dbo.IndorRealtorUrgentQuoteDraftPhotos(DraftId);
    PRINT 'Table IndorRealtorUrgentQuoteDraftPhotos created.';
END
GO

PRINT 'Urgente Quote wizard tables ready.';
