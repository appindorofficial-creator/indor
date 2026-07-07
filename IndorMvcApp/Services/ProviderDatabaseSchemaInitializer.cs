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

    /// <summary>
    /// Contractor Network tables (subcontractor marketplace). Created on demand so
    /// existing Azure databases pick them up without a manual migration.
    /// </summary>
    private static readonly string[] NetworkTableStatements =
    [
        """
        IF OBJECT_ID(N'dbo.IndorProveedorNetworkJobs', N'U') IS NULL
        CREATE TABLE dbo.IndorProveedorNetworkJobs (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkJobs PRIMARY KEY,
            PosterProveedorId INT NOT NULL,
            TradeId NVARCHAR(40) NULL,
            TradeLabel NVARCHAR(120) NULL,
            Description NVARCHAR(600) NULL,
            Location NVARCHAR(200) NULL,
            DateNeeded DATETIME2 NULL,
            BudgetRange NVARCHAR(40) NULL,
            PhotoUrl NVARCHAR(500) NULL,
            Status NVARCHAR(30) NOT NULL CONSTRAINT DF_IndorNetworkJobs_Status DEFAULT ('Open'),
            FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkJobs_Fecha DEFAULT (SYSUTCDATETIME()),
            FechaActualizacion DATETIME2 NULL
        );
        """,
        """
        IF OBJECT_ID(N'dbo.IndorProveedorNetworkHires', N'U') IS NULL
        CREATE TABLE dbo.IndorProveedorNetworkHires (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkHires PRIMARY KEY,
            HirerProveedorId INT NOT NULL,
            SubcontractorProveedorId INT NOT NULL,
            NetworkJobId INT NULL,
            ProjectTitle NVARCHAR(160) NULL,
            TradeLabel NVARCHAR(120) NULL,
            BudgetRange NVARCHAR(40) NULL,
            StartDate DATETIME2 NULL,
            Status NVARCHAR(30) NOT NULL CONSTRAINT DF_IndorNetworkHires_Status DEFAULT ('Hired'),
            FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkHires_Fecha DEFAULT (SYSUTCDATETIME())
        );
        """,
        """
        IF OBJECT_ID(N'dbo.IndorProveedorNetworkResenas', N'U') IS NULL
        CREATE TABLE dbo.IndorProveedorNetworkResenas (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkResenas PRIMARY KEY,
            SubcontractorProveedorId INT NOT NULL,
            AuthorProveedorId INT NULL,
            AuthorName NVARCHAR(120) NULL,
            Rating INT NOT NULL CONSTRAINT DF_IndorNetworkResenas_Rating DEFAULT (5),
            Comment NVARCHAR(600) NULL,
            FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkResenas_Fecha DEFAULT (SYSUTCDATETIME())
        );
        """,
        """
        IF OBJECT_ID(N'dbo.IndorProveedorNetworkGuardados', N'U') IS NULL
        CREATE TABLE dbo.IndorProveedorNetworkGuardados (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkGuardados PRIMARY KEY,
            OwnerProveedorId INT NOT NULL,
            SubcontractorProveedorId INT NOT NULL,
            FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkGuardados_Fecha DEFAULT (SYSUTCDATETIME())
        );
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

        foreach (var sql in NetworkTableStatements)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Contractor network schema ensure step failed.");
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
