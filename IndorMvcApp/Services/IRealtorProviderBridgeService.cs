using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public interface IRealtorProviderBridgeService
{
    Task<List<IndorProveedor>> MatchProveedoresForTradeAsync(string trade, CancellationToken cancellationToken = default);

    Task<IndorProveedorLead> CreateLeadFromRealtorQuoteAsync(
        IndorRealtorQuote quote,
        IndorProveedor proveedor,
        IReadOnlyList<IndorRealtorInspectionUploadFinding> tradeFindings,
        string? inspectionReportUrl,
        CancellationToken cancellationToken = default);

    Task SyncBidFromEstimateAsync(IndorProveedorEstimate estimate, CancellationToken cancellationToken = default);
}
