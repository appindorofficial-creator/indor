namespace IndorMvcApp.ViewModels;

/// <summary>A single verification checklist item (License, Insurance, W-9, Background, Profile).</summary>
public sealed class VerificationItemViewModel
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public string IconClass { get; init; } = "fa-circle-check";
    /// <summary>ok | warn | pending</summary>
    public string State { get; init; } = "pending";
    public required string StatusLabel { get; init; }
    /// <summary>Secondary line, e.g. "License #PL-48219" or "General Liability Active".</summary>
    public string? Detail { get; init; }
    /// <summary>Right-aligned meta, e.g. "expires 11/24/2027".</summary>
    public string? Meta { get; init; }
}

/// <summary>A contractor row in the verification queue.</summary>
public sealed class VerificationQueueCardViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? TradeLabel { get; init; }
    public string? LocationLabel { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string? PhotoUrl { get; init; }

    public string PillLabel { get; init; } = "Pending Review";
    /// <summary>pending | warn | ready</summary>
    public string PillKind { get; init; } = "pending";

    public List<VerificationItemViewModel> Items { get; init; } = [];
    public int ItemsComplete { get; init; }
    public int TotalItems { get; init; } = 5;
    public int ProgressPercent => TotalItems == 0 ? 0 : (int)Math.Round(100.0 * ItemsComplete / TotalItems);
}

public sealed class VerificationTabViewModel
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public int Count { get; init; }
}

/// <summary>Screen 1 — Verify Contractors queue.</summary>
public sealed class VerificationQueueViewModel
{
    public string ActiveTab { get; init; } = "pending";
    public string? Query { get; init; }
    public List<VerificationTabViewModel> Tabs { get; init; } = [];
    public List<VerificationQueueCardViewModel> Contractors { get; init; } = [];
}

/// <summary>Screen 2 — Contractor Verification detail.</summary>
public sealed class ContractorVerificationViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? TradeLabel { get; init; }
    public string? LocationLabel { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string? PhotoUrl { get; init; }

    public string? ContactPerson { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string SubmittedLabel { get; init; } = "";

    public string StatusLabel { get; init; } = "In Review";
    public string StatusKind { get; init; } = "review";

    public List<VerificationItemViewModel> Items { get; init; } = [];
    public int ItemsComplete { get; init; }
    public int TotalItems { get; init; } = 5;
    public bool IsReady => ItemsComplete >= TotalItems;

    public string? OperatorNotes { get; init; }
    public string? NotesSavedLabel { get; init; }
    public List<string> FollowUps { get; init; } = [];
}

/// <summary>Screen 3 — Verification Complete.</summary>
public sealed class VerificationCompleteViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? TradeLabel { get; init; }
    public string? LocationLabel { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";

    public List<VerificationItemViewModel> Items { get; init; } = [];
    public int ItemsComplete { get; init; }
    public int TotalItems { get; init; } = 5;
    public bool IsReady => ItemsComplete >= TotalItems;
    public bool AlreadyApproved { get; init; }
}
