using IndorMvcApp.Data;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

/// <summary>
/// Ensures realtor notification preference columns exist on older Azure DBs.
/// </summary>
public static class RealtorNotificationSchemaInitializer
{
    private static readonly string[] ColumnStatements =
    [
        """
        IF COL_LENGTH(N'dbo.IndorRealtors', N'NotifyEmailAlerts') IS NULL
            ALTER TABLE dbo.IndorRealtors
                ADD NotifyEmailAlerts BIT NOT NULL
                    CONSTRAINT DF_IndorRealtors_NotifyEmailAlerts DEFAULT (1);
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtors', N'NotifyQuoteUpdates') IS NULL
            ALTER TABLE dbo.IndorRealtors
                ADD NotifyQuoteUpdates BIT NOT NULL
                    CONSTRAINT DF_IndorRealtors_NotifyQuoteUpdates DEFAULT (1);
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtors', N'NotifyReportNotifications') IS NULL
            ALTER TABLE dbo.IndorRealtors
                ADD NotifyReportNotifications BIT NOT NULL
                    CONSTRAINT DF_IndorRealtors_NotifyReportNotifications DEFAULT (1);
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtors', N'NotifyPackageViewAlerts') IS NULL
            ALTER TABLE dbo.IndorRealtors
                ADD NotifyPackageViewAlerts BIT NOT NULL
                    CONSTRAINT DF_IndorRealtors_NotifyPackageViewAlerts DEFAULT (0);
        """
    ];

    public static Task EnsureColumnsAsync(
        AppDbContext db,
        CancellationToken cancellationToken = default)
        => EnsureColumnsAsync(db, null, cancellationToken);

    public static async Task EnsureColumnsAsync(
        AppDbContext db,
        ILogger? logger,
        CancellationToken cancellationToken = default)
    {
        foreach (var sql in ColumnStatements)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Could not ensure realtor notification preference column.");
            }
        }
    }
}
