-- =============================================================
-- IndorDB — Add Spanish (*Es) columns for bilingual catalog data
-- Safe to run multiple times (idempotent).
--
-- Convention:
--   English text stays in existing columns (Nombre, LabelEn, PageTitle, …)
--   Spanish text goes in new *Es columns (NombreEs, LabelEs, PageTitleEs, …)
--
-- Usage (SSMS / sqlcmd):
--   USE [IndorDB];
--   GO
--   :r AlterCatalogTablesAddSpanishColumns.sql
--
-- After this script, run:
--   :r SeedCatalogSpanishTranslations.sql
-- =============================================================

SET NOCOUNT ON;
GO

/* ---------- helper: add nullable column if missing ---------- */
IF OBJECT_ID(N'tempdb..#AddCol') IS NOT NULL DROP PROCEDURE #AddCol;
GO

CREATE PROCEDURE #AddCol
    @Table SYSNAME,
    @Column SYSNAME,
    @Definition NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID(N'dbo.' + QUOTENAME(@Table), N'U') IS NULL
        RETURN;

    IF COL_LENGTH(@Table, @Column) IS NULL
    BEGIN
        DECLARE @sql NVARCHAR(MAX) =
            N'ALTER TABLE dbo.' + QUOTENAME(@Table) + N' ADD ' + QUOTENAME(@Column) + N' ' + @Definition + N';';
        EXEC sys.sp_executesql @sql;
        PRINT N'Added ' + @Table + N'.' + @Column;
    END
END
GO

/* =============================================================
   P0 — Core home catalog (Microservicios, Servicios, Inspecciones)
   ============================================================= */
EXEC #AddCol @Table = N'Microservicios', @Column = N'NombreEs',           @Definition = N'NVARCHAR(150) NULL';
EXEC #AddCol @Table = N'Microservicios', @Column = N'SubtituloEs',        @Definition = N'NVARCHAR(250) NULL';
EXEC #AddCol @Table = N'Microservicios', @Column = N'DescripcionEs',      @Definition = N'NVARCHAR(1000) NULL';
EXEC #AddCol @Table = N'Microservicios', @Column = N'DescripcionCompletaEs', @Definition = N'NVARCHAR(MAX) NULL';
EXEC #AddCol @Table = N'Microservicios', @Column = N'IncluyeEs',           @Definition = N'NVARCHAR(MAX) NULL';
EXEC #AddCol @Table = N'Microservicios', @Column = N'FrecuenciaEs',        @Definition = N'NVARCHAR(100) NULL';
EXEC #AddCol @Table = N'Microservicios', @Column = N'PrecioPrefijoEs',    @Definition = N'NVARCHAR(50) NULL';
EXEC #AddCol @Table = N'Microservicios', @Column = N'CtaTextoEs',          @Definition = N'NVARCHAR(80) NULL';

EXEC #AddCol @Table = N'Servicios', @Column = N'NombreEs',                @Definition = N'NVARCHAR(150) NULL';
EXEC #AddCol @Table = N'Servicios', @Column = N'SubtituloEs',             @Definition = N'NVARCHAR(250) NULL';
EXEC #AddCol @Table = N'Servicios', @Column = N'DescripcionEs',           @Definition = N'NVARCHAR(1000) NULL';
EXEC #AddCol @Table = N'Servicios', @Column = N'DescripcionCompletaEs',  @Definition = N'NVARCHAR(MAX) NULL';
EXEC #AddCol @Table = N'Servicios', @Column = N'IncluyeEs',               @Definition = N'NVARCHAR(MAX) NULL';
EXEC #AddCol @Table = N'Servicios', @Column = N'FrecuenciaEs',            @Definition = N'NVARCHAR(100) NULL';
EXEC #AddCol @Table = N'Servicios', @Column = N'PrecioPrefijoEs',         @Definition = N'NVARCHAR(50) NULL';
EXEC #AddCol @Table = N'Servicios', @Column = N'PrecioTextoEs',           @Definition = N'NVARCHAR(50) NULL';
EXEC #AddCol @Table = N'Servicios', @Column = N'CtaTextoEs',              @Definition = N'NVARCHAR(80) NULL';

