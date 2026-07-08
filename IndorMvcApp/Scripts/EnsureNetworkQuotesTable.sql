/*
  Contractor Network — ensure the job-quotes table exists.
  OPTIONAL: this table is also created automatically at app startup by
  ProviderDatabaseSchemaInitializer. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorProveedorNetworkQuotes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorNetworkQuotes (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkQuotes PRIMARY KEY,
        NetworkJobId INT NOT NULL,
        SubcontractorProveedorId INT NOT NULL,
        AmountLow DECIMAL(10,2) NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Low DEFAULT (0),
        AmountHigh DECIMAL(10,2) NOT NULL CONSTRAINT DF_IndorNetworkQuotes_High DEFAULT (0),
        QuotedAmount DECIMAL(10,2) NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Amt DEFAULT (0),
        ResponseMinutes INT NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Resp DEFAULT (60),
        Message NVARCHAR(400) NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Status DEFAULT ('Pending'),
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Fecha DEFAULT (SYSUTCDATETIME())
    );
    PRINT 'Table IndorProveedorNetworkQuotes created.';
END
GO
