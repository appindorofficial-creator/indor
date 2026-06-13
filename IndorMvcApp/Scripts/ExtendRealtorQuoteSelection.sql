-- Run in Azure SQL to support quote selection / next steps flow
IF COL_LENGTH('dbo.IndorRealtorQuotes', 'SelectedBidId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorQuotes ADD SelectedBidId INT NULL;
    PRINT 'Column IndorRealtorQuotes.SelectedBidId added.';
END

IF COL_LENGTH('dbo.IndorRealtorQuotes', 'AcceptedUtc') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorQuotes ADD AcceptedUtc DATETIME2(7) NULL;
    PRINT 'Column IndorRealtorQuotes.AcceptedUtc added.';
END
