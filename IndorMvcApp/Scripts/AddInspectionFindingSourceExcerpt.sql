/*

  INDOR — Store PDF source excerpt per AI inspection finding.

  Run on IndorDB. Safe to run multiple times.

*/



IF COL_LENGTH('dbo.IndorRealtorInspectionUploadFindings', 'SourceExcerpt') IS NULL

BEGIN

    ALTER TABLE dbo.IndorRealtorInspectionUploadFindings ADD SourceExcerpt NVARCHAR(2000) NULL;

    PRINT 'Column IndorRealtorInspectionUploadFindings.SourceExcerpt added.';

END

GO



IF COL_LENGTH('dbo.IndorRealtorInspectionUploadFindings', 'SourcePage') IS NULL

BEGIN

    ALTER TABLE dbo.IndorRealtorInspectionUploadFindings ADD SourcePage INT NULL;

    PRINT 'Column IndorRealtorInspectionUploadFindings.SourcePage added.';

END

GO



PRINT 'Inspection finding source excerpt columns ready.';


