/*
  FixHomeCareViewAllNavigation.sql
  --------------------------------
  "View all tasks" was pointing to MyHome/Maintenance (empty personal log).
  It should open Home/HomeCareGuide (full Home Care Guide task list).

  Run on Azure/local after deploy. Idempotent.
*/

SET NOCOUNT ON;

UPDATE dbo.HomeCarePrioritiesConfig
SET ViewAllController = N'Home',
    ViewAllAction = N'HomeCareGuide'
WHERE ViewAllController IS NULL
   OR ViewAllController = N'MyHome'
   OR ViewAllAction = N'Maintenance'
   OR ViewAllAction IS NULL;

PRINT '';
PRINT '=== Home Care Guide — View all link ===';
SELECT Titulo, ViewAllTexto, ViewAllController, ViewAllAction
FROM dbo.HomeCarePrioritiesConfig;

GO
