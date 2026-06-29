using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IRealtorInviteClientService
{
    Task<IndorRealtorInvitation> EnsureDraftAsync(CancellationToken cancellationToken = default);
    Task<IndorRealtorInvitation?> GetDraftAsync(CancellationToken cancellationToken = default);
    Task CancelDraftAsync(CancellationToken cancellationToken = default);
    string ResolveResumeAction(int currentStep);

    Task<RealtorInviteClientInfoViewModel> BuildClientInfoAsync(CancellationToken cancellationToken = default);
    Task SaveClientInfoAsync(string fullName, string email, string? phone, string clientRole, string? quickNote, CancellationToken cancellationToken = default);

    Task<RealtorInvitePropertyViewModel> BuildPropertyAsync(string? search, CancellationToken cancellationToken = default);
    Task SavePropertyAsync(int? propertyFileId, CancellationToken cancellationToken = default);

    Task<RealtorInviteCreatePropertyViewModel> BuildCreatePropertyAsync(string? prefillAddress = null, CancellationToken cancellationToken = default);
    Task<int> CreatePropertyAsync(RealtorInviteCreatePropertyViewModel model, CancellationToken cancellationToken = default);

    Task<RealtorInviteAccessViewModel> BuildAccessAsync(CancellationToken cancellationToken = default);
    Task SaveAccessAsync(RealtorInviteAccessViewModel model, CancellationToken cancellationToken = default);

    Task<RealtorInviteReviewViewModel> BuildReviewAsync(CancellationToken cancellationToken = default);
    Task<int> SendInvitationAsync(bool sendReminder48h, CancellationToken cancellationToken = default);
    Task<RealtorInviteSuccessViewModel> BuildSuccessAsync(int invitationId, CancellationToken cancellationToken = default);

    Task<RealtorInvitePublicViewModel?> GetPublicInvitationAsync(Guid token, CancellationToken cancellationToken = default);
    Task<RealtorInvitePublicViewModel?> AcceptInvitationAsync(Guid token, CancellationToken cancellationToken = default);
}