EXEC #AddCol @Table = N'Inspecciones', @Column = N'NombreEs',             @Definition = N'NVARCHAR(150) NULL';
EXEC #AddCol @Table = N'Inspecciones', @Column = N'SubtituloEs',          @Definition = N'NVARCHAR(250) NULL';
EXEC #AddCol @Table = N'Inspecciones', @Column = N'DescripcionEs',        @Definition = N'NVARCHAR(1000) NULL';
EXEC #AddCol @Table = N'Inspecciones', @Column = N'DescripcionCompletaEs', @Definition = N'NVARCHAR(MAX) NULL';
EXEC #AddCol @Table = N'Inspecciones', @Column = N'IncluyeEs',            @Definition = N'NVARCHAR(MAX) NULL';
EXEC #AddCol @Table = N'Inspecciones', @Column = N'FrecuenciaEs',         @Definition = N'NVARCHAR(100) NULL';
EXEC #AddCol @Table = N'Inspecciones', @Column = N'PrecioPrefijoEs',      @Definition = N'NVARCHAR(50) NULL';
EXEC #AddCol @Table = N'Inspecciones', @Column = N'PrecioTextoEs',        @Definition = N'NVARCHAR(50) NULL';
EXEC #AddCol @Table = N'Inspecciones', @Column = N'CtaTextoEs',            @Definition = N'NVARCHAR(80) NULL';

EXEC #AddCol @Table = N'ServiciosEmergencia', @Column = N'NombreEs',             @Definition = N'NVARCHAR(80) NULL';
EXEC #AddCol @Table = N'ServiciosEmergencia', @Column = N'TituloEmergenciaEs',   @Definition = N'NVARCHAR(150) NULL';
EXEC #AddCol @Table = N'ServiciosEmergencia', @Column = N'DescripcionEs',        @Definition = N'NVARCHAR(300) NULL';
EXEC #AddCol @Table = N'ServiciosEmergencia', @Column = N'BadgeTextoEs',         @Definition = N'NVARCHAR(80) NULL';
EXEC #AddCol @Table = N'ServiciosEmergencia', @Column = N'CaracteristicasEs',    @Definition = N'NVARCHAR(500) NULL';
EXEC #AddCol @Table = N'ServiciosEmergencia', @Column = N'CtaTextoEs',           @Definition = N'NVARCHAR(80) NULL';

EXEC #AddCol @Table = N'PlanesMembresia', @Column = N'NombreEs',           @Definition = N'NVARCHAR(100) NULL';
EXEC #AddCol @Table = N'PlanesMembresia', @Column = N'SubtituloEs',        @Definition = N'NVARCHAR(250) NULL';
EXEC #AddCol @Table = N'PlanesMembresia', @Column = N'DescripcionEs',      @Definition = N'NVARCHAR(1000) NULL';
EXEC #AddCol @Table = N'PlanesMembresia', @Column = N'CaracteristicasEs',  @Definition = N'NVARCHAR(MAX) NULL';

EXEC #AddCol @Table = N'PlanesInternet', @Column = N'NombreEs',            @Definition = N'NVARCHAR(120) NULL';
EXEC #AddCol @Table = N'PlanesInternet', @Column = N'CaracteristicasEs',  @Definition = N'NVARCHAR(MAX) NULL';

/* =============================================================
   P1 — Home sections
   ============================================================= */
EXEC #AddCol @Table = N'HomeCarePrioritiesConfig', @Column = N'TituloEs',       @Definition = N'NVARCHAR(80) NULL';
EXEC #AddCol @Table = N'HomeCarePrioritiesConfig', @Column = N'SubtituloEs',    @Definition = N'NVARCHAR(200) NULL';
EXEC #AddCol @Table = N'HomeCarePrioritiesConfig', @Column = N'ViewAllTextoEs', @Definition = N'NVARCHAR(40) NULL';

