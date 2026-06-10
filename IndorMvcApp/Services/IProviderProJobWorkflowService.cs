using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IProviderProJobWorkflowService
{
    Task<ProviderProJobsScheduleViewModel> GetJobsScheduleAsync(IndorProveedor proveedor, string? view = "today", CancellationToken cancellationToken = default);
    Task<ProviderProCreateJobCategoriesViewModel> GetCreateJobCategoriesAsync(IndorProveedor proveedor, CancellationToken cancellationToken = default);
    Task<ProviderProCreateJobDetailsViewModel?> GetCreateJobDetailsAsync(IndorProveedor proveedor, string categoryId, ProviderProCreateJobDraft? draft, CancellationToken cancellationToken = default);
    Task<ProviderProCreateJobScheduleViewModel?> GetCreateJobScheduleAsync(IndorProveedor proveedor, ProviderProCreateJobDraft draft, CancellationToken cancellationToken = default);
    Task<ProviderProCreateJobReviewViewModel?> GetCreateJobReviewAsync(IndorProveedor proveedor, ProviderProCreateJobDraft draft);
    Task<ProviderProCreateJobSuccessViewModel?> GetCreateJobSuccessAsync(IndorProveedor proveedor, int jobId, CancellationToken cancellationToken = default);
    Task<int> CreateJobAsync(int proveedorId, ProviderProCreateJobInput input, CancellationToken cancellationToken = default);
    Task<ProviderProJobDetailsViewModel?> GetJobDetailsAsync(IndorProveedor proveedor, int jobId, bool fromCalendar = false, string? calendarDate = null, CancellationToken cancellationToken = default);
    Task<bool> StartJobAsync(int proveedorId, int jobId, CancellationToken cancellationToken = default);
    Task<ProviderProActiveJobViewModel?> GetActiveJobAsync(IndorProveedor proveedor, int jobId, CancellationToken cancellationToken = default);
    Task<bool> CompleteJobAsync(int proveedorId, int jobId, CancellationToken cancellationToken = default);
    Task<ProviderProJobCompletionReportViewModel?> GetJobReportAsync(IndorProveedor proveedor, int jobId, CancellationToken cancellationToken = default);
    Task<ProviderProCalendarOverviewViewModel> GetCalendarOverviewAsync(IndorProveedor proveedor, string? view = "week", string? filter = "all", string? weekStart = null, CancellationToken cancellationToken = default);
    Task<ProviderProDayScheduleViewModel> GetDayScheduleAsync(IndorProveedor proveedor, string? date = null, CancellationToken cancellationToken = default);
    Task<ProviderProRescheduleJobViewModel?> GetRescheduleJobAsync(IndorProveedor proveedor, int jobId, CancellationToken cancellationToken = default);
    Task<bool> RescheduleJobAsync(int proveedorId, ProviderProRescheduleJobInput input, CancellationToken cancellationToken = default);
    Task<ProviderProCalendarUpdatedViewModel?> GetCalendarUpdatedAsync(IndorProveedor proveedor, int jobId, CancellationToken cancellationToken = default);
}
