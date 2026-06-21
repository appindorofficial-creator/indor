/*
  INDOR — Neighbor request offer display fields.
  Run after AlterNeighborRequestWizard.sql. Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorNeighborRequestOffers', N'Rating') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequestOffers ADD Rating DECIMAL(3,1) NULL;
    PRINT 'Column IndorNeighborRequestOffers.Rating added.';
END
GO

IF COL_LENGTH(N'dbo.IndorNeighborRequestOffers', N'ScheduleLabel') IS NULL
BEGIN
    ALTER TABLE dbo.IndorNeighborRequestOffers ADD ScheduleLabel NVARCHAR(80) NULL;
    PRINT 'Column IndorNeighborRequestOffers.ScheduleLabel added.';
END
GO
