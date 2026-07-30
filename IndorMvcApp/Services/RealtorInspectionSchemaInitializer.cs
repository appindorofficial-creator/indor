using IndorMvcApp.Data;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

/// <summary>
/// Ensures realtor inspection → provider lead bridge columns exist on older Azure DBs
/// (same pattern as <see cref="ProviderDatabaseSchemaInitializer"/>).
/// </summary>
public static class RealtorInspectionSchemaInitializer
{
    private static readonly string[] BridgeColumnStatements =
    [
        """
        IF COL_LENGTH(N'dbo.IndorProveedorLeads', N'RealtorQuoteId') IS NULL
            ALTER TABLE dbo.IndorProveedorLeads ADD RealtorQuoteId INT NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorLeads', N'LeadSource') IS NULL
            ALTER TABLE dbo.IndorProveedorLeads ADD LeadSource NVARCHAR(40) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorLeads', N'InspectionReportUrl') IS NULL
            ALTER TABLE dbo.IndorProveedorLeads ADD InspectionReportUrl NVARCHAR(500) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorProveedorLeads', N'FindingsJson') IS NULL
            ALTER TABLE dbo.IndorProveedorLeads ADD FindingsJson NVARCHAR(MAX) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtorQuoteSentProviders', N'ProveedorId') IS NULL
            ALTER TABLE dbo.IndorRealtorQuoteSentProviders ADD ProveedorId INT NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtorQuoteSentProviders', N'LeadId') IS NULL
            ALTER TABLE dbo.IndorRealtorQuoteSentProviders ADD LeadId INT NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtorQuoteBids', N'ProveedorId') IS NULL
            ALTER TABLE dbo.IndorRealtorQuoteBids ADD ProveedorId INT NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtorQuoteBids', N'EstimateId') IS NULL
            ALTER TABLE dbo.IndorRealtorQuoteBids ADD EstimateId INT NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtorQuoteBids', N'LeadId') IS NULL
            ALTER TABLE dbo.IndorRealtorQuoteBids ADD LeadId INT NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtorQuoteBids', N'SubmittedUtc') IS NULL
            ALTER TABLE dbo.IndorRealtorQuoteBids ADD SubmittedUtc DATETIME2(7) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtorInspectionUploadFindings', N'Description') IS NULL
            ALTER TABLE dbo.IndorRealtorInspectionUploadFindings ADD Description NVARCHAR(1000) NULL;
        """,
        """
        IF COL_LENGTH(N'dbo.IndorRealtorInspectionUploadDrafts', N'AnalysisSummary') IS NULL
            ALTER TABLE dbo.IndorRealtorInspectionUploadDrafts ADD AnalysisSummary NVARCHAR(2000) NULL;
        """
    ];

    public static Task EnsureBridgeColumnsAsync(
        AppDbContext db,
        CancellationToken cancellationToken = default)
        => EnsureBridgeColumnsAsync(db, null, cancellationToken);

    public static async Task EnsureBridgeColumnsAsync(
        AppDbContext db,
        ILogger? logger,
        CancellationToken cancellationToken = default)
    {
        foreach (var sql in BridgeColumnStatements)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Realtor inspection bridge schema ensure step failed.");
            }
        }
    }
}
