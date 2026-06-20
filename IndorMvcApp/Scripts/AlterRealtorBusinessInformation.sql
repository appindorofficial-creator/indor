/*
  INDOR — Business information fields for realtor profile setup.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorRealtors', N'OfficeAddress') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD OfficeAddress NVARCHAR(500) NULL;
    PRINT 'Column IndorRealtors.OfficeAddress added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'LanguagesJson') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD LanguagesJson NVARCHAR(200) NULL;
    PRINT 'Column IndorRealtors.LanguagesJson added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'IndorMessagingEnabled') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors
        ADD IndorMessagingEnabled BIT NOT NULL
            CONSTRAINT DF_IndorRealtors_Messaging DEFAULT (1);
    PRINT 'Column IndorRealtors.IndorMessagingEnabled added.';
END
GO

PRINT 'Realtor business information columns ready.';
