/*
  Bug 71 — Align Roof inspection Home Care card frequency with in-flow messaging.
  Card subtitle and form both use: professional inspection every 1–2 years.

  Safe to run multiple times.
*/

IF EXISTS (SELECT 1 FROM dbo.HomeCarePriorities WHERE Nombre = N'Roof inspection')
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET Subtitulo = N'Every 1–2 years'
    WHERE Nombre = N'Roof inspection';

    PRINT 'HomeCarePriorities "Roof inspection" subtitle set to Every 1–2 years.';
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "Roof inspection" not found — run CreateHomeCarePrioritiesTables.sql first.';
END
GO

IF EXISTS (SELECT 1 FROM dbo.RoofInspectionServicioLanding)
BEGIN
    UPDATE l
    SET RecomendacionItems = N'Visual roof check: spring & fall|Professional inspection: every 1–2 years|After major storms: inspect again|Older roof or active issues: inspect sooner'
    FROM dbo.RoofInspectionServicioLanding l
    INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
    WHERE p.Nombre = N'Roof inspection'
      AND (l.RecomendacionItems IS NULL OR l.RecomendacionItems LIKE N'%2–3%' OR l.RecomendacionItems NOT LIKE N'%1–2%');

    PRINT 'RoofInspectionServicioLanding recommendations synced to every 1–2 years.';
END
GO
