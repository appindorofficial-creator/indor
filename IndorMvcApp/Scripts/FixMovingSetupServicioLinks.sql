/*
  FixMovingSetupServicioLinks.sql
  -------------------------------
  Points Moving Setup grid items to their real booking flows (not MovingSetup/Index).
  Also updates the featured CTA to start the Moving flow.

  Safe to run multiple times.
*/

USE [IndorDB];
GO

DECLARE @MovingId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Moving' ORDER BY Id);
DECLARE @CleaningId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Cleaning' ORDER BY Id);
DECLARE @PackingId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Packing Help' ORDER BY Id);
DECLARE @FurnitureId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Furniture & Assembly' ORDER BY Id);
DECLARE @TvId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'TV Wall Mounting' ORDER BY Id);
DECLARE @UtilitiesId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Utilities Setup' ORDER BY Id);
DECLARE @GeneralHelpId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'General Help' ORDER BY Id);

IF @MovingId IS NOT NULL
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'Moving', LinkAction = N'MovingService', LinkRouteId = Id
    WHERE Id = @MovingId;

IF @CleaningId IS NOT NULL
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'Cleaning', LinkAction = N'CleaningService', LinkRouteId = Id
    WHERE Id = @CleaningId;

IF @PackingId IS NOT NULL
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'Packing', LinkAction = N'PackingService', LinkRouteId = Id
    WHERE Id = @PackingId;

IF @FurnitureId IS NOT NULL
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'FurnitureAssembly', LinkAction = N'FurnitureAssemblyService', LinkRouteId = Id
    WHERE Id = @FurnitureId;

IF @TvId IS NOT NULL
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'TvWallMounting', LinkAction = N'TvWallMountingService', LinkRouteId = Id
    WHERE Id = @TvId;

IF @UtilitiesId IS NOT NULL
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'UtilitiesSetup', LinkAction = N'UtilitiesSetupAddress', LinkRouteId = Id
    WHERE Id = @UtilitiesId;

IF @GeneralHelpId IS NOT NULL
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'GeneralHelp', LinkAction = N'GeneralHelpRequest', LinkRouteId = Id
    WHERE Id = @GeneralHelpId;

IF @MovingId IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.MovingSetupConfig)
BEGIN
    UPDATE dbo.MovingSetupConfig
    SET FeaturedCtaController = N'Moving',
        FeaturedCtaAction = N'MovingService',
        FeaturedCtaRouteId = @MovingId
    WHERE FeaturedCtaController = N'MovingSetup'
       OR FeaturedCtaController IS NULL;
END
GO

PRINT '=== MovingSetupServicios — navigation ===';
SELECT Id, Nombre, LinkController, LinkAction, LinkRouteId
FROM dbo.MovingSetupServicios
ORDER BY Orden;
GO
