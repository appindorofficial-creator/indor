/*
  INDOR — Rename Home Care Guide section to Home Maintenance Planner (Services tab).
  Safe to run multiple times.
*/

IF EXISTS (SELECT 1 FROM dbo.HomeCarePrioritiesConfig)
BEGIN
    UPDATE dbo.HomeCarePrioritiesConfig
    SET Titulo = N'Home Maintenance Planner',
        Subtitulo = N'Stay ahead of important home maintenance.',
        IconoClase = N'fa-shield-halved',
        ViewAllTexto = N'View all tasks',
        ViewAllController = N'Home',
        ViewAllAction = N'HomeCareGuide'
    WHERE Titulo IN (N'Home Care Guide', N'Home Maintenance Planner');

    PRINT 'HomeCarePrioritiesConfig title set to Home Maintenance Planner.';
END
ELSE
    PRINT 'HomeCarePrioritiesConfig not found — run CreateHomeCarePrioritiesTables.sql first.';
GO
