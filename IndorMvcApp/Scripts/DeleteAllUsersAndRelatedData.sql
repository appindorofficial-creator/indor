/*
  DeleteAllUsersAndRelatedData.sql
  --------------------------------
  Elimina TODOS los usuarios (AspNetUsers) y los datos asociados:
    - Propiedades y tablas con PropiedadId
    - Solicitudes, archivos adjuntos, emergencias, etc.
    - Registro de proveedores (IndorProveedores y tablas hijas)
    - Pagos, membresías, mensajes, programaciones, etc.

  NO elimina catálogo:
    - Microservicios, Inspecciones, planes, landings, HomeCarePriorities
    - IndorProveedorCategoriasCatalogo, OfertasCatalogo, ExamPreguntas, AlcanceReglas
    - AspNetRoles (roles del sistema, ej. ProveedorServicios)

  ADVERTENCIA: irreversible. Usar solo en desarrollo/pruebas. Hacer backup antes.

  Ejecutar en SSMS / Azure Data Studio contra IndorDB.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @sql NVARCHAR(MAX);
    DECLARE @tbl NVARCHAR(300);
    DECLARE @cnt INT;

    PRINT '=== INDOR: eliminar usuarios y datos asociados ===';
    PRINT '';

    /* ------------------------------------------------------------------
       1) Archivos adjuntos (hijos de solicitudes)
       ------------------------------------------------------------------ */
    PRINT '-- Archivos --';
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
        PRINT CONCAT('  ', @tbl, ': ', @cnt);
        FETCH NEXT FROM archivos INTO @tbl;
    END
    CLOSE archivos;
    DEALLOCATE archivos;

    /* ------------------------------------------------------------------
       2) Hijos de solicitudes
       ------------------------------------------------------------------ */
    IF OBJECT_ID(N'dbo.UtilitiesSetupContactos', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.UtilitiesSetupContactos;
        PRINT CONCAT('  dbo.UtilitiesSetupContactos: ', @@ROWCOUNT);
    END

    /* ------------------------------------------------------------------
       3) Solicitudes (todas las tablas Solicitud*)
       ------------------------------------------------------------------ */
    PRINT '-- Solicitudes --';
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
        PRINT CONCAT('  ', @tbl, ': ', @cnt);
        FETCH NEXT FROM solicitudes INTO @tbl;
    END
    CLOSE solicitudes;
    DEALLOCATE solicitudes;

    /* ------------------------------------------------------------------
       4) Tablas con PropiedadId (antes de Propiedades)
       ------------------------------------------------------------------ */
    PRINT '-- Datos de propiedad --';
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
            PRINT CONCAT('  ', @tbl, ': ', @cnt);
        FETCH NEXT FROM propRefs INTO @tbl;
    END
    CLOSE propRefs;
    DEALLOCATE propRefs;

    IF OBJECT_ID(N'dbo.Propiedades', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.Propiedades;
        PRINT CONCAT('  dbo.Propiedades: ', @@ROWCOUNT);
        DBCC CHECKIDENT (N'dbo.Propiedades', RESEED, 0);
    END

    /* ------------------------------------------------------------------
       5) Portal de proveedores (registros por usuario, no catálogo)
       ------------------------------------------------------------------ */
    PRINT '-- Proveedores (registro) --';
    IF OBJECT_ID(N'dbo.IndorProveedorExamRespuestas', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.IndorProveedorExamRespuestas; PRINT CONCAT('  IndorProveedorExamRespuestas: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.IndorProveedorDocumentos', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.IndorProveedorDocumentos; PRINT CONCAT('  IndorProveedorDocumentos: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.IndorProveedorCategoriasSel', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.IndorProveedorCategoriasSel; PRINT CONCAT('  IndorProveedorCategoriasSel: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.IndorProveedorOfertasSel', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.IndorProveedorOfertasSel; PRINT CONCAT('  IndorProveedorOfertasSel: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.IndorProveedores', N'U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.IndorProveedores;
        PRINT CONCAT('  IndorProveedores: ', @@ROWCOUNT);
        DBCC CHECKIDENT (N'dbo.IndorProveedores', RESEED, 0);
    END

    /* ------------------------------------------------------------------
       6) Cuenta / actividad del usuario
       ------------------------------------------------------------------ */
    PRINT '-- Cuenta usuario --';
    IF OBJECT_ID(N'dbo.Pagos', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.Pagos; PRINT CONCAT('  Pagos: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.MetodosPago', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.MetodosPago; PRINT CONCAT('  MetodosPago: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.MembresiasUsuario', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.MembresiasUsuario; PRINT CONCAT('  MembresiasUsuario: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.HistorialServicios', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.HistorialServicios; PRINT CONCAT('  HistorialServicios: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.MensajesSoporte', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.MensajesSoporte; PRINT CONCAT('  MensajesSoporte: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.ProgramacionesMicroservicio', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.ProgramacionesMicroservicio; PRINT CONCAT('  ProgramacionesMicroservicio: ', @@ROWCOUNT); END

    /* Otras tablas con UserId no cubiertas arriba */
    PRINT '-- Otras tablas UserId --';
    DECLARE extra CURSOR LOCAL FAST_FORWARD FOR
        SELECT QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
        FROM sys.tables t
        INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.name = N'UserId'
        WHERE t.name NOT LIKE N'AspNet%'
          AND t.name NOT LIKE N'Solicitud%'
          AND t.name NOT LIKE N'Solicitudes%'
          AND t.name NOT LIKE N'Archivo%'
          AND t.name NOT LIKE N'Archivos%'
          AND t.name NOT LIKE N'IndorProveedor%'
          AND t.name NOT IN (
              N'Propiedades',
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
            PRINT CONCAT('  ', @tbl, ': ', @cnt);
        FETCH NEXT FROM extra INTO @tbl;
    END
    CLOSE extra;
    DEALLOCATE extra;

    /* ------------------------------------------------------------------
       7) ASP.NET Identity
       ------------------------------------------------------------------ */
    PRINT '-- Identity --';
    IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.AspNetUserTokens; PRINT CONCAT('  AspNetUserTokens: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.AspNetUserLogins; PRINT CONCAT('  AspNetUserLogins: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.AspNetUserClaims; PRINT CONCAT('  AspNetUserClaims: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.AspNetUserRoles; PRINT CONCAT('  AspNetUserRoles: ', @@ROWCOUNT); END
    IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
    BEGIN DELETE FROM dbo.AspNetUsers; PRINT CONCAT('  AspNetUsers: ', @@ROWCOUNT); END

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '=== Completado: 0 usuarios, sin propiedades ni registros de proveedor. ===';
    PRINT 'Catálogo y roles (AspNetRoles) conservados.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @line INT = ERROR_LINE();
    RAISERROR(N'DeleteAllUsersAndRelatedData falló (línea %d): %s', 16, 1, @line, @msg);
END CATCH;
GO

/* Verificación rápida (opcional, descomentar):
SELECT COUNT(*) AS Users FROM dbo.AspNetUsers;
SELECT COUNT(*) AS Properties FROM dbo.Propiedades;
SELECT COUNT(*) AS Providers FROM dbo.IndorProveedores;
*/
