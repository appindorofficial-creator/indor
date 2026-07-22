using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

/// <summary>
/// Reads and writes for the Contractor Network (subcontractor marketplace):
/// finding verified subcontractors, viewing profiles, posting jobs, and hiring.
/// </summary>
public interface IProviderNetworkService
{
    Task<NetworkHomeViewModel> GetHomeAsync(IndorProveedor me, CancellationToken cancellationToken = default);

    Task<FindSubcontractorsViewModel> GetFindAsync(
        IndorProveedor me,
        string? query,
        string? trade,
        string? view,
        bool nearby,
        bool insuredOnly,
        bool availableNow,
        bool docsReady,
        string? mode = null,
        CancellationToken cancellationToken = default);

    Task<SubcontractorProfileViewModel?> GetProfileAsync(
        IndorProveedor me,
        int subcontractorId,
        CancellationToken cancellationToken = default);

    // ---- Post a Job wizard (draft persisted in the database) ----

    Task<PostJobDetailsViewModel> GetDetailsAsync(IndorProveedor me, int? draftId, CancellationToken cancellationToken = default);

    Task<int> SaveDetailsAsync(
        int posterProveedorId,
        PostJobDetailsInput input,
        List<string> newPhotoUrls,
        CancellationToken cancellationToken = default);

    Task<PostJobLocationViewModel?> GetLocationAsync(IndorProveedor me, int draftId, CancellationToken cancellationToken = default);

    Task<bool> SaveLocationAsync(IndorProveedor me, PostJobLocationInput input, CancellationToken cancellationToken = default);

    Task<PostJobReviewViewModel?> GetReviewAsync(IndorProveedor me, int draftId, CancellationToken cancellationToken = default);

    Task<int?> PublishJobAsync(IndorProveedor me, int draftId, CancellationToken cancellationToken = default);

    Task<NetworkJobPostedViewModel?> GetJobPostedAsync(
        IndorProveedor me,
        int jobId,
        CancellationToken cancellationToken = default);

    Task<HireSubcontractorViewModel?> GetHireAsync(
        IndorProveedor me,
        int subcontractorId,
        int? jobId,
        CancellationToken cancellationToken = default);

    Task<int?> ConfirmHireAsync(
        int hirerProveedorId,
        ConfirmHireInput input,
        CancellationToken cancellationToken = default);

    Task<NetworkHireConfirmedViewModel?> GetHireConfirmedAsync(
        IndorProveedor me,
        int hireId,
        CancellationToken cancellationToken = default);

    Task<bool> ToggleSaveAsync(int ownerProveedorId, int subcontractorId, CancellationToken cancellationToken = default);

    Task<MessageSubcontractorViewModel?> GetMessageComposeAsync(
        IndorProveedor me,
        int subcontractorId,
        CancellationToken cancellationToken = default);

    Task<MessageSubcontractorSentViewModel?> SendMessageAsync(
        IndorProveedor me,
        MessageSubcontractorInput input,
        CancellationToken cancellationToken = default);
}
