using IndorMvcApp.Data;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

/// <summary>
/// Ensures provider profile columns exist on older Azure databases created before
/// Edit Profile shipped (BusinessAddress, ServiceDescription, geolocation, etc.).
/// </summary>
public static class ProviderDatabaseSchemaInitializer
{
    private static readonly string[] EditProfileColumnStatements =
    [
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'BusinessAddress') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD BusinessAddress NVARCHAR(300) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'ServiceDescription') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD ServiceDescription NVARCHAR(200) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'Latitude') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD Latitude DECIMAL(9,6) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'Longitude') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD Longitude DECIMAL(9,6) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'IsInsured') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD IsInsured BIT NOT NULL
                CONSTRAINT DF_IndorProveedores_IsInsured DEFAULT (0);
        """
    ];

    public static async Task EnsureEditProfileColumnsAsync(
        AppDbContext db,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        foreach (var sql in EditProfileColumnStatements)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Provider schema ensure step failed.");
            }
        }
    }
}