EXEC #AddCol @Table = N'HomeCarePriorities', @Column = N'NombreEs',    @Definition = N'NVARCHAR(120) NULL';
EXEC #AddCol @Table = N'HomeCarePriorities', @Column = N'SubtituloEs', @Definition = N'NVARCHAR(120) NULL';

EXEC #AddCol @Table = N'MovingSetupConfig', @Column = N'TituloEs',              @Definition = N'NVARCHAR(80) NULL';
EXEC #AddCol @Table = N'MovingSetupConfig', @Column = N'SubtituloEs',           @Definition = N'NVARCHAR(200) NULL';
EXEC #AddCol @Table = N'MovingSetupConfig', @Column = N'ViewAllTextoEs',        @Definition = N'NVARCHAR(40) NULL';
EXEC #AddCol @Table = N'MovingSetupConfig', @Column = N'FeaturedEtiquetaEs',    @Definition = N'NVARCHAR(60) NULL';
EXEC #AddCol @Table = N'MovingSetupConfig', @Column = N'FeaturedTituloEs',      @Definition = N'NVARCHAR(120) NULL';
EXEC #AddCol @Table = N'MovingSetupConfig', @Column = N'FeaturedDescripcionEs', @Definition = N'NVARCHAR(400) NULL';
EXEC #AddCol @Table = N'MovingSetupConfig', @Column = N'FeaturedCaracteristicasEs', @Definition = N'NVARCHAR(500) NULL';
EXEC #AddCol @Table = N'MovingSetupConfig', @Column = N'FeaturedCtaTextoEs',    @Definition = N'NVARCHAR(60) NULL';

EXEC #AddCol @Table = N'MovingSetupServicios', @Column = N'NombreEs', @Definition = N'NVARCHAR(120) NULL';
EXEC #AddCol @Table = N'MovingSetupEnlacesRapidos', @Column = N'NombreEs', @Definition = N'NVARCHAR(120) NULL';

/* =============================================================
   P2 — Catalogs that already use *En suffix
   ============================================================= */
EXEC #AddCol @Table = N'IndorProveedorCategoriasCatalogo', @Column = N'LabelEs',       @Definition = N'NVARCHAR(120) NULL';
EXEC #AddCol @Table = N'IndorProveedorCategoriasCatalogo', @Column = N'DescriptionEs', @Definition = N'NVARCHAR(200) NULL';

EXEC #AddCol @Table = N'IndorProveedorOfertasCatalogo', @Column = N'LabelEs', @Definition = N'NVARCHAR(120) NULL';

EXEC #AddCol @Table = N'IndorProveedorAlcanceReglas', @Column = N'LabelEs', @Definition = N'NVARCHAR(120) NULL';

EXEC #AddCol @Table = N'IndorNeighborRequestCategories', @Column = N'LabelEs',       @Definition = N'NVARCHAR(80) NULL';
EXEC #AddCol @Table = N'IndorNeighborRequestCategories', @Column = N'DescriptionEs', @Definition = N'NVARCHAR(200) NULL';

EXEC #AddCol @Table = N'LawnCatalogOptions', @Column = N'LabelEs',       @Definition = N'NVARCHAR(120) NULL';
EXEC #AddCol @Table = N'LawnCatalogOptions', @Column = N'DescriptionEs', @Definition = N'NVARCHAR(200) NULL';

EXEC #AddCol @Table = N'IndorPropertyAdminServiceCatalog', @Column = N'CategoryTitleEs', @Definition = N'NVARCHAR(120) NULL';
EXEC #AddCol @Table = N'IndorPropertyAdminServiceCatalog', @Column = N'ServiceNameEs',   @Definition = N'NVARCHAR(120) NULL';

EXEC #AddCol @Table = N'IndorPropertyAdminPreventiveServiceCatalog', @Column = N'ServiceNameEs',      @Definition = N'NVARCHAR(120) NULL';
EXEC #AddCol @Table = N'IndorPropertyAdminPreventiveServiceCatalog', @Column = N'DefaultFrequencyEs', @Definition = N'NVARCHAR(80) NULL';

