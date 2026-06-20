-- Findings should not be pre-selected; realtors choose issues while reviewing.
IF EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.IndorRealtorInspectionUploadFindings')
      AND name = N'DF_IndorInspFinding_Selected')
BEGIN
    ALTER TABLE dbo.IndorRealtorInspectionUploadFindings
        DROP CONSTRAINT DF_IndorInspFinding_Selected;
END
GO

ALTER TABLE dbo.IndorRealtorInspectionUploadFindings
    ADD CONSTRAINT DF_IndorInspFinding_Selected DEFAULT (0) FOR IsSelected;
GO

-- Clear pre-selected state for drafts still on the findings step.
UPDATE f
SET f.IsSelected = 0
FROM dbo.IndorRealtorInspectionUploadFindings AS f
INNER JOIN dbo.IndorRealtorInspectionUploadDrafts AS d ON d.Id = f.DraftId
WHERE d.CurrentStep = 3
  AND f.IsSelected = 1;
GO
