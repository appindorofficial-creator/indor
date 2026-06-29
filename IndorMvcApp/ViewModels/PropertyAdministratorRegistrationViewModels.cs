namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorRegistrationState
{
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string PortfolioBusinessName { get; set; } = "";
    public bool TermsAccepted { get; set; }
    public bool MarketingOptIn { get; set; }
    public string PropertyCountRange { get; set; } = "";
    public string PortfolioType { get; set; } = "";
    public string OwnershipType { get; set; } = "";
    public string PrimaryMarket { get; set; } = "";
    public string ManagementStyle { get; set; } = "";
    public bool ToolMaintenanceRequests { get; set; }
    public bool ToolTurnoverCleaning { get; set; }
    public bool ToolGuestMessaging { get; set; }
    public bool ToolInvoicesPayments { get; set; }
    public bool ToolDocumentsWarranties { get; set; }
    public bool ToolServiceProviders { get; set; }
    public bool ToolTeamAccess { get; set; }
    public bool NotifyUrgentMaintenance { get; set; } = true;
    public bool NotifyWeeklySummary { get; set; } = true;
    public bool NotifyBookingLeaseUpdates { get; set; } = true;
}

public class PropertyAdministratorRegistrationStepViewModel
{
    public int Step { get; set; } = 1;
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 5;
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string BackUrl { get; set; } = "";
    public PropertyAdministratorRegistrationState State { get; set; } = new();
}

public class PropertyAdministratorProfileInput
{
    public string PortfolioBusinessName { get; set; } = "";
    public bool TermsAccepted { get; set; }
    public bool MarketingOptIn { get; set; }
}

public class PropertyAdministratorPortfolioInput
{
    public string PropertyCountRange { get; set; } = "";
    public string PortfolioType { get; set; } = "";
    public string OwnershipType { get; set; } = "";
    public string PrimaryMarket { get; set; } = "";
    public string ManagementStyle { get; set; } = "";
}

public class PropertyAdministratorPropertyInput
{
    public string PropertyName { get; set; } = "";
    public string? HouseNumber { get; set; }
    public string StreetName { get; set; } = "";
    public string? StreetAddress { get; set; }
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string? ZipCode { get; set; }
    public string Location { get; set; } = "";
    public string PropertyType { get; set; } = "";
}

public class PropertyAdministratorPropertyItemViewModel
{
    public int Id { get; set; }
    public int? PropiedadId { get; set; }
    public string PropertyName { get; set; } = "";
    public string Location { get; set; } = "";
    public string PropertyType { get; set; } = "";
    public string PropertyTypeLabel { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = "";
    public string StatusLabel { get; set; } = "Active";
    public string DetailUrl { get; set; } = "#";
    public string? OccupancyLabel { get; set; }
}

public class PropertyAdministratorPropertiesStepViewModel : PropertyAdministratorRegistrationStepViewModel
{
    public IReadOnlyList<PropertyAdministratorPropertyItemViewModel> Properties { get; set; } = [];
    public PropertyAdministratorPropertyInput? DraftProperty { get; set; }
    public string? FormError { get; set; }
    public string? FormSuccess { get; set; }
    public IReadOnlyList<string> ImportErrors { get; set; } = [];
    public bool CanUploadDocuments => Properties.Any(p => p.PropiedadId is > 0);
    public bool IsRegistrationComplete { get; set; }
    public string DoneUrl { get; set; } = "#";
}

public class PropertyAdministratorPortfolioImportResult
{
    public int ImportedCount { get; set; }
    public List<PropertyAdministratorPropertyInput> Properties { get; set; } = [];
    public List<string> Errors { get; set; } = [];
}

public class PropertyAdministratorToolsInput
{
    public bool ToolMaintenanceRequests { get; set; }
    public bool ToolTurnoverCleaning { get; set; }
    public bool ToolGuestMessaging { get; set; }
    public bool ToolInvoicesPayments { get; set; }
    public bool ToolDocumentsWarranties { get; set; }
    public bool ToolServiceProviders { get; set; }
    public bool ToolTeamAccess { get; set; }
    public bool NotifyUrgentMaintenance { get; set; }
    public bool NotifyWeeklySummary { get; set; }
    public bool NotifyBookingLeaseUpdates { get; set; }
}

public class PropertyAdministratorReviewViewModel : PropertyAdministratorRegistrationStepViewModel
{
    public int PropertyCount { get; set; }
    public string PortfolioTypeLabel { get; set; } = "";
    public string ManagementStyleLabel { get; set; } = "";
    public bool AccountCreated { get; set; }
    public bool PortfolioDetailsAdded { get; set; }
    public bool PropertiesAdded { get; set; }
    public bool ToolsSelected { get; set; }
}

public class PropertyAdministratorInviteTeamViewModel : PropertyAdministratorRegistrationStepViewModel
{
    public IReadOnlyList<string> PendingInvites { get; set; } = [];
    public string? FormError { get; set; }
    public string? FormSuccess { get; set; }
}

public class PropertyAdministratorImportPortfolioViewModel : PropertyAdministratorRegistrationStepViewModel
{
    public string? FormError { get; set; }
    public string? FormSuccess { get; set; }
}

public class PropertyAdministratorUploadDocumentsViewModel : PropertyAdministratorRegistrationStepViewModel
{
    public IReadOnlyList<string> UploadedFiles { get; set; } = [];
    public string? FormError { get; set; }
    public string? FormSuccess { get; set; }
}

public class PropertyAdministratorDashboardViewModel
{
    public string DisplayName { get; set; } = "";
    public string PortfolioTypeLabel { get; set; } = "";
    public string PrimaryMarket { get; set; } = "";
    public string ManagementStyleLabel { get; set; } = "";
    public int PropertyCount { get; set; }
    public IReadOnlyList<PropertyAdministratorPropertyItemViewModel> Properties { get; set; } = [];
    public IReadOnlyList<string> EnabledTools { get; set; } = [];
}
