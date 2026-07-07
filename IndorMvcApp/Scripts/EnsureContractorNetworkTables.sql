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
        FechaActualizacion DATETIME2 NULL
    );
    PRINT 'Table IndorProveedorNetworkJobs created.';
END
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
