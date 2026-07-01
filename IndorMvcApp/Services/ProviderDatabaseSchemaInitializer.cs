using IndorMvcApp.Data;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

/// <summary>
/// Ensures provider profile columns exist on older Azure databases created before
/// Edit Profile and multi-trade onboarding shipped.
/// </summary>
public static class ProviderDatabaseSchemaInitializer
{
    private static readonly string[] ProviderColumnStatements =
    [
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'EpaCertificationNumber') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD EpaCertificationNumber NVARCHAR(80) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'BackgroundCheckConsent') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD BackgroundCheckConsent BIT NOT NULL
                CONSTRAINT DF_IndorProveedores_BgCheck DEFAULT (0);
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'ServiceDescription') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD ServiceDescription NVARCHAR(200) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'IsInsured') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD IsInsured BIT NOT NULL
                CONSTRAINT DF_IndorProveedores_IsInsured DEFAULT (0);
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'IsLicensed') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD IsLicensed BIT NOT NULL
                CONSTRAINT DF_IndorProveedores_IsLicensed DEFAULT (0);
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'TeamSize') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD TeamSize NVARCHAR(40) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedores', N'BusinessAddress') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD BusinessAddress NVARCHAR(300) NULL;
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
        IF COL_LENGTH(N'dbo.IndorProveedores', N'OnboardingMetaJson') IS NULL
            ALTER TABLE dbo.IndorProveedores ADD OnboardingMetaJson NVARCHAR(2000) NULL;
        """
    ];

    public static async Task EnsureEditProfileColumnsAsync(
        AppDbContext db,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!await TableExistsAsync(db, cancellationToken))
        {
            return;
        }

        foreach (var sql in ProviderColumnStatements)
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

    private static async Task<bool> TableExistsAsync(
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT CASE WHEN OBJECT_ID(N'dbo.IndorProveedores', N'U') IS NOT NULL THEN 1 ELSE 0 END;
            """;
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is 1 or true;
    }
}
