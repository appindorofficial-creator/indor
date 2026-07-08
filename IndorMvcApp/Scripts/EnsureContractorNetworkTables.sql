/*
  Contractor Network — ensure subcontractor marketplace tables exist (Azure / IndorDB).
  OPTIONAL: these tables are also created automatically at app startup by
  ProviderDatabaseSchemaInitializer. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorProveedorNetworkJobs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorNetworkJobs (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkJobs PRIMARY KEY,
        PosterProveedorId INT NOT NULL,
        TradeId NVARCHAR(40) NULL,
        TradeLabel NVARCHAR(120) NULL,
        Description NVARCHAR(600) NULL,
        Location NVARCHAR(200) NULL,
        DateNeeded DATETIME2 NULL,
        BudgetRange NVARCHAR(40) NULL,
        PhotoUrl NVARCHAR(500) NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_IndorNetworkJobs_Status DEFAULT ('Open'),
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkJobs_Fecha DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion DATETIME2 NULL,
        JobTitle NVARCHAR(160) NULL,
        Urgency NVARCHAR(20) NULL,
        PropertyType NVARCHAR(30) NULL,
        WhoMeets NVARCHAR(30) NULL,
        QuoteType NVARCHAR(20) NULL,
        AccessNotes NVARCHAR(300) NULL,
        PhotoUrlsJson NVARCHAR(MAX) NULL,
        Latitude DECIMAL(9,6) NULL,
        Longitude DECIMAL(9,6) NULL
    );
    PRINT 'Table IndorProveedorNetworkJobs created.';
END
GO

-- Add Post-a-Job wizard columns for databases created before they shipped.
IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'JobTitle') IS NULL
    ALTER TABLE dbo.IndorProveedorNetworkJobs ADD JobTitle NVARCHAR(160) NULL;
IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'Urgency') IS NULL
    ALTER TABLE dbo.IndorProveedorNetworkJobs ADD Urgency NVARCHAR(20) NULL;
IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'PropertyType') IS NULL
    ALTER TABLE dbo.IndorProveedorNetworkJobs ADD PropertyType NVARCHAR(30) NULL;
IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'WhoMeets') IS NULL
    ALTER TABLE dbo.IndorProveedorNetworkJobs ADD WhoMeets NVARCHAR(30) NULL;
IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'QuoteType') IS NULL
    ALTER TABLE dbo.IndorProveedorNetworkJobs ADD QuoteType NVARCHAR(20) NULL;
IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'AccessNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorNetworkJobs ADD AccessNotes NVARCHAR(300) NULL;
IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'PhotoUrlsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorNetworkJobs ADD PhotoUrlsJson NVARCHAR(MAX) NULL;
IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'Latitude') IS NULL
    ALTER TABLE dbo.IndorProveedorNetworkJobs ADD Latitude DECIMAL(9,6) NULL;
IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'Longitude') IS NULL
    ALTER TABLE dbo.IndorProveedorNetworkJobs ADD Longitude DECIMAL(9,6) NULL;
GO

IF OBJECT_ID(N'dbo.IndorProveedorNetworkHires', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorNetworkHires (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkHires PRIMARY KEY,
        HirerProveedorId INT NOT NULL,
        SubcontractorProveedorId INT NOT NULL,
        NetworkJobId INT NULL,
        ProjectTitle NVARCHAR(160) NULL,
        TradeLabel NVARCHAR(120) NULL,
        BudgetRange NVARCHAR(40) NULL,
        StartDate DATETIME2 NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_IndorNetworkHires_Status DEFAULT ('Hired'),
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkHires_Fecha DEFAULT (SYSUTCDATETIME())
    );
    PRINT 'Table IndorProveedorNetworkHires created.';
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorNetworkResenas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorNetworkResenas (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkResenas PRIMARY KEY,
        SubcontractorProveedorId INT NOT NULL,
        AuthorProveedorId INT NULL,
        AuthorName NVARCHAR(120) NULL,
        Rating INT NOT NULL CONSTRAINT DF_IndorNetworkResenas_Rating DEFAULT (5),
        Comment NVARCHAR(600) NULL,
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkResenas_Fecha DEFAULT (SYSUTCDATETIME())
    );
    PRINT 'Table IndorProveedorNetworkResenas created.';
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorNetworkGuardados', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorNetworkGuardados (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkGuardados PRIMARY KEY,
        OwnerProveedorId INT NOT NULL,
        SubcontractorProveedorId INT NOT NULL,
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkGuardados_Fecha DEFAULT (SYSUTCDATETIME())
    );
    PRINT 'Table IndorProveedorNetworkGuardados created.';
END
GO
