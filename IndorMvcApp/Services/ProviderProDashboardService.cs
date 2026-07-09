using IndorMvcApp.Helpers;
using IndorMvcApp.Models;

using IndorMvcApp.ViewModels;



namespace IndorMvcApp.Services;



public class ProviderProDashboardService(IProviderProDataService dataService, IIndorLocalizer localizer)

{

    public async Task<ProviderProDashboardViewModel> BuildAsync(IndorProveedor proveedor, CancellationToken cancellationToken = default)

    {

        var companyName = !string.IsNullOrWhiteSpace(proveedor.DbaName)

            ? proveedor.DbaName

            : proveedor.BusinessName ?? proveedor.PrimaryContact ?? "Your company";



        var isApproved = string.Equals(proveedor.RegistrationStatus, ProviderRegistrationStatuses.Approved, StringComparison.OrdinalIgnoreCase);

        var isProActive = string.Equals(proveedor.RegistrationStatus, ProviderRegistrationStatuses.IndorProActive, StringComparison.OrdinalIgnoreCase)

            || isApproved

            || string.Equals(proveedor.RegistrationStatus, ProviderRegistrationStatuses.PendingReview, StringComparison.OrdinalIgnoreCase)

            || string.Equals(proveedor.RegistrationStatus, ProviderRegistrationStatuses.Submitted, StringComparison.OrdinalIgnoreCase);



        var score = proveedor.ExamPassed == true && proveedor.ExamScorePercent.HasValue

            ? proveedor.ExamScorePercent.Value

            : isProActive ? 82 : 0;



        var workspace = isProActive

            ? await dataService.GetWorkspaceDataAsync(proveedor.Id, includeLeads: isApproved, cancellationToken)

            : new ProviderProWorkspaceData();



        var model = new ProviderProDashboardViewModel

        {

            CompanyName = companyName,

            CompanyInitial = companyName.Trim().Length > 0 ? companyName.Trim()[0].ToString().ToUpperInvariant() : "P",

            Greeting = localizer.T(IndorGreeting.ForNow()),

            IsVerified = isApproved || (proveedor.ExamPassed == true && proveedor.Documentos.Count > 0),

            IsProActive = isProActive,

            ActivationPending = !isApproved,

            IsInsured = proveedor.IsInsured,

            ProviderScore = score,

            ScoreLabel = score >= 85
                ? localizer.T("Great Work!")
                : score >= 70
                    ? localizer.T("On Track")
                    : localizer.T("Getting Started"),

            ScoreSubtext = score >= 85
                ? localizer.T("Top 20% of providers")
                : localizer.T("Complete activation to unlock INDOR leads"),

            HomeRecordsThisMonth = workspace.HomeRecordsThisMonth,

            HomeRecordsDelta = workspace.HomeRecordsDelta,

            HomesProtected = workspace.HomesProtected,

            UnreadMessages = isProActive
                ? await dataService.GetUnreadMessageCountAsync(proveedor.Id, cancellationToken)
                : 0,

            AiSuggestions = BuildAiSuggestions(proveedor, isApproved, workspace),

            TodaysJobs = workspace.TodaysJobs,

            NewLeads = workspace.NewLeads,

            PendingEstimates = workspace.PendingEstimates,

            PendingApprovals = workspace.PendingApprovals,

            Payments = workspace.Payments,

            UpcomingCalendar = workspace.UpcomingCalendar

        };



        model.Metrics = new ProviderProMetricsViewModel

        {

            JobsToday = workspace.TodaysJobs.Count,

            NewLeads = workspace.NewLeadsCount,

            PendingEstimates = workspace.PendingEstimatesCount,

            PaymentsDue = workspace.Payments.Overdue

        };

        model.ShowEstimateTutorial = workspace.PendingEstimatesCount > 0;
        model.EstimateWizardSteps =
        [
            new() { Number = 1, Label = "Home", IsCurrent = true },
            new() { Number = 2, Label = "Pending Estimates" },
            new() { Number = 3, Label = "Estimate Review" },
            new() { Number = 4, Label = "Edit Estimate" },
            new() { Number = 5, Label = "Send Estimate" }
        ];

        return model;

    }

    private static List<string> BuildAiSuggestions(

        IndorProveedor proveedor,

        bool isApproved,

        ProviderProWorkspaceData workspace)

    {

        var suggestions = new List<string>();



        if (!isApproved)

        {

            suggestions.Add("Complete your activation call to start receiving INDOR leads.");

        }



        if (proveedor.ExamPassed != true)

        {

            suggestions.Add("Take your trade assessment when you're ready to activate for INDOR jobs.");

        }



        if (proveedor.Documentos.Count == 0)

        {

            suggestions.Add("Upload verification documents to speed up provider approval.");

        }



        if (workspace.PendingApprovals.Count > 0)

        {

            suggestions.Add($"{workspace.PendingApprovals.Count} homeowner approval{(workspace.PendingApprovals.Count == 1 ? "" : "s")} waiting for your review.");

        }



        if (workspace.PendingEstimatesCount > 0)

        {

            suggestions.Add($"Follow up on {workspace.PendingEstimatesCount} open estimate{(workspace.PendingEstimatesCount == 1 ? "" : "s")} from the last 7 days.");

        }



        if (workspace.TodaysJobs.Count > 0)

        {

            suggestions.Add($"Review today's {workspace.TodaysJobs.Count} scheduled job{(workspace.TodaysJobs.Count == 1 ? "" : "s")} before heading out.");

        }



        if (suggestions.Count == 0)

        {

            suggestions.Add("Your dashboard is up to date. Check back for new INDOR leads.");

        }



        return suggestions.Take(3).ToList();

    }

}


