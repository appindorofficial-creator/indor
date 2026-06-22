namespace IndorMvcApp.ViewModels;

public class RealtorInviteStepViewModel
{
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public string Title { get; set; } = "Invite Client";
    public string Subtitle { get; set; } = "";
    public string CancelUrl { get; set; } = "/Realtor/Clients";
}

public class RealtorInviteClientInfoViewModel : RealtorInviteStepViewModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ClientRole { get; set; } = "Buyer";
    public string QuickNote { get; set; } = "";
    public IReadOnlyList<string> ClientRoles { get; set; } = [];
}

public class RealtorInvitePropertyOptionViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string Label { get; set; } = "";
    public string CityRegion { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string Icon { get; set; } = "fa-house";
}

public class RealtorInvitePropertyViewModel : RealtorInviteStepViewModel
{
    public string? SearchQuery { get; set; }
    public int? SelectedPropertyFileId { get; set; }
    public List<RealtorInvitePropertyOptionViewModel> Properties { get; set; } = [];
}

public class RealtorInviteCreatePropertyViewModel : RealtorInviteStepViewModel
{
    public string Address { get; set; } = "";
    public string Unit { get; set; } = "";
    public string City { get; set; } = "";
    public string StateCode { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Nickname { get; set; } = "";
    public string PropertyType { get; set; } = "Single-family";
    public bool SelectForClient { get; set; } = true;
    public IReadOnlyList<string> States { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string Icon)> PropertyTypes { get; set; } = [];
}

public class RealtorInviteAccessViewModel : RealtorInviteStepViewModel
{
    public bool AccessPropertyOverview { get; set; } = true;
    public bool AccessFilesReports { get; set; } = true;
    public bool AccessQuotesEstimates { get; set; } = true;
    public bool AccessMessages { get; set; } = true;
    public bool AccessProjectUpdates { get; set; } = true;
    public bool AccessPayments { get; set; }
    public string CollaborationLevel { get; set; } = "CanComment";
    public bool DeliveryEmail { get; set; } = true;
    public bool DeliveryText { get; set; }
    public string WelcomeMessage { get; set; } = "";
    public IReadOnlyList<(string Value, string Label, string Icon)> CollaborationOptions { get; set; } = [];
}

public class RealtorInviteReviewViewModel : RealtorInviteStepViewModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ClientRole { get; set; } = "";
    public string PropertyDisplay { get; set; } = "";
    public string AccessSummary { get; set; } = "";
    public string CollaborationLabel { get; set; } = "";
    public string DeliveryLabel { get; set; } = "";
    public string WelcomeMessage { get; set; } = "";
    public bool SendReminder48h { get; set; } = true;
}

public class RealtorInviteSuccessViewModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string PropertyDisplay { get; set; } = "";
    public string StatusLabel { get; set; } = "Pending Acceptance";
    public string InviteLink { get; set; } = "";
    public List<RealtorInvitationCardViewModel> PendingInvitations { get; set; } = [];
}
