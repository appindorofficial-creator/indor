namespace IndorMvcApp.Models;

/// <summary>
/// Provider-entered document metadata stored inside <see cref="ProviderOnboardingMeta.ProfileDocuments"/>.
/// </summary>
public class ProviderProfileDocumentMeta
{
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
