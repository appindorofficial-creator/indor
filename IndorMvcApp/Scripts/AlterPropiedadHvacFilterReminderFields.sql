/*
  HVAC filter replacement reminder fields on PropiedadHvacSistemas.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.PropiedadHvacSistemas', N'HasPets') IS NULL
    ALTER TABLE dbo.PropiedadHvacSistemas ADD [HasPets] [bit] NULL;
GO

IF COL_LENGTH(N'dbo.PropiedadHvacSistemas', N'FilterScheduleMode') IS NULL
    ALTER TABLE dbo.PropiedadHvacSistemas ADD [FilterScheduleMode] [nvarchar](20) NULL;
GO

IF COL_LENGTH(N'dbo.PropiedadHvacSistemas', N'NextFilterChangeDate') IS NULL
    ALTER TABLE dbo.PropiedadHvacSistemas ADD [NextFilterChangeDate] [datetime2](7) NULL;
GO

IF COL_LENGTH(N'dbo.PropiedadHvacSistemas', N'RemindOneWeekBefore') IS NULL
    ALTER TABLE dbo.PropiedadHvacSistemas ADD [RemindOneWeekBefore] [bit] NOT NULL
        CONSTRAINT [DF_PropiedadHvacSistemas_RemindOneWeekBefore] DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.PropiedadHvacSistemas', N'RemindOneDayBefore') IS NULL
    ALTER TABLE dbo.PropiedadHvacSistemas ADD [RemindOneDayBefore] [bit] NOT NULL
        CONSTRAINT [DF_PropiedadHvacSistemas_RemindOneDayBefore] DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.PropiedadHvacSistemas', N'FilterReminderSetupComplete') IS NULL
    ALTER TABLE dbo.PropiedadHvacSistemas ADD [FilterReminderSetupComplete] [bit] NOT NULL
        CONSTRAINT [DF_PropiedadHvacSistemas_FilterReminderSetupComplete] DEFAULT (0);
GO

IF COL_LENGTH(N'dbo.PropiedadHvacSistemas', N'FilterNotificationsConsent') IS NULL
    ALTER TABLE dbo.PropiedadHvacSistemas ADD [FilterNotificationsConsent] [bit] NOT NULL
        CONSTRAINT [DF_PropiedadHvacSistemas_FilterNotificationsConsent] DEFAULT (0);
GO

PRINT 'PropiedadHvacSistemas filter reminder fields ready.';
GO
