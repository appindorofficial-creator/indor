/*
  INDOR — Pressure Washing flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'PressureWashingDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'pressure-washing';

PRINT 'Pressure Washing catalog link updated.';
GO
