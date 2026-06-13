/*
  INDOR — Report section reference per AI inspection finding.
  Run on IndorDB. Safe to run multiple times.
*/

IF COL_LENGTH('dbo.IndorRealtorInspectionUploadFindings', 'SourceSection') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorInspectionUploadFindings ADD SourceSection NVARCHAR(120) NULL;
    PRINT 'Column IndorRealtorInspectionUploadFindings.SourceSection added.';
END
GO

PRINT 'Inspection finding source section column ready.';
