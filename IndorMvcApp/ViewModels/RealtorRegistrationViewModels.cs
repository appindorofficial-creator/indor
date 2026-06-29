namespace IndorMvcApp.ViewModels;

public class RealtorRegistrationState
{
    public string BrokerageName { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public string LicenseState { get; set; } = "";
    public string ServiceAreas { get; set; } = "";
    public string OfficeAddress { get; set; } = "";
    public string Languages { get; set; } = "";
    public bool ProfessionalTermsAccepted { get; set; }
    public bool VerificationSkipped { get; set; }
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
}

public class RealtorRegistrationStepViewModel
{
    public int Step { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public int DisplayStep { get; set; } = 2;
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string BackUrl { get; set; } = "";
    public RealtorRegistrationState State { get; set; } = new();
    public IReadOnlyList<string> LicenseStates { get; set; } = [];
    public IReadOnlyList<string> SupportedLanguages { get; set; } = [];
}

public class RealtorDocumentSlotViewModel
{
    public string DocumentType { get; set; } = "";
    public string Label { get; set; } = "";
    public bool Required { get; set; }
    public bool Uploaded { get; set; }
    public string? FileUrl { get; set; }
}

public class RealtorReadyViewModel
{
    public int Step { get; set; } = 4;
    public int TotalSteps { get; set; } = 4;
    public string BadgeLabel { get; set; } = "Realtor Basic";
    public bool LicenseNumberSaved { get; set; }
    public bool ProfileCreated { get; set; }
    public bool LicensePhotoUploaded { get; set; }
    public bool GovernmentIdUploaded { get; set; }
    public bool CanUpgradeToVerified { get; set; }
}
