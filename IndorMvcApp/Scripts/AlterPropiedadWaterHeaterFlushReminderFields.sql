/*
  Annual water heater flush reminder fields on PropiedadWaterHeaterSistemas.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.PropiedadWaterHeaterSistemas', N'NextFlushDate') IS NULL
    ALTER TABLE dbo.PropiedadWaterHeaterSistemas ADD [NextFlushDate] [datetime2](7) NULL;
GO

IF COL_LENGTH(N'dbo.PropiedadWaterHeaterSistemas', N'FlushLocation') IS NULL
    ALTER TABLE dbo.PropiedadWaterHeaterSistemas ADD [FlushLocation] [nvarchar](80) NULL;
GO

IF COL_LENGTH(N'dbo.PropiedadWaterHeaterSistemas', N'RemindOneWeekBefore') IS NULL
    ALTER TABLE dbo.PropiedadWaterHeaterSistemas ADD [RemindOneWeekBefore] [bit] NOT NULL
        CONSTRAINT [DF_PropiedadWaterHeaterSistemas_RemindOneWeekBefore] DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.PropiedadWaterHeaterSistemas', N'RemindOneDayBefore') IS NULL
    ALTER TABLE dbo.PropiedadWaterHeaterSistemas ADD [RemindOneDayBefore] [bit] NOT NULL
        CONSTRAINT [DF_PropiedadWaterHeaterSistemas_RemindOneDayBefore] DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.PropiedadWaterHeaterSistemas', N'AutoRepeatEnabled') IS NULL
    ALTER TABLE dbo.PropiedadWaterHeaterSistemas ADD [AutoRepeatEnabled] [bit] NOT NULL
        CONSTRAINT [DF_PropiedadWaterHeaterSistemas_AutoRepeatEnabled] DEFAULT (1);
GO

IF COL_LENGTH(N'dbo.PropiedadWaterHeaterSistemas', N'FlushReminderSetupComplete') IS NULL
    ALTER TABLE dbo.PropiedadWaterHeaterSistemas ADD [FlushReminderSetupComplete] [bit] NOT NULL
        CONSTRAINT [DF_PropiedadWaterHeaterSistemas_FlushReminderSetupComplete] DEFAULT (0);
GO

IF COL_LENGTH(N'dbo.PropiedadWaterHeaterSistemas', N'FlushNotificationsConsent') IS NULL
    ALTER TABLE dbo.PropiedadWaterHeaterSistemas ADD [FlushNotificationsConsent] [bit] NOT NULL
        CONSTRAINT [DF_PropiedadWaterHeaterSistemas_FlushNotificationsConsent] DEFAULT (0);
GO

PRINT 'PropiedadWaterHeaterSistemas flush reminder fields ready.';
GO
