/* =====================================================================
   Add extra "Post Quick Job" labor categories
   ---------------------------------------------------------------------
   Adds Painting Help, Fence Help, Assembly Help and Outdoor Cleanup to
   dbo.IndorNeighborRequestCategories so the Quick Job step shows the
   full 10-category grid. Safe to run multiple times (idempotent).
   ===================================================================== */

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.IndorNeighborRequestCategories', N'U') IS NULL
BEGIN
    RAISERROR(N'Table dbo.IndorNeighborRequestCategories does not exist. Run AlterNeighborRequestWizard.sql first.', 16, 1);
    RETURN;
END;

MERGE dbo.IndorNeighborRequestCategories AS target
USING (VALUES
    (N'painting',        N'Painting Help',   N'Walls, fences and touch-ups',  N'fa-roller-coaster',     70),
    (N'fence',           N'Fence Help',      N'Cleanup, staining or repairs', N'fa-border-all',         80),
    (N'assembly',        N'Assembly Help',   N'Shelves, furniture and items', N'fa-screwdriver-wrench', 90),
    (N'outdoor-cleanup', N'Outdoor Cleanup', N'Debris, branches and hauling', N'fa-trowel',            100)
) AS source (Code, LabelEn, DescriptionEn, IconClass, SortOrder)
ON target.Code = source.Code
WHEN MATCHED THEN
    UPDATE SET
        target.LabelEn       = source.LabelEn,
        target.DescriptionEn = source.DescriptionEn,
        target.IconClass     = source.IconClass,
        target.SortOrder     = source.SortOrder,
        target.IsActive      = 1
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Code, LabelEn, DescriptionEn, IconClass, SortOrder, IsActive)
    VALUES (source.Code, source.LabelEn, source.DescriptionEn, source.IconClass, source.SortOrder, 1);

PRINT N'Quick Job labor categories added/updated.';

SELECT Id, Code, LabelEn, DescriptionEn, IconClass, SortOrder, IsActive
FROM dbo.IndorNeighborRequestCategories
ORDER BY SortOrder, Id;
