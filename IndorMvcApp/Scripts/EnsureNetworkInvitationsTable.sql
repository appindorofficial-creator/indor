/*
  Contractor Network — ensure the direct "Invite to Job" table exists.
  OPTIONAL: this table is also created automatically at app startup by
  ProviderDatabaseSchemaInitializer. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorProveedorNetworkInvitaciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorNetworkInvitaciones (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkInvitaciones PRIMARY KEY,
        InviterProveedorId INT NOT NULL,
        SubcontractorProveedorId INT NOT NULL,
        NetworkJobId INT NULL,
        JobTitle NVARCHAR(160) NULL,
        TradeId NVARCHAR(40) NULL,
        ServiceCategory NVARCHAR(120) NULL,
        PropertyAddress NVARCHAR(300) NULL,
        ScheduleDate DATETIME2 NULL,
        ScheduleToday BIT NOT NULL CONSTRAINT DF_IndorNetworkInv_Today DEFAULT (1),
        BudgetRange NVARCHAR(40) NULL,
        Description NVARCHAR(600) NULL,
        TimingPreference NVARCHAR(20) NULL,
        AttachmentsJson NVARCHAR(MAX) NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorNetworkInv_Status DEFAULT ('Sent'),
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkInv_Fecha DEFAULT (SYSUTCDATETIME())
    );
    PRINT 'Table IndorProveedorNetworkInvitaciones created.';
END
GO
