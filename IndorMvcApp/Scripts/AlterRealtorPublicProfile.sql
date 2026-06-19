/*
  INDOR — Public profile fields for realtor public page.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorRealtors', N'PublicTagline') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD PublicTagline NVARCHAR(300) NULL;
    PRINT 'Column IndorRealtors.PublicTagline added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'PublicBio') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD PublicBio NVARCHAR(1000) NULL;
    PRINT 'Column IndorRealtors.PublicBio added.';
END
GO

PRINT 'Realtor public profile columns ready.';
