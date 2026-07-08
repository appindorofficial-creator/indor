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
            FechaActualizacion DATETIME2 NULL,
            JobTitle NVARCHAR(160) NULL,
            Urgency NVARCHAR(20) NULL,
            PropertyType NVARCHAR(30) NULL,
            WhoMeets NVARCHAR(30) NULL,
            QuoteType NVARCHAR(20) NULL,
            AccessNotes NVARCHAR(300) NULL,
            PhotoUrlsJson NVARCHAR(MAX) NULL,
            Latitude DECIMAL(9,6) NULL,
            Longitude DECIMAL(9,6) NULL
        );
        """,
        // Columns added after the network tables first shipped (idempotent).
        """
        IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'JobTitle') IS NULL
            ALTER TABLE dbo.IndorProveedorNetworkJobs ADD JobTitle NVARCHAR(160) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'Urgency') IS NULL
            ALTER TABLE dbo.IndorProveedorNetworkJobs ADD Urgency NVARCHAR(20) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'PropertyType') IS NULL
            ALTER TABLE dbo.IndorProveedorNetworkJobs ADD PropertyType NVARCHAR(30) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'WhoMeets') IS NULL
            ALTER TABLE dbo.IndorProveedorNetworkJobs ADD WhoMeets NVARCHAR(30) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'QuoteType') IS NULL
            ALTER TABLE dbo.IndorProveedorNetworkJobs ADD QuoteType NVARCHAR(20) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'AccessNotes') IS NULL
            ALTER TABLE dbo.IndorProveedorNetworkJobs ADD AccessNotes NVARCHAR(300) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'PhotoUrlsJson') IS NULL
            ALTER TABLE dbo.IndorProveedorNetworkJobs ADD PhotoUrlsJson NVARCHAR(MAX) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'Latitude') IS NULL
            ALTER TABLE dbo.IndorProveedorNetworkJobs ADD Latitude DECIMAL(9,6) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorNetworkJobs', N'Longitude') IS NULL
            ALTER TABLE dbo.IndorProveedorNetworkJobs ADD Longitude DECIMAL(9,6) NULL;
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
        """,
        """
        IF OBJECT_ID(N'dbo.IndorProveedorNetworkQuotes', N'U') IS NULL
        CREATE TABLE dbo.IndorProveedorNetworkQuotes (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkQuotes PRIMARY KEY,
            NetworkJobId INT NOT NULL,
            SubcontractorProveedorId INT NOT NULL,
            AmountLow DECIMAL(10,2) NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Low DEFAULT (0),
            AmountHigh DECIMAL(10,2) NOT NULL CONSTRAINT DF_IndorNetworkQuotes_High DEFAULT (0),
            QuotedAmount DECIMAL(10,2) NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Amt DEFAULT (0),
            ResponseMinutes INT NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Resp DEFAULT (60),
            Message NVARCHAR(400) NULL,
            Status NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Status DEFAULT ('Pending'),
            FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkQuotes_Fecha DEFAULT (SYSUTCDATETIME())
        );
        """,
        """
        IF OBJECT_ID(N'dbo.IndorProveedorNetworkInvitaciones', N'U') IS NULL
        CREATE TABLE dbo.IndorProveedorNetworkInvitaciones (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorNetworkInvitaciones PRIMARY KEY,
            InviterProveedorId INT NOT NULL,
            SubcontractorProveedorId INT NOT NULL,
            NetworkJobId INT NULL,
            JobTitle NVARCHAR(160) NULL,
            TradeId NVARCHAR(40) NULL,
            ServiceCategory NVARCHAR(120) NULL,
            PropertyAddress NVARCHAR(300) NULL,
            ScheduleDate DATETIME2 NULL,
            ScheduleToday BIT NOT NULL CONSTRAINT DF_IndorNetworkInv_Today DEFAULT (1),
            BudgetRange NVARCHAR(40) NULL,
            Description NVARCHAR(600) NULL,
            TimingPreference NVARCHAR(20) NULL,
            AttachmentsJson NVARCHAR(MAX) NULL,
            Status NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorNetworkInv_Status DEFAULT ('Sent'),
            FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorNetworkInv_Fecha DEFAULT (SYSUTCDATETIME())
        );
        """,
        """
        IF OBJECT_ID(N'dbo.IndorProveedorVerificaciones', N'U') IS NULL
        CREATE TABLE dbo.IndorProveedorVerificaciones (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_IndorProveedorVerificaciones PRIMARY KEY,
            ProveedorId INT NOT NULL,
            Status NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorVerificaciones_Status DEFAULT ('Pending'),
            LicenseVerified BIT NOT NULL CONSTRAINT DF_IndorVerificaciones_Lic DEFAULT (0),
            LicenseExpiry DATETIME2 NULL,
            InsuranceVerified BIT NOT NULL CONSTRAINT DF_IndorVerificaciones_Ins DEFAULT (0),
            InsuranceExpiry DATETIME2 NULL,
            W9Verified BIT NOT NULL CONSTRAINT DF_IndorVerificaciones_W9 DEFAULT (0),
            BackgroundStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorVerificaciones_Bg DEFAULT ('Pending'),
            OperatorNotes NVARCHAR(600) NULL,
            FollowUpNote NVARCHAR(300) NULL,
            ReviewerName NVARCHAR(160) NULL,
            ApprovedUtc DATETIME2 NULL,
            FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_IndorVerificaciones_Fecha DEFAULT (SYSUTCDATETIME()),
            FechaActualizacion DATETIME2 NULL,
            CONSTRAINT UQ_IndorVerificaciones_Proveedor UNIQUE (ProveedorId)
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
