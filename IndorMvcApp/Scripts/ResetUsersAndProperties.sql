/*
  ResetUsersAndProperties.sql
  ---------------------------------
  Borra TODOS los usuarios, propiedades y datos transaccionales ligados a ellos.

  NO borra catálogo (Microservicios, Inspecciones, HomeCarePriorities, landings, etc.).

  Ejecutar en SSMS contra la base de datos de INDOR.
  ADVERTENCIA: irreversible. Haz backup si lo necesitas.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @sql   NVARCHAR(MAX);
    DECLARE @tbl   NVARCHAR(300);
    DECLARE @cnt   INT;

    PRINT '=== INDOR reset: usuarios y propiedades ===';
    PRINT '';

    /* ------------------------------------------------------------------
       1) Archivos adjuntos (hijos de solicitudes)
       ------------------------------------------------------------------ */
    DECLARE archivos CURSOR LOCAL FAST_FORWARD FOR
        SELECT QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
        FROM sys.tables t
        WHERE t.name LIKE N'Archivo%' OR t.name LIKE N'Archivos%'
        ORDER BY t.name;

    OPEN archivos;
    FETCH NEXT FROM archivos INTO @tbl;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'DELETE FROM ' + @tbl + N'; SET @c = @@ROWCOUNT;';
        EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
        PRINT CONCAT('  ', @tbl, ': ', @cnt, ' fila(s)');
        FETCH NEXT FROM archivos INTO @tbl;
    END
    CLOSE archivos;
    DEALLOCATE archivos;

    /* ------------------------------------------------------------------
       2) Contactos / hijos de solicitudes (si existen)
       ------------------------------------------------------------------ */
    IF OBJECT_ID(N'dbo.UtilitiesSetupContactos', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.UtilitiesSetupContactos;
        PRINT CONCAT('  dbo.UtilitiesSetupContactos: ', @@ROWCOUNT, ' fila(s)');
    END

    /* ------------------------------------------------------------------
       3) Todas las solicitudes de servicio / inspección / emergencia
       ------------------------------------------------------------------ */
    DECLARE solicitudes CURSOR LOCAL FAST_FORWARD FOR
        SELECT QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
        FROM sys.tables t
        WHERE t.name LIKE N'Solicitud%' OR t.name LIKE N'Solicitudes%'
        ORDER BY t.name;

    OPEN solicitudes;
    FETCH NEXT FROM solicitudes INTO @tbl;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'DELETE FROM ' + @tbl + N'; SET @c = @@ROWCOUNT;';
        EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
        PRINT CONCAT('  ', @tbl, ': ', @cnt, ' fila(s)');
        FETCH NEXT FROM solicitudes INTO @tbl;
    END
    CLOSE solicitudes;
    DEALLOCATE solicitudes;

    /* ------------------------------------------------------------------
       4) Cualquier tabla con PropiedadId (antes de borrar Propiedades)
       ------------------------------------------------------------------ */
    DECLARE propRefs CURSOR LOCAL FAST_FORWARD FOR
        SELECT QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
        FROM sys.tables t
        INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.name = N'PropiedadId'
        WHERE t.name <> N'Propiedades'
        ORDER BY t.name;

    OPEN propRefs;
    FETCH NEXT FROM propRefs INTO @tbl;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'DELETE FROM ' + @tbl + N'; SET @c = @@ROWCOUNT;';
        EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
        IF @cnt > 0
            PRINT CONCAT('  ', @tbl, ': ', @cnt, ' fila(s)');
        FETCH NEXT FROM propRefs INTO @tbl;
    END
    CLOSE propRefs;
    DEALLOCATE propRefs;

    IF OBJECT_ID(N'dbo.Propiedades', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.Propiedades;
        PRINT CONCAT('  dbo.Propiedades: ', @@ROWCOUNT, ' fila(s)');
        DBCC CHECKIDENT (N'dbo.Propiedades', RESEED, 0);
        PRINT '  dbo.Propiedades: identity reseeded to 0';
    END

    /* ------------------------------------------------------------------
       5) Pagos y datos de cuenta del usuario
       ------------------------------------------------------------------ */
    IF OBJECT_ID(N'dbo.Pagos', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.Pagos; PRINT CONCAT('  dbo.Pagos: ', @@ROWCOUNT, ' fila(s)'); END

    IF OBJECT_ID(N'dbo.MetodosPago', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.MetodosPago; PRINT CONCAT('  dbo.MetodosPago: ', @@ROWCOUNT, ' fila(s)'); END

    IF OBJECT_ID(N'dbo.MembresiasUsuario', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.MembresiasUsuario; PRINT CONCAT('  dbo.MembresiasUsuario: ', @@ROWCOUNT, ' fila(s)'); END

    IF OBJECT_ID(N'dbo.HistorialServicios', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.HistorialServicios; PRINT CONCAT('  dbo.HistorialServicios: ', @@ROWCOUNT, ' fila(s)'); END

    IF OBJECT_ID(N'dbo.MensajesSoporte', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.MensajesSoporte; PRINT CONCAT('  dbo.MensajesSoporte: ', @@ROWCOUNT, ' fila(s)'); END

    /* ProgramacionesMicroservicio ya se borró en el paso 4 (PropiedadId) */

    /* ------------------------------------------------------------------
       6) Cualquier otra tabla con UserId (excepto AspNet e identidad)
       ------------------------------------------------------------------ */
    DECLARE extra CURSOR LOCAL FAST_FORWARD FOR
        SELECT QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
        FROM sys.tables t
        INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.name = N'UserId'
        WHERE t.name NOT LIKE N'AspNet%'
          AND t.name NOT LIKE N'Solicitud%'
          AND t.name NOT LIKE N'Solicitudes%'
          AND t.name NOT LIKE N'Archivo%'
          AND t.name NOT LIKE N'Archivos%'
          AND t.name NOT IN (
              N'Propiedades', N'PropiedadHistorial', N'PropiedadMantenimiento', N'PropiedadHvacSistemas',
              N'PropiedadDocumentos', N'PropiedadProveedores',
              N'Pagos', N'MetodosPago', N'MembresiasUsuario',
              N'HistorialServicios', N'MensajesSoporte', N'ProgramacionesMicroservicio',
              N'UtilitiesSetupContactos'
          )
        ORDER BY t.name;

    OPEN extra;
    FETCH NEXT FROM extra INTO @tbl;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'DELETE FROM ' + @tbl + N'; SET @c = @@ROWCOUNT;';
        EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
        IF @cnt > 0
            PRINT CONCAT('  ', @tbl, ': ', @cnt, ' fila(s)');
        FETCH NEXT FROM extra INTO @tbl;
    END
    CLOSE extra;
    DEALLOCATE extra;

    /* ------------------------------------------------------------------
       7) ASP.NET Identity — usuarios
       ------------------------------------------------------------------ */
    IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.AspNetUserTokens; PRINT CONCAT('  dbo.AspNetUserTokens: ', @@ROWCOUNT, ' fila(s)'); END

    IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.AspNetUserLogins; PRINT CONCAT('  dbo.AspNetUserLogins: ', @@ROWCOUNT, ' fila(s)'); END

    IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.AspNetUserClaims; PRINT CONCAT('  dbo.AspNetUserClaims: ', @@ROWCOUNT, ' fila(s)'); END

    IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.AspNetUserRoles; PRINT CONCAT('  dbo.AspNetUserRoles: ', @@ROWCOUNT, ' fila(s)'); END

    IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.AspNetUsers;
        PRINT CONCAT('  dbo.AspNetUsers: ', @@ROWCOUNT, ' fila(s)');
    END

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '=== Reset completado. Usuarios y propiedades eliminados. ===';
    PRINT 'Catálogo (servicios, inspecciones, landings) conservado.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @line INT = ERROR_LINE();
    RAISERROR(N'ResetUsersAndProperties falló (línea %d): %s', 16, 1, @line, @msg);
END CATCH;
GO
