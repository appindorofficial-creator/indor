/*
  INDOR — Sewer Backup flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'SewerBackupDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'sewer-backup';

PRINT 'Sewer Backup catalog link updated.';
GO
