using Microsoft.AspNetCore.Http;

namespace IndorMvcApp.ViewModels;

/// <summary>Subcontractor header shown atop the invite + confirmation screens.</summary>
public sealed class InviteSubHeaderViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? TradeLabel { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string IconTone { get; init; } = "blue";
    public string? PhotoUrl { get; init; }
    public string? LocationLabel { get; init; }
    public string? DistanceLabel { get; init; }
    public bool IsVerified { get; init; }
    public bool IsInsured { get; init; }
    public bool IsDocsReady { get; init; }
    public bool IsAvailableNow { get; init; }
    public string AvailabilityLabel { get; init; } = "Contact for availability";
}

/// <summary>Screen — Invite to Job (compose a direct request to a subcontractor).</summary>
public sealed class InviteToJobViewModel
{
    public required InviteSubHeaderViewModel Sub { get; init; }

    public string? JobTitle { get; set; }
    public string? TradeId { get; set; }
    public string? ServiceCategory { get; set; }
    public string? PropertyAddress { get; set; }
    public bool ScheduleToday { get; set; } = true;
    public string? ScheduleDate { get; set; }
    public string? BudgetRange { get; set; }
    public string? Description { get; set; }
    public string TimingPreference { get; set; } = IndorMvcApp.Models.NetworkInvitationTimings.Urgent;

    public List<string> Attachments { get; init; } = [];
    public IReadOnlyList<string> BudgetOptions => PostJobOptions.Budgets;
    public string? ErrorMessage { get; set; }
}

public sealed class InviteToJobInput
{
    public int SubcontractorId { get; set; }
    public string? JobTitle { get; set; }
    public string? TradeId { get; set; }
    public string? ServiceCategory { get; set; }
    public string? PropertyAddress { get; set; }
    public bool ScheduleToday { get; set; } = true;
    public string? ScheduleDate { get; set; }
    public string? BudgetRange { get; set; }
    public string? Description { get; set; }
    public string? TimingPreference { get; set; }
    public List<string>? ExistingAttachments { get; set; }
    public IFormFileCollection? Attachments { get; set; }
    /// <summary>"draft" to save and exit, otherwise send.</summary>
    public string? Mode { get; set; }
}

/// <summary>Screen — Invitation Sent (success + tracking).</summary>
public sealed class InvitationSentViewModel
{
    public int Id { get; init; }
    public required InviteSubHeaderViewModel Sub { get; init; }

    public string JobTitle { get; init; } = "";
    public string ScheduleLabel { get; init; } = "";
    public string BudgetRange { get; init; } = "";

    public List<RequestStepViewModel> Steps { get; init; } = [];
}
