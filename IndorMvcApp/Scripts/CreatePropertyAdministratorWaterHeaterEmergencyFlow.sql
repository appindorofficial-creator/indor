/*
  INDOR — Water Heater Emergency flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'WaterHeaterDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'emergency-water-heater';

PRINT 'Water Heater Emergency catalog link updated.';
GO
