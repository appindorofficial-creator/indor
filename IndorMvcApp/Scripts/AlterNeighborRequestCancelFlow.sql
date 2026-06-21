/*
  INDOR — Cancel request flow (reason, note, cancelled timestamp).
  Run after AlterNeighborRequestEditFlow.sql. Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'CancelReasonCode') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD CancelReasonCode NVARCHAR(40) NULL;
    PRINT 'Column IndorNeighborRequests.CancelReasonCode added.';
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'CancelNote') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD CancelNote NVARCHAR(500) NULL;
    PRINT 'Column IndorNeighborRequests.CancelNote added.';
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequests', N'CancelledUtc') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequests ADD CancelledUtc DATETIME2(7) NULL;
    PRINT 'Column IndorNeighborRequests.CancelledUtc added.';
END
GO

UPDATE dbo.IndorNeighborRequests
SET CancelledUtc = UpdatedUtc
WHERE Status = N'Cancelled' AND CancelledUtc IS NULL AND UpdatedUtc IS NOT NULL;
GO
