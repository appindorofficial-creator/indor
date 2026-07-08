/*
  Contractor Verification console — ensure the operator review table exists.
  OPTIONAL: this table is also created automatically at app startup by
  ProviderDatabaseSchemaInitializer. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorProveedorVerificaciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorVerificaciones (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorVerificaciones PRIMARY KEY,
        ProveedorId INT NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorVerificaciones_Status DEFAULT ('Pending'),
        LicenseVerified BIT NOT NULL CONSTRAINT DF_IndorVerificaciones_Lic DEFAULT (0),
        LicenseExpiry DATETIME2 NULL,
        InsuranceVerified BIT NOT NULL CONSTRAINT DF_IndorVerificaciones_Ins DEFAULT (0),
        InsuranceExpiry DATETIME2 NULL,
        W9Verified BIT NOT NULL CONSTRAINT DF_IndorVerificaciones_W9 DEFAULT (0),
        BackgroundStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorVerificaciones_Bg DEFAULT ('Pending'),
        OperatorNotes NVARCHAR(600) NULL,
        FollowUpNote NVARCHAR(300) NULL,
        ReviewerName NVARCHAR(160) NULL,
        ApprovedUtc DATETIME2 NULL,
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorVerificaciones_Fecha DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion DATETIME2 NULL,
        CONSTRAINT UQ_IndorVerificaciones_Proveedor UNIQUE (ProveedorId)
    );
    PRINT 'Table IndorProveedorVerificaciones created.';
END
GO
