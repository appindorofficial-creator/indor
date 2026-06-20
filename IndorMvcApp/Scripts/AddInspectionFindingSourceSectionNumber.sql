/*
  INDOR — Hierarchical report index per AI inspection finding (e.g. 1.1.1, 3.1.2).
  Run on IndorDB. Safe to run multiple times.
*/

IF COL_LENGTH('dbo.IndorRealtorInspectionUploadFindings', 'SourceSectionNumber') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorInspectionUploadFindings ADD SourceSectionNumber NVARCHAR(30) NULL;
    PRINT 'Column IndorRealtorInspectionUploadFindings.SourceSectionNumber added.';
END
GO

PRINT 'Inspection finding source section number column ready.';
