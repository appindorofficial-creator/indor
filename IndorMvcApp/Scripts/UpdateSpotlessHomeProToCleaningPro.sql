/*
  Rename microservice Spotless Home Pro -> Cleaning Pro.
  Safe to run multiple times.
*/

UPDATE dbo.Microservicios
SET Nombre = N'Cleaning Pro'
WHERE Id = 4
   OR Nombre IN (N'Spotless Home Pro', N'Hogar Impecable Pro');

IF @@ROWCOUNT > 0
BEGIN
    PRINT 'Microservicio renamed to Cleaning Pro.';
END
ELSE IF EXISTS (SELECT 1 FROM dbo.Microservicios WHERE Id = 4 AND Nombre = N'Cleaning Pro')
BEGIN
    PRINT 'Microservicio Id 4 is already named Cleaning Pro.';
END
ELSE
BEGIN
    PRINT 'Microservicio Id 4 not found. No rows updated.';
END
GO
