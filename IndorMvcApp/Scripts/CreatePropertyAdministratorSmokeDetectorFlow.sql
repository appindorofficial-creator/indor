/*
  INDOR — Smoke Detector Check flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'SmokeDetectorDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'smoke-detector';

PRINT 'Smoke Detector Check catalog link updated.';
GO
