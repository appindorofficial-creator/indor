/*
  INDOR — Property Administrator notification preferences columns.
  Run on IndorDB after CreatePropertyAdministratorRegistrationTables.sql.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorPropertyAdministrators', N'NotifyPushEnabled') IS NULL
    ALTER TABLE dbo.IndorPropertyAdministrators
        ADD NotifyPushEnabled BIT NOT NULL
            CONSTRAINT DF_IndorPropAdmin_NotifyPush DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.IndorPropertyAdministrators', N'NotifyEmailEnabled') IS NULL
    ALTER TABLE dbo.IndorPropertyAdministrators
        ADD NotifyEmailEnabled BIT NOT NULL
            CONSTRAINT DF_IndorPropAdmin_NotifyEmail DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.IndorPropertyAdministrators', N'NotifySmsEnabled') IS NULL
    ALTER TABLE dbo.IndorPropertyAdministrators
        ADD NotifySmsEnabled BIT NOT NULL
            CONSTRAINT DF_IndorPropAdmin_NotifySms DEFAULT (0);
GO

IF COL_LENGTH(N'dbo.IndorPropertyAdministrators', N'NotifyPropertyUpdates') IS NULL
    ALTER TABLE dbo.IndorPropertyAdministrators
        ADD NotifyPropertyUpdates BIT NOT NULL
            CONSTRAINT DF_IndorPropAdmin_NotifyProperty DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.IndorPropertyAdministrators', N'NotifyServiceUpdates') IS NULL
    ALTER TABLE dbo.IndorPropertyAdministrators
        ADD NotifyServiceUpdates BIT NOT NULL
            CONSTRAINT DF_IndorPropAdmin_NotifyService DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.IndorPropertyAdministrators', N'NotifyTaskReminders') IS NULL
    ALTER TABLE dbo.IndorPropertyAdministrators
        ADD NotifyTaskReminders BIT NOT NULL
            CONSTRAINT DF_IndorPropAdmin_NotifyTasks DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.IndorPropertyAdministrators', N'NotifyPaymentsBilling') IS NULL
    ALTER TABLE dbo.IndorPropertyAdministrators
        ADD NotifyPaymentsBilling BIT NOT NULL
            CONSTRAINT DF_IndorPropAdmin_NotifyPayments DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.IndorPropertyAdministrators', N'QuietHoursStart') IS NULL
    ALTER TABLE dbo.IndorPropertyAdministrators
        ADD QuietHoursStart NVARCHAR(5) NOT NULL
            CONSTRAINT DF_IndorPropAdmin_QuietStart DEFAULT (N'22:00');
GO

IF COL_LENGTH(N'dbo.IndorPropertyAdministrators', N'QuietHoursEnd') IS NULL
    ALTER TABLE dbo.IndorPropertyAdministrators
        ADD QuietHoursEnd NVARCHAR(5) NOT NULL
            CONSTRAINT DF_IndorPropAdmin_QuietEnd DEFAULT (N'07:00');
GO

PRINT 'Property administrator notification preference columns are ready.';
GO
