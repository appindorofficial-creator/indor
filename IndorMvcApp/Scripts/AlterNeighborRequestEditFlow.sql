/*
  INDOR — Edit request flow (details summary, preferred time window).
  Run after AlterNeighborRequestWizard.sql. Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'DetailsSummary') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD DetailsSummary NVARCHAR(200) NULL;
    PRINT 'Column IndorNeighborRequests.DetailsSummary added.';
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'TimeWindowStart') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD TimeWindowStart TIME(0) NULL;
    PRINT 'Column IndorNeighborRequests.TimeWindowStart added.';
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'TimeWindowEnd') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD TimeWindowEnd TIME(0) NULL;
    PRINT 'Column IndorNeighborRequests.TimeWindowEnd added.';
END
GO

UPDATE dbo.IndorNeighborRequestCategories
SET LabelEn = N'House cleaning'
WHERE Code = N'cleaning' AND LabelEn = N'Cleaning';
GO
