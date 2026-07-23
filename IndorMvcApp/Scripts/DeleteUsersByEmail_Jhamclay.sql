/*
  DeleteUsersByEmail_Jhamclay.sql
  -------------------------------
  Elimina SOLO estos usuarios y su data asociada:

    - Jhamclayz@gmail.com
    - Jhamclayinvestor@gmail.com

  Incluye (si existen):
    - AspNetUsers / roles / claims / logins / tokens
    - PasswordResetCodes
    - Propiedades y tablas con PropiedadId de esos usuarios
    - Solicitudes / archivos ligados por UserId
    - IndorProveedores (+ hijos) / IndorRealtors / IndorPropertyAdministrators
    - Provider ops (jobs, customers, reports, etc.) del proveedor
    - Cualquier otra tabla con columna UserId que apunte a esos Ids
    - Filas de clientes/proveedor cuyo CustomerEmail coincida

  NO toca catálogo (Microservicios, roles del sistema, etc.).

  ADVERTENCIA: irreversible. Hacer backup antes.
  Ejecutar en SSMS / Azure Data Studio contra IndorDB.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* ------------------------------------------------------------------
       0) Usuarios objetivo
       ------------------------------------------------------------------ */
    IF OBJECT_ID(N'tempdb..#TargetUsers') IS NOT NULL DROP TABLE #TargetUsers;
    CREATE TABLE #TargetUsers
    (
        UserId NVARCHAR(450) NOT NULL PRIMARY KEY,
        Email  NVARCHAR(256) NOT NULL
    );

    INSERT INTO #TargetUsers (UserId, Email)
    SELECT u.Id, u.Email
    FROM dbo.AspNetUsers u
    WHERE LOWER(LTRIM(RTRIM(u.Email))) IN (
        N'jhamclayz@gmail.com',
        N'jhamclayinvestor@gmail.com'
    );

    DECLARE @userCount INT = (SELECT COUNT(*) FROM #TargetUsers);

    PRINT '=== INDOR: eliminar usuarios Jhamclay* ===';
    PRINT CONCAT('Usuarios encontrados: ', @userCount);

    IF @userCount = 0
    BEGIN
        PRINT 'No hay usuarios con esos emails. Nada que borrar.';
        COMMIT TRANSACTION;
        RETURN;
    END

    SELECT UserId, Email FROM #TargetUsers;

    /* Propiedades de esos usuarios */
    IF OBJECT_ID(N'tempdb..#TargetProps') IS NOT NULL DROP TABLE #TargetProps;
    CREATE TABLE #TargetProps (PropiedadId INT NOT NULL PRIMARY KEY);

    IF OBJECT_ID(N'dbo.Propiedades', N'U') IS NOT NULL
       AND COL_LENGTH(N'dbo.Propiedades', N'UserId') IS NOT NULL
    BEGIN
        INSERT INTO #TargetProps (PropiedadId)
        SELECT p.Id
        FROM dbo.Propiedades p
        INNER JOIN #TargetUsers t ON t.UserId = p.UserId;
    END

    /* Proveedores / Realtor / PA de esos usuarios */
    IF OBJECT_ID(N'tempdb..#TargetProveedores') IS NOT NULL DROP TABLE #TargetProveedores;
    CREATE TABLE #TargetProveedores (ProveedorId INT NOT NULL PRIMARY KEY);

    IF OBJECT_ID(N'dbo.IndorProveedores', N'U') IS NOT NULL
    BEGIN
        INSERT INTO #TargetProveedores (ProveedorId)
        SELECT p.Id
        FROM dbo.IndorProveedores p
        INNER JOIN #TargetUsers t ON t.UserId = p.UserId
        WHERE p.Id IS NOT NULL;
    END

    IF OBJECT_ID(N'tempdb..#TargetRealtors') IS NOT NULL DROP TABLE #TargetRealtors;
    CREATE TABLE #TargetRealtors (RealtorId INT NOT NULL PRIMARY KEY);

    IF OBJECT_ID(N'dbo.IndorRealtors', N'U') IS NOT NULL
    BEGIN
        INSERT INTO #TargetRealtors (RealtorId)
        SELECT r.Id
        FROM dbo.IndorRealtors r
        INNER JOIN #TargetUsers t ON t.UserId = r.UserId;
    END

    IF OBJECT_ID(N'tempdb..#TargetPropAdmins') IS NOT NULL DROP TABLE #TargetPropAdmins;
    CREATE TABLE #TargetPropAdmins (PropAdminId INT NOT NULL PRIMARY KEY);

    IF OBJECT_ID(N'dbo.IndorPropertyAdministrators', N'U') IS NOT NULL
    BEGIN
        INSERT INTO #TargetPropAdmins (PropAdminId)
        SELECT a.Id
        FROM dbo.IndorPropertyAdministrators a
        INNER JOIN #TargetUsers t ON t.UserId = a.UserId;
    END

    DECLARE @sql NVARCHAR(MAX);
    DECLARE @tbl SYSNAME;
    DECLARE @schema SYSNAME;
    DECLARE @full NVARCHAR(300);
    DECLARE @cnt INT;

    /* ------------------------------------------------------------------
       1) Provider ops ligados por ProveedorId
       ------------------------------------------------------------------ */
    PRINT '-- Provider ops (ProveedorId) --';
    IF EXISTS (SELECT 1 FROM #TargetProveedores)
    BEGIN
        DECLARE providerChild CURSOR LOCAL FAST_FORWARD FOR
            SELECT s.name, t.name
            FROM sys.tables t
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.name = N'ProveedorId'
            WHERE t.name <> N'IndorProveedores'
            ORDER BY t.name;

        OPEN providerChild;
        FETCH NEXT FROM providerChild INTO @schema, @tbl;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @full = QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl);
            SET @sql = N'
                DELETE x FROM ' + @full + N' x
                INNER JOIN #TargetProveedores tp ON tp.ProveedorId = x.ProveedorId;
                SET @c = @@ROWCOUNT;';
            EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
            IF @cnt > 0 PRINT CONCAT('  ', @full, ': ', @cnt);
            FETCH NEXT FROM providerChild INTO @schema, @tbl;
        END
        CLOSE providerChild;
        DEALLOCATE providerChild;

        /* Hijos típicos por nombre si no usan ProveedorId */
        IF OBJECT_ID(N'dbo.IndorProveedorExamRespuestas', N'U') IS NOT NULL
        BEGIN
            DELETE x FROM dbo.IndorProveedorExamRespuestas x
            INNER JOIN #TargetProveedores tp ON tp.ProveedorId = x.ProveedorId;
            PRINT CONCAT('  IndorProveedorExamRespuestas: ', @@ROWCOUNT);
        END
        IF OBJECT_ID(N'dbo.IndorProveedorDocumentos', N'U') IS NOT NULL
        BEGIN
            DELETE x FROM dbo.IndorProveedorDocumentos x
            INNER JOIN #TargetProveedores tp ON tp.ProveedorId = x.ProveedorId;
            PRINT CONCAT('  IndorProveedorDocumentos: ', @@ROWCOUNT);
        END
        IF OBJECT_ID(N'dbo.IndorProveedorCategoriasSel', N'U') IS NOT NULL
        BEGIN
            DELETE x FROM dbo.IndorProveedorCategoriasSel x
            INNER JOIN #TargetProveedores tp ON tp.ProveedorId = x.ProveedorId;
            PRINT CONCAT('  IndorProveedorCategoriasSel: ', @@ROWCOUNT);
        END
        IF OBJECT_ID(N'dbo.IndorProveedorOfertasSel', N'U') IS NOT NULL
        BEGIN
            DELETE x FROM dbo.IndorProveedorOfertasSel x
            INNER JOIN #TargetProveedores tp ON tp.ProveedorId = x.ProveedorId;
            PRINT CONCAT('  IndorProveedorOfertasSel: ', @@ROWCOUNT);
        END
    END

    /* Customers / leads cuyo email sea el de estos usuarios (si existen) */
    PRINT '-- Provider rows by CustomerEmail / Email --';
    DECLARE emailChild CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.name, t.name, c.name
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        INNER JOIN sys.columns c ON c.object_id = t.object_id
        WHERE c.name IN (N'CustomerEmail', N'Email', N'ClientEmail')
          AND t.name NOT LIKE N'AspNet%'
          AND t.name NOT IN (N'IndorProveedores', N'IndorRealtors', N'IndorPropertyAdministrators')
        ORDER BY t.name, c.name;

    DECLARE @col SYSNAME;
    OPEN emailChild;
    FETCH NEXT FROM emailChild INTO @schema, @tbl, @col;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @full = QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl);
        SET @sql = N'
            DELETE x FROM ' + @full + N' x
            INNER JOIN #TargetUsers tu ON LOWER(LTRIM(RTRIM(x.' + QUOTENAME(@col) + N'))) = LOWER(LTRIM(RTRIM(tu.Email)));
            SET @c = @@ROWCOUNT;';
        BEGIN TRY
            EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
            IF @cnt > 0 PRINT CONCAT('  ', @full, '.', @col, ': ', @cnt);
        END TRY
        BEGIN CATCH
            PRINT CONCAT('  (skip) ', @full, '.', @col, ': ', ERROR_MESSAGE());
        END CATCH
        FETCH NEXT FROM emailChild INTO @schema, @tbl, @col;
    END
    CLOSE emailChild;
    DEALLOCATE emailChild;

    /* ------------------------------------------------------------------
       2) Tablas con PropiedadId de esos usuarios
       ------------------------------------------------------------------ */
    PRINT '-- Datos de propiedad --';
    IF EXISTS (SELECT 1 FROM #TargetProps)
    BEGIN
        DECLARE propRefs CURSOR LOCAL FAST_FORWARD FOR
            SELECT s.name, t.name
            FROM sys.tables t
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.name = N'PropiedadId'
            WHERE t.name <> N'Propiedades'
            ORDER BY t.name;

        OPEN propRefs;
        FETCH NEXT FROM propRefs INTO @schema, @tbl;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @full = QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl);
            SET @sql = N'
                DELETE x FROM ' + @full + N' x
                INNER JOIN #TargetProps tp ON tp.PropiedadId = x.PropiedadId;
                SET @c = @@ROWCOUNT;';
            BEGIN TRY
                EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
                IF @cnt > 0 PRINT CONCAT('  ', @full, ': ', @cnt);
            END TRY
            BEGIN CATCH
                PRINT CONCAT('  (skip) ', @full, ': ', ERROR_MESSAGE());
            END CATCH
            FETCH NEXT FROM propRefs INTO @schema, @tbl;
        END
        CLOSE propRefs;
        DEALLOCATE propRefs;

        DELETE p FROM dbo.Propiedades p
        INNER JOIN #TargetProps tp ON tp.PropiedadId = p.Id;
        PRINT CONCAT('  dbo.Propiedades: ', @@ROWCOUNT);
    END

    /* ------------------------------------------------------------------
       3) Realtor / PA hijos por Id
       ------------------------------------------------------------------ */
    PRINT '-- Realtor / PA --';
    IF EXISTS (SELECT 1 FROM #TargetRealtors)
    BEGIN
        DECLARE realtorChild CURSOR LOCAL FAST_FORWARD FOR
            SELECT s.name, t.name
            FROM sys.tables t
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.name IN (N'RealtorId', N'IndorRealtorId')
            WHERE t.name <> N'IndorRealtors'
            ORDER BY t.name;

        OPEN realtorChild;
        FETCH NEXT FROM realtorChild INTO @schema, @tbl;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @full = QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl);
            IF COL_LENGTH(@schema + N'.' + @tbl, N'RealtorId') IS NOT NULL
                SET @sql = N'DELETE x FROM ' + @full + N' x INNER JOIN #TargetRealtors tr ON tr.RealtorId = x.RealtorId; SET @c = @@ROWCOUNT;';
            ELSE
                SET @sql = N'DELETE x FROM ' + @full + N' x INNER JOIN #TargetRealtors tr ON tr.RealtorId = x.IndorRealtorId; SET @c = @@ROWCOUNT;';
            BEGIN TRY
                EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
                IF @cnt > 0 PRINT CONCAT('  ', @full, ': ', @cnt);
            END TRY
            BEGIN CATCH
                PRINT CONCAT('  (skip) ', @full, ': ', ERROR_MESSAGE());
            END CATCH
            FETCH NEXT FROM realtorChild INTO @schema, @tbl;
        END
        CLOSE realtorChild;
        DEALLOCATE realtorChild;

        DELETE r FROM dbo.IndorRealtors r
        INNER JOIN #TargetRealtors tr ON tr.RealtorId = r.Id;
        PRINT CONCAT('  dbo.IndorRealtors: ', @@ROWCOUNT);
    END

    IF EXISTS (SELECT 1 FROM #TargetPropAdmins)
    BEGIN
        DECLARE paChild CURSOR LOCAL FAST_FORWARD FOR
            SELECT s.name, t.name
            FROM sys.tables t
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            INNER JOIN sys.columns c ON c.object_id = t.object_id
                AND c.name IN (N'PropertyAdministratorId', N'PropAdminId', N'AdministradorId')
            WHERE t.name <> N'IndorPropertyAdministrators'
            ORDER BY t.name;

        OPEN paChild;
        FETCH NEXT FROM paChild INTO @schema, @tbl;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @full = QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl);
            IF COL_LENGTH(@schema + N'.' + @tbl, N'PropertyAdministratorId') IS NOT NULL
                SET @sql = N'DELETE x FROM ' + @full + N' x INNER JOIN #TargetPropAdmins ta ON ta.PropAdminId = x.PropertyAdministratorId; SET @c = @@ROWCOUNT;';
            ELSE IF COL_LENGTH(@schema + N'.' + @tbl, N'PropAdminId') IS NOT NULL
                SET @sql = N'DELETE x FROM ' + @full + N' x INNER JOIN #TargetPropAdmins ta ON ta.PropAdminId = x.PropAdminId; SET @c = @@ROWCOUNT;';
            ELSE
                SET @sql = N'DELETE x FROM ' + @full + N' x INNER JOIN #TargetPropAdmins ta ON ta.PropAdminId = x.AdministradorId; SET @c = @@ROWCOUNT;';
            BEGIN TRY
                EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
                IF @cnt > 0 PRINT CONCAT('  ', @full, ': ', @cnt);
            END TRY
            BEGIN CATCH
                PRINT CONCAT('  (skip) ', @full, ': ', ERROR_MESSAGE());
            END CATCH
            FETCH NEXT FROM paChild INTO @schema, @tbl;
        END
        CLOSE paChild;
        DEALLOCATE paChild;

        DELETE a FROM dbo.IndorPropertyAdministrators a
        INNER JOIN #TargetPropAdmins ta ON ta.PropAdminId = a.Id;
        PRINT CONCAT('  dbo.IndorPropertyAdministrators: ', @@ROWCOUNT);
    END

    /* ------------------------------------------------------------------
       4) Todas las tablas con UserId (excepto AspNet*)
       ------------------------------------------------------------------ */
    PRINT '-- Tablas con UserId --';
    DECLARE userIdTables CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.name, t.name
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.name = N'UserId'
        WHERE t.name NOT LIKE N'AspNet%'
        ORDER BY
            CASE
                WHEN t.name LIKE N'Archivo%' OR t.name LIKE N'Archivos%' THEN 0
                WHEN t.name LIKE N'Solicitud%' OR t.name LIKE N'Solicitudes%' THEN 1
                ELSE 2
            END,
            t.name;

    OPEN userIdTables;
    FETCH NEXT FROM userIdTables INTO @schema, @tbl;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @full = QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl);
        SET @sql = N'
            DELETE x FROM ' + @full + N' x
            INNER JOIN #TargetUsers tu ON tu.UserId = x.UserId;
            SET @c = @@ROWCOUNT;';
        BEGIN TRY
            EXEC sp_executesql @sql, N'@c INT OUTPUT', @cnt OUTPUT;
            IF @cnt > 0 PRINT CONCAT('  ', @full, ': ', @cnt);
        END TRY
        BEGIN CATCH
            PRINT CONCAT('  (skip) ', @full, ': ', ERROR_MESSAGE());
        END CATCH
        FETCH NEXT FROM userIdTables INTO @schema, @tbl;
    END
    CLOSE userIdTables;
    DEALLOCATE userIdTables;

    /* Password reset por email o UserId */
    IF OBJECT_ID(N'dbo.PasswordResetCodes', N'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH(N'dbo.PasswordResetCodes', N'UserId') IS NOT NULL
        BEGIN
            DELETE x FROM dbo.PasswordResetCodes x
            INNER JOIN #TargetUsers tu ON tu.UserId = x.UserId;
            PRINT CONCAT('  PasswordResetCodes(UserId): ', @@ROWCOUNT);
        END
        IF COL_LENGTH(N'dbo.PasswordResetCodes', N'Email') IS NOT NULL
        BEGIN
            DELETE x FROM dbo.PasswordResetCodes x
            INNER JOIN #TargetUsers tu ON LOWER(LTRIM(RTRIM(x.Email))) = LOWER(LTRIM(RTRIM(tu.Email)));
            PRINT CONCAT('  PasswordResetCodes(Email): ', @@ROWCOUNT);
        END
    END

    /* Proveedores (después de hijos) */
    IF OBJECT_ID(N'dbo.IndorProveedores', N'U') IS NOT NULL
    BEGIN
        DELETE p FROM dbo.IndorProveedores p
        INNER JOIN #TargetUsers tu ON tu.UserId = p.UserId;
        PRINT CONCAT('  dbo.IndorProveedores: ', @@ROWCOUNT);
    END

    /* ------------------------------------------------------------------
       5) ASP.NET Identity
       ------------------------------------------------------------------ */
    PRINT '-- Identity --';
    IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NOT NULL
    BEGIN
        DELETE x FROM dbo.AspNetUserTokens x
        INNER JOIN #TargetUsers tu ON tu.UserId = x.UserId;
        PRINT CONCAT('  AspNetUserTokens: ', @@ROWCOUNT);
    END
    IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL
    BEGIN
        DELETE x FROM dbo.AspNetUserLogins x
        INNER JOIN #TargetUsers tu ON tu.UserId = x.UserId;
        PRINT CONCAT('  AspNetUserLogins: ', @@ROWCOUNT);
    END
    IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL
    BEGIN
        DELETE x FROM dbo.AspNetUserClaims x
        INNER JOIN #TargetUsers tu ON tu.UserId = x.UserId;
        PRINT CONCAT('  AspNetUserClaims: ', @@ROWCOUNT);
    END
    IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL
    BEGIN
        DELETE x FROM dbo.AspNetUserRoles x
        INNER JOIN #TargetUsers tu ON tu.UserId = x.UserId;
        PRINT CONCAT('  AspNetUserRoles: ', @@ROWCOUNT);
    END

    DELETE u FROM dbo.AspNetUsers u
    INNER JOIN #TargetUsers tu ON tu.UserId = u.Id;
    PRINT CONCAT('  AspNetUsers: ', @@ROWCOUNT);

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '=== Completado. Verificar: ===';
    SELECT Email FROM dbo.AspNetUsers
    WHERE LOWER(LTRIM(RTRIM(Email))) IN (
        N'jhamclayz@gmail.com',
        N'jhamclayinvestor@gmail.com'
    );
    -- Debe devolver 0 filas.
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @line INT = ERROR_LINE();
    RAISERROR(N'DeleteUsersByEmail_Jhamclay falló (línea %d): %s', 16, 1, @line, @msg);
END CATCH;
GO
