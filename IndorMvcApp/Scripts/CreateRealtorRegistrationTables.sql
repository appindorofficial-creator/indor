/*
  INDOR — Realtor registration profile, documents, and dashboard data.
  Run on IndorDB. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorRealtors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtors (
        Id                      INT              IDENTITY(1,1) NOT NULL,
        UserId                  NVARCHAR(450)    NULL,
        RegistrationToken       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IndorRealtors_Token DEFAULT (NEWID()),
        RegistrationStatus      NVARCHAR(30)     NOT NULL CONSTRAINT DF_IndorRealtors_Status DEFAULT (N'Draft'),
        CurrentStep             INT              NOT NULL CONSTRAINT DF_IndorRealtors_Step DEFAULT (1),
        DisplayName             NVARCHAR(120)    NULL,
        Email                   NVARCHAR(256)    NULL,
        Phone                   NVARCHAR(30)     NULL,
        BrokerageName           NVARCHAR(200)    NULL,
        LicenseNumber           NVARCHAR(80)     NULL,
        LicenseState            NVARCHAR(10)     NULL,
        ServiceAreas            NVARCHAR(500)    NULL,
        ProfessionalTermsAccepted BIT            NOT NULL CONSTRAINT DF_IndorRealtors_Terms DEFAULT (0),
        TermsAcceptedUtc        DATETIME2(7)     NULL,
        VerificationSkipped     BIT              NOT NULL CONSTRAINT DF_IndorRealtors_SkipVer DEFAULT (0),
        ProfileCompletedUtc     DATETIME2(7)     NULL,
        FechaCreacion           DATETIME2(7)     NOT NULL CONSTRAINT DF_IndorRealtors_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion      DATETIME2(7)     NULL,
        CONSTRAINT PK_IndorRealtors PRIMARY KEY CLUSTERED (Id)
    );

    CREATE UNIQUE INDEX UX_IndorRealtors_Token ON dbo.IndorRealtors (RegistrationToken);
    CREATE INDEX IX_IndorRealtors_UserId ON dbo.IndorRealtors (UserId);

    ALTER TABLE dbo.IndorRealtors WITH CHECK ADD CONSTRAINT FK_IndorRealtors_AspNetUsers
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id);

    PRINT 'Table IndorRealtors created.';
END
ELSE
    PRINT 'Table IndorRealtors already exists.';
GO

IF OBJECT_ID(N'dbo.IndorRealtorDocumentos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorDocumentos (
        Id              INT            IDENTITY(1,1) NOT NULL,
        RealtorId       INT            NOT NULL,
        DocumentType    NVARCHAR(40)   NOT NULL,
        FileUrl         NVARCHAR(500)  NULL,
        UploadedUtc     DATETIME2(7)   NULL,
        CONSTRAINT PK_IndorRealtorDocumentos PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRealtorDoc_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX UX_IndorRealtorDoc_Type ON dbo.IndorRealtorDocumentos(RealtorId, DocumentType);
    PRINT 'Table IndorRealtorDocumentos created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorPropertyFiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorPropertyFiles (
        Id              INT            IDENTITY(1,1) NOT NULL,
        RealtorId       INT            NOT NULL,
        Title           NVARCHAR(150)  NOT NULL,
        Address         NVARCHAR(250)  NOT NULL,
        CityRegion      NVARCHAR(120)  NULL,
        Beds            INT            NULL,
        Baths           DECIMAL(3,1)   NULL,
        SqFt            INT            NULL,
        PhotoUrl        NVARCHAR(500)  NULL,
        Status          NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorRealtorProp_Status DEFAULT (N'Active'),
        FechaCreacion   DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorRealtorProp_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorRealtorPropertyFiles PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRealtorProp_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorRealtorProp_Realtor ON dbo.IndorRealtorPropertyFiles(RealtorId);
    PRINT 'Table IndorRealtorPropertyFiles created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorQuotes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorQuotes (
        Id              INT            IDENTITY(1,1) NOT NULL,
        RealtorId       INT            NOT NULL,
        QuoteCode       NVARCHAR(30)   NOT NULL,
        Address         NVARCHAR(250)  NOT NULL,
        ServiceType     NVARCHAR(120)  NOT NULL,
        RequestedUtc    DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorRealtorQuote_Req DEFAULT (SYSUTCDATETIME()),
        Status          NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorRealtorQuote_Status DEFAULT (N'Pending'),
        Amount          DECIMAL(12,2)  NULL,
        CONSTRAINT PK_IndorRealtorQuotes PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRealtorQuote_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorRealtorQuote_Realtor ON dbo.IndorRealtorQuotes(RealtorId);
    PRINT 'Table IndorRealtorQuotes created.';
END
GO

IF OBJECT_ID(N'dbo.IndorRealtorSharedPackages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorRealtorSharedPackages (
        Id              INT            IDENTITY(1,1) NOT NULL,
        RealtorId       INT            NOT NULL,
        ClientName      NVARCHAR(120)  NOT NULL,
        Address         NVARCHAR(250)  NOT NULL,
        SharedUtc       DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorRealtorPkg_Shared DEFAULT (SYSUTCDATETIME()),
        StatusLabel     NVARCHAR(60)   NOT NULL CONSTRAINT DF_IndorRealtorPkg_Status DEFAULT (N'Viewed by client'),
        CONSTRAINT PK_IndorRealtorSharedPackages PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorRealtorPkg_Realtor FOREIGN KEY (RealtorId) REFERENCES dbo.IndorRealtors(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorRealtorPkg_Realtor ON dbo.IndorRealtorSharedPackages(RealtorId);
    PRINT 'Table IndorRealtorSharedPackages created.';
END
GO
