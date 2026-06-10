/*
  INDOR — Extend IndorRealtorInvitations for Invite Client wizard (4 steps).
  Run after ExtendRealtorPortalTables.sql. Safe to run multiple times.
*/

IF COL_LENGTH('dbo.IndorRealtorInvitations', 'Phone') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD Phone NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'ClientRole') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD ClientRole NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'QuickNote') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD QuickNote NVARCHAR(250) NULL;
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'CurrentStep') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD CurrentStep INT NOT NULL CONSTRAINT DF_IndorRealtorInv_Step DEFAULT (1);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'InvitationToken') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD InvitationToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IndorRealtorInv_Token DEFAULT (NEWID());
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'PropertyFileId') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD PropertyFileId INT NULL;
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'PropertyAddress') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD PropertyAddress NVARCHAR(250) NULL;
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'PropertyLabel') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD PropertyLabel NVARCHAR(80) NULL;
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'PropertyCityRegion') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD PropertyCityRegion NVARCHAR(120) NULL;
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'PropertyStatusLabel') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD PropertyStatusLabel NVARCHAR(40) NULL;
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'AccessPropertyOverview') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD AccessPropertyOverview BIT NOT NULL CONSTRAINT DF_IndorRealtorInv_AccOverview DEFAULT (1);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'AccessFilesReports') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD AccessFilesReports BIT NOT NULL CONSTRAINT DF_IndorRealtorInv_AccFiles DEFAULT (1);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'AccessQuotesEstimates') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD AccessQuotesEstimates BIT NOT NULL CONSTRAINT DF_IndorRealtorInv_AccQuotes DEFAULT (1);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'AccessMessages') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD AccessMessages BIT NOT NULL CONSTRAINT DF_IndorRealtorInv_AccMsg DEFAULT (1);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'AccessProjectUpdates') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD AccessProjectUpdates BIT NOT NULL CONSTRAINT DF_IndorRealtorInv_AccUpdates DEFAULT (1);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'AccessPayments') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD AccessPayments BIT NOT NULL CONSTRAINT DF_IndorRealtorInv_AccPay DEFAULT (0);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'CollaborationLevel') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD CollaborationLevel NVARCHAR(30) NOT NULL CONSTRAINT DF_IndorRealtorInv_Collab DEFAULT (N'CanComment');
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'DeliveryEmail') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD DeliveryEmail BIT NOT NULL CONSTRAINT DF_IndorRealtorInv_DelEmail DEFAULT (1);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'DeliveryText') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD DeliveryText BIT NOT NULL CONSTRAINT DF_IndorRealtorInv_DelText DEFAULT (0);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'WelcomeMessage') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD WelcomeMessage NVARCHAR(250) NULL;
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'SendReminder48h') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD SendReminder48h BIT NOT NULL CONSTRAINT DF_IndorRealtorInv_Reminder DEFAULT (1);
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'FechaCreacion') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD FechaCreacion DATETIME2(7) NOT NULL CONSTRAINT DF_IndorRealtorInv_Created DEFAULT (SYSUTCDATETIME());
IF COL_LENGTH('dbo.IndorRealtorInvitations', 'FechaActualizacion') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD FechaActualizacion DATETIME2(7) NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_IndorRealtorInv_PropertyFile'
)
BEGIN
    ALTER TABLE dbo.IndorRealtorInvitations WITH CHECK ADD CONSTRAINT FK_IndorRealtorInv_PropertyFile
        FOREIGN KEY (PropertyFileId) REFERENCES dbo.IndorRealtorPropertyFiles(Id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_IndorRealtorInv_Token')
    CREATE UNIQUE INDEX UX_IndorRealtorInv_Token ON dbo.IndorRealtorInvitations(InvitationToken);
GO

PRINT 'IndorRealtorInvitations extended for Invite Client wizard.';
