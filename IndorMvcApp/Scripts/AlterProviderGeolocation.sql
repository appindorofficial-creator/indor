/*
  INDOR — Add geolocation columns to registered providers for map display.
  Safe to run multiple times.

  Note: GO separates batches so CREATE INDEX is compiled after columns exist.
*/

IF COL_LENGTH(N'dbo.IndorProveedores', N'Latitude') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD Latitude DECIMAL(9,6) NULL;
    PRINT 'Column IndorProveedores.Latitude added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'Longitude') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD Longitude DECIMAL(9,6) NULL;
    PRINT 'Column IndorProveedores.Longitude added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'Latitude') IS NOT NULL
   AND COL_LENGTH(N'dbo.IndorProveedores', N'Longitude') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = N'IX_IndorProveedores_Geo'
         AND object_id = OBJECT_ID(N'dbo.IndorProveedores')
   )
BEGIN
    CREATE INDEX IX_IndorProveedores_Geo
        ON dbo.IndorProveedores (Latitude, Longitude)
        WHERE Latitude IS NOT NULL AND Longitude IS NOT NULL;

    PRINT 'Index IX_IndorProveedores_Geo created.';
END
GO

PRINT 'Provider geolocation columns ready.';
