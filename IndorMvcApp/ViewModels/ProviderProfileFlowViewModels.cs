using IndorMvcApp.Validation;

namespace IndorMvcApp.ViewModels;

public sealed class ProviderProfileCompletionViewModel : ProviderProPageBaseViewModel
{
    public string CompanyInitial { get; init; } = "P";
    public string? LogoUrl { get; init; }
    public string LocationLabel { get; init; } = "";
    public string ServiceAreaSummary { get; init; } = "";
    public string DisplayBusinessName { get; init; } = "";
    public int CompletedSections { get; init; }
    public int TotalSections { get; init; } = 7;
    public int ProgressPercent => TotalSections == 0 ? 0 : (int)Math.Round(100.0 * CompletedSections / TotalSections);
    public List<ProviderProfileSectionViewModel> Sections { get; init; } = [];
    public string? ContinueAction { get; init; }
    public string? ContinueLabel { get; init; }
    public string? NextStepTitle { get; init; }
    public string? NextStepAction { get; init; }
}

public sealed class ProviderProfileSectionViewModel
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string StatusLabel { get; init; }
    /// <summary>complete | incomplete | pending | missing</summary>
    public required string StatusKind { get; init; }
    public required string ActionLabel { get; init; }
    public required string Action { get; init; }
    public required string IconClass { get; init; }
    public bool IsComplete { get; init; }
}

public sealed class ProviderProfileBusinessViewModel : ProviderProPageBaseViewModel
{
    public string CompanyInitial { get; init; } = "P";
    public string? LogoUrl { get; init; }
    public string BusinessName { get; set; } = "";
    public string? PrimaryCategoryId { get; set; }
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Website { get; set; }
    public bool WebsiteNotApplicable { get; set; }
    public string ServiceDescription { get; set; } = "";
    public string PreferredHours { get; set; } = "";
    public bool EmergencyService { get; set; } = true;
    public bool SameDayJobs { get; set; } = true;
    public string? EmergencyPreference { get; set; }
    public string? SameDayPreference { get; set; }
    public string PrimaryCity { get; set; } = "";
    public string ServiceZipCodes { get; set; } = "";
    public List<ProviderProEditProfileServiceOptionViewModel> ServiceOptions { get; init; } = [];
    public List<ProviderProfileCategoryOptionViewModel> CategoryOptions { get; init; } = [];
    public string? ErrorMessage { get; set; }
}

public sealed class ProviderProfileCategoryOptionViewModel
{
    public required string Id { get; init; }
    public required string Label { get; init; }
}

public sealed class ProviderProfileBusinessInput
{
    public string? BusinessName { get; set; }
    public string? PrimaryCategoryId { get; set; }

    [UsPhoneOptional]
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool WebsiteNotApplicable { get; set; }
    public string? ServiceDescription { get; set; }
    public string? PreferredHours { get; set; }
    public string? EmergencyPreference { get; set; }
    public string? SameDayPreference { get; set; }
    public string? PrimaryCity { get; set; }
    public string? ServiceZipCodes { get; set; }
    public string[]? ServiceIds { get; set; }
    public string? SaveMode { get; set; }
}

public sealed class ProviderProfileDocumentsViewModel : ProviderProPageBaseViewModel
{
    public string CompanyInitial { get; init; } = "P";
    public int CompletedDocuments { get; init; }
    public int TotalDocuments { get; init; } = 4;
    public int ProgressPercent => TotalDocuments == 0 ? 0 : (int)Math.Round(100.0 * CompletedDocuments / TotalDocuments);
    public List<ProviderProfileDocumentSectionViewModel> Sections { get; init; } = [];
    public string? ActiveSectionId { get; init; }
    public string? ContinueAction { get; init; }
    public string? ErrorMessage { get; set; }
}

public sealed class ProviderProfileDocumentSectionViewModel
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string IconClass { get; init; }
    public required string DocumentType { get; init; }
    public required string StatusLabel { get; init; }
    public required string StatusKind { get; init; }
    public required string ActionLabel { get; init; }
    public bool IsComplete { get; init; }
    public bool IsExpanded { get; init; }
    public bool IsUploaded { get; init; }
    public string? FileUrl { get; init; }
    public bool NotApplicable { get; init; }
    public bool Unknown { get; init; }
    public Dictionary<string, string?> Fields { get; init; } = [];
}

public sealed class ProviderProfileDocumentsInput
{
    public string? Section { get; set; }
    public string? SaveMode { get; set; }

    public string? LicenseNumber { get; set; }
    public string? LicenseType { get; set; }
    public string? LicenseState { get; set; }
    public string? LicenseExpiry { get; set; }
    public bool LicenseNotApplicable { get; set; }
    public bool LicenseUnknown { get; set; }

    public string? InsuranceCompany { get; set; }
    public string? PolicyNumber { get; set; }
    public string? CoverageAmount { get; set; }
    public string? InsuranceExpiry { get; set; }
    public bool InsuranceNotApplicable { get; set; }
    public bool InsuranceUnknown { get; set; }

    public string? W9LegalName { get; set; }
    public string? W9DbaName { get; set; }
    public string? W9TaxClassification { get; set; }
    public string? W9Ein { get; set; }
    public bool W9NotApplicable { get; set; }
    public bool W9Unknown { get; set; }

    public string? BackgroundFullName { get; set; }
    public string? BackgroundDob { get; set; }
    public string? BackgroundSsnLast4 { get; set; }
    public string? BackgroundState { get; set; }
    public bool BackgroundConsent { get; set; }
}
