using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

/// <summary>
/// Operator "Verify Contractors" console. Reads real contractor data
/// (license, insurance, documents, profile completeness) and writes the
/// operator's review decisions to <c>IndorProveedorVerificaciones</c>.
/// </summary>
public interface IContractorVerificationService
{
    Task<VerificationQueueViewModel> GetQueueAsync(IndorProveedor me, string? tab, string? query, CancellationToken cancellationToken = default);

    Task<ContractorVerificationViewModel?> GetDetailAsync(IndorProveedor me, int contractorId, CancellationToken cancellationToken = default);

    Task<bool> SaveReviewAsync(IndorProveedor me, int contractorId, string? operatorNotes, string? mode, CancellationToken cancellationToken = default);

    Task<VerificationCompleteViewModel?> GetCompleteAsync(IndorProveedor me, int contractorId, CancellationToken cancellationToken = default);

    Task<bool> ApproveAsync(IndorProveedor me, int contractorId, CancellationToken cancellationToken = default);
}