/* =============================================================
   P3 — Service landing pages (*ServicioLanding)
   ============================================================= */
DECLARE @LandingTables TABLE (TableName SYSNAME PRIMARY KEY);
INSERT INTO @LandingTables (TableName) VALUES
 (N'LawnServicioLanding'),
 (N'SafeAirServicioLanding'),
 (N'TrashServicioLanding'),
 (N'CleaningProServicioLanding'),
 (N'MovingServicioLanding'),
 (N'CleaningServicioLanding'),
 (N'PackingServicioLanding'),
 (N'FurnitureAssemblyServicioLanding'),
 (N'TvWallMountingServicioLanding'),
 (N'HvacMaintenanceServicioLanding'),
 (N'WaterHeaterFlushServicioLanding'),
 (N'RoofInspectionServicioLanding'),
 (N'CrawlspaceCheckServicioLanding'),
 (N'ExteriorPaintServicioLanding'),
 (N'GutterCleaningServicioLanding'),
 (N'PowerWashServicioLanding'),
 (N'PestControlServicioLanding'),
 (N'SmokeDetectorServicioLanding');

DECLARE @LandingColumns TABLE (ColumnName SYSNAME NOT NULL, Definition NVARCHAR(200) NOT NULL);
INSERT INTO @LandingColumns (ColumnName, Definition) VALUES
 (N'PageTitle',              N'NVARCHAR(80) NULL'),
 (N'LandingTitulo',         N'NVARCHAR(120) NULL'),
 (N'LandingTagline',        N'NVARCHAR(200) NULL'),
 (N'LandingSubtitulo',      N'NVARCHAR(400) NULL'),
 (N'PrecioTexto',           N'NVARCHAR(120) NULL'),
 (N'IncluyeItems',          N'NVARCHAR(500) NULL'),
 (N'InfoBoxTexto',          N'NVARCHAR(400) NULL'),
 (N'InfoBoxTitulo',         N'NVARCHAR(120) NULL'),
 (N'CtaTexto',              N'NVARCHAR(40) NULL'),
 (N'ReminderBannerTitulo',  N'NVARCHAR(80) NULL'),
 (N'ReminderBannerTexto',   N'NVARCHAR(300) NULL'),
 (N'RemindOnlyCtaTexto',    N'NVARCHAR(60) NULL'),
 (N'BestForLabel',          N'NVARCHAR(60) NULL'),
 (N'BestForOptions',        N'NVARCHAR(200) NULL'),
 (N'BestForValues',         N'NVARCHAR(200) NULL'),
 (N'EstimatedTimeLabel',    N'NVARCHAR(60) NULL'),
 (N'EstimatedTimeValue',    N'NVARCHAR(60) NULL'),
 (N'BestTimingLabel',       N'NVARCHAR(60) NULL'),
 (N'BestTimingValue',       N'NVARCHAR(120) NULL');

DECLARE @t SYSNAME, @c SYSNAME, @def NVARCHAR(200), @es SYSNAME, @sql NVARCHAR(MAX);

DECLARE landing_cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT lt.TableName, lc.ColumnName, lc.Definition
    FROM @LandingTables lt
    CROSS JOIN @LandingColumns lc;

OPEN landing_cur;
FETCH NEXT FROM landing_cur INTO @t, @c, @def;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @es = @c + N'Es';
    IF OBJECT_ID(N'dbo.' + QUOTENAME(@t), N'U') IS NOT NULL
       AND COL_LENGTH(@t, @c) IS NOT NULL
       AND COL_LENGTH(@t, @es) IS NULL
    BEGIN
        SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@t) + N' ADD ' + QUOTENAME(@es) + N' ' + @def + N';';
        EXEC sys.sp_executesql @sql;
        PRINT N'Added ' + @t + N'.' + @es;
    END
    FETCH NEXT FROM landing_cur INTO @t, @c, @def;
END

CLOSE landing_cur;
DEALLOCATE landing_cur;

DROP PROCEDURE #AddCol;
GO

PRINT 'AlterCatalogTablesAddSpanishColumns completed.';
GO
