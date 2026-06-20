/*
  INDOR — Realtor notification preference columns.
  Run on IndorDB after CreateRealtorRegistrationTables.sql.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorRealtors', N'NotifyEmailAlerts') IS NULL
    ALTER TABLE dbo.IndorRealtors
        ADD NotifyEmailAlerts BIT NOT NULL
            CONSTRAINT DF_IndorRealtors_NotifyEmailAlerts DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'NotifyQuoteUpdates') IS NULL
    ALTER TABLE dbo.IndorRealtors
        ADD NotifyQuoteUpdates BIT NOT NULL
            CONSTRAINT DF_IndorRealtors_NotifyQuoteUpdates DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'NotifyReportNotifications') IS NULL
    ALTER TABLE dbo.IndorRealtors
        ADD NotifyReportNotifications BIT NOT NULL
            CONSTRAINT DF_IndorRealtors_NotifyReportNotifications DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'NotifyPackageViewAlerts') IS NULL
    ALTER TABLE dbo.IndorRealtors
        ADD NotifyPackageViewAlerts BIT NOT NULL
            CONSTRAINT DF_IndorRealtors_NotifyPackageViewAlerts DEFAULT (0);
GO

PRINT 'Realtor notification preference columns are ready.';
GO
