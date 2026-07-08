using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

/// <summary>
/// "My Requests" — the job poster's view of the network jobs they posted:
/// tracking quotes, comparing them, hiring a pro, and the confirmation.
/// Reads posted jobs and writes quotes + hires to the database.
/// </summary>
public interface INetworkRequestsService
{
    Task<MyRequestsViewModel> GetMyRequestsAsync(IndorProveedor me, string? tab, string? query, CancellationToken cancellationToken = default);

    Task<RequestDetailsViewModel?> GetDetailsAsync(IndorProveedor me, int jobId, CancellationToken cancellationToken = default);

    Task<CompareQuotesViewModel?> GetCompareAsync(IndorProveedor me, int jobId, string? sort, CancellationToken cancellationToken = default);

    Task<bool> SelectQuoteAsync(IndorProveedor me, int jobId, int quoteId, CancellationToken cancellationToken = default);

    Task<RequestConfirmedViewModel?> GetConfirmedAsync(IndorProveedor me, int jobId, CancellationToken cancellationToken = default);

    // ---- Invite to Job (direct invitation to a specific subcontractor) ----

    Task<InviteToJobViewModel?> GetInviteAsync(IndorProveedor me, int subcontractorId, CancellationToken cancellationToken = default);

    Task<int?> SaveInviteAsync(IndorProveedor me, InviteToJobInput input, List<string> newAttachmentUrls, CancellationToken cancellationToken = default);

    Task<InvitationSentViewModel?> GetInvitationSentAsync(IndorProveedor me, int invitationId, CancellationToken cancellationToken = default);
}
