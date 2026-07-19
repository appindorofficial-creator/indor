namespace IndorMvcApp.Models;

public class ProviderRegistrationState
{
    public const int TotalSteps = 5;
    public const string ElectricalCategoryId = "electrical";
    public const string PlumbingCategoryId = "plumbing";
    public const string HvacCategoryId = "hvac";
    public const string HandymanCategoryId = "handyman";
    public const string ConstructionCategoryId = "construction";
    public const string BathroomCategoryId = "bathroom";
    public const string KitchenCategoryId = "kitchen";
    public const string RoofingCategoryId = "roofing";
    public const string PaintingCategoryId = "painting";
    public const string FlooringCategoryId = "flooring";
    public const string CleaningCategoryId = "cleaning";
    public const string LandscapingCategoryId = "landscaping";
    public const string PestCategoryId = "pest";
    public const string ApplianceCategoryId = "appliance";
    public const int ExamPassingPercent = 70;

    public List<string> SelectedCategoryIds { get; set; } = [];
    public List<string> SelectedServiceIds { get; set; } = [];

    public string ProviderType { get; set; } = "Company";
    public string BusinessName { get; set; } = "";
    public string DbaName { get; set; } = "";
    public string PrimaryContact { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string YearsExperience { get; set; } = "";
    public List<string> Languages { get; set; } = ["English"];
    public string? LicenseNumber { get; set; }
    public string PrimaryCity { get; set; } = "Charlotte, NC";
    public int TravelRadiusMiles { get; set; } = 25;
    public List<string> ZipOrNeighborhoods { get; set; } = ["28202", "28203", "28205"];
    public bool EmergencyService { get; set; } = true;
    public bool SameDayJobs { get; set; } = true;
    public List<string> AvailableDays { get; set; } = ["Mon", "Tue", "Wed", "Thu", "Fri"];
    public string PreferredHours { get; set; } = "8:00 AM – 6:00 PM";
    public List<string> JobSizes { get; set; } = ["small", "standard", "large"];
    public bool LogoUploaded { get; set; }
    public bool ScopeTradeUnderstood { get; set; }
    public bool ScopeStandardsAgreed { get; set; }
    public Dictionary<int, string> ExamAnswers { get; set; } = new();
    public int ExamScorePercent { get; set; }
    public bool? ExamPassed { get; set; }
    public bool ProfileSubmitted { get; set; }
    public bool SubmitConfirmed { get; set; }
    public string BusinessAddress { get; set; } = "";
    public string? EpaCertificationNumber { get; set; }
    public bool BackgroundCheckConsent { get; set; }
    public bool ExamIntroAcknowledged { get; set; }
    public string ServiceDescription { get; set; } = "";
    public bool IsInsured { get; set; }
    public bool IsLicensed { get; set; }
    public string TeamSize { get; set; } = "";

    public string ServiceZipCodes { get; set; } = "";

    public string OnboardingPath { get; set; } = "";

    /// <summary>Company or Independent — entry choice for provider counts.</summary>
    public string OrganizationKind { get; set; } = "";

    public bool AssessmentSkipped { get; set; }

    public bool AssessmentStarted { get; set; }

    public bool TermsAccepted { get; set; }

    public string? Website { get; set; }

    public string? EinNumber { get; set; }

    public string? ActivationCallSlot { get; set; }

    public bool ActivationCallScheduled { get; set; }

    public bool IndorProActive { get; set; }

    public bool UsesNewWizard { get; set; } = true;

    public bool ExamIsMandatory => AssessmentStarted && !AssessmentSkipped;

    public string ServiceZipCodesDisplay =>
        !string.IsNullOrWhiteSpace(ServiceZipCodes)
            ? ServiceZipCodes
            : ZipOrNeighborhoods.Count > 0 ? string.Join(", ", ZipOrNeighborhoods) : "";

    public bool IsSingleTrade =>
        SelectedCategoryIds.Count == 1;

    public string? PrimaryTradeId =>
        SelectedCategoryIds.Count == 1 ? SelectedCategoryIds[0] : null;

    public bool IsPlumbingOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(PlumbingCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsHvacOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(HvacCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsHandymanOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(HandymanCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsConstructionOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(ConstructionCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsBathroomOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(BathroomCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsKitchenOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(KitchenCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsRoofingOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(RoofingCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsPaintingOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(PaintingCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsFlooringOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(FlooringCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsCleaningOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(CleaningCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsLandscapingOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(LandscapingCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsPestOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(PestCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool IsApplianceOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(ApplianceCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool UsesServicesFirstFlow => IsHvacOnly || IsHandymanOnly || IsBathroomOnly;

    public bool UsesExamIntroFlow =>
        IsHvacOnly || IsHandymanOnly || IsConstructionOnly || IsBathroomOnly || IsKitchenOnly || IsRoofingOnly || IsPaintingOnly || IsFlooringOnly || IsCleaningOnly || IsLandscapingOnly || IsPestOnly || IsApplianceOnly;

    public bool UsesBusinessBeforeServicesFlow =>
        IsPlumbingOnly || IsConstructionOnly || IsKitchenOnly || IsRoofingOnly || IsPaintingOnly || IsFlooringOnly || IsCleaningOnly || IsLandscapingOnly || IsPestOnly || IsApplianceOnly;

    public bool UsesServicesBeforeExamIntro =>
        IsConstructionOnly || IsKitchenOnly || IsRoofingOnly || IsPaintingOnly || IsFlooringOnly || IsCleaningOnly || IsLandscapingOnly || IsPestOnly || IsApplianceOnly;

    public bool IsElectricianOnly =>
        SelectedCategoryIds.Count == 1 &&
        SelectedCategoryIds[0].Equals(ElectricalCategoryId, StringComparison.OrdinalIgnoreCase);

    public bool SelectedIncludesElectrical =>
        SelectedCategoryIds.Any(id =>
            id.Equals(ElectricalCategoryId, StringComparison.OrdinalIgnoreCase));

    public string DisplayCompanyName =>
        !string.IsNullOrWhiteSpace(DbaName) ? DbaName :
        !string.IsNullOrWhiteSpace(BusinessName) ? BusinessName : PrimaryContact;
}

public record OnboardingOption(string Id, string Label, string IconClass);

public record ExamQuestion(int Number, string Text, IReadOnlyList<string> Options, int CorrectIndex);
