namespace IndorMvcApp.Models;

public static class ProviderDocumentTypes
{
    public const string License = "license";
    public const string Insurance = "insurance";
    public const string Logo = "logo";
    public const string PhotoId = "photo_id";
    public const string BusinessRegistration = "business_registration";
    public const string W9 = "w9";
    public const string TradeCerts = "trade_certs";
    public const string Portfolio = "portfolio";
    public const string PlumbingLicense = "plumbing_license";
    public const string HvacLicense = "hvac_license";
    public const string EpaCertification = "epa_certification";
    public const string LiabilityInsurance = "liability_insurance";
    public const string GovernmentId = "government_id";
    public const string References = "references";
    public const string WorkPhotos = "work_photos";
    public const string ContractorLicense = "contractor_license";
    public const string RoofingLicense = "roofing_license";

    public static readonly IReadOnlyList<(string Type, string Label, bool Required)> DefaultSlots =
    [
        (License, "Trade license", true),
        (Insurance, "Proof of insurance", true),
        (Logo, "Company logo (optional)", false),
    ];

    public static readonly IReadOnlyList<(string Type, string Label, bool Required)> PlumbingSlots =
    [
        (PlumbingLicense, "Plumbing license", true),
        (PhotoId, "Photo ID", true),
        (Insurance, "Certificate of insurance", true),
        (BusinessRegistration, "Business registration", true),
        (W9, "W-9 form", true),
        (TradeCerts, "Trade certifications (optional)", false),
        (Portfolio, "Portfolio photos (optional)", false),
    ];

    public static readonly IReadOnlyList<(string Type, string Label, bool Required)> HvacSlots =
    [
        (HvacLicense, "HVAC contractor license", true),
        (EpaCertification, "EPA certification", true),
        (LiabilityInsurance, "General liability insurance", true),
        (BusinessRegistration, "Business registration", true),
        (GovernmentId, "Government ID", true),
        (W9, "W-9 form", true),
    ];

    public static readonly IReadOnlyList<(string Type, string Label, bool Required)> RoofingSlots =
    [
        (RoofingLicense, "Roofing license", true),
        (LiabilityInsurance, "General liability insurance", true),
        (GovernmentId, "Government ID", true),
        (BusinessRegistration, "Business registration or W-9", true),
        (Portfolio, "Project photos", true),
        (TradeCerts, "Additional certification (optional)", false),
    ];

    public static readonly IReadOnlyList<(string Type, string Label, bool Required)> KitchenSlots =
    [
        (GovernmentId, "Photo ID", true),
        (BusinessRegistration, "Business registration", true),
        (License, "License / qualification proof", true),
        (LiabilityInsurance, "Certificate of insurance", true),
        (Portfolio, "Portfolio photos", true),
        (References, "Customer references (optional)", false),
    ];

    public static readonly IReadOnlyList<(string Type, string Label, bool Required)> BathroomSlots =
    [
        (GovernmentId, "Photo ID", true),
        (BusinessRegistration, "Business registration", true),
        (License, "License / qualification proof", true),
        (LiabilityInsurance, "Certificate of insurance", true),
        (Portfolio, "Portfolio photos", true),
        (References, "Customer references (optional)", false),
    ];

    public static readonly IReadOnlyList<(string Type, string Label, bool Required)> ConstructionSlots =
    [
        (ContractorLicense, "General contractor license", true),
        (LiabilityInsurance, "Certificate of insurance", true),
        (BusinessRegistration, "Business registration", true),
        (W9, "W-9 form", true),
        (GovernmentId, "Government ID", true),
        (Portfolio, "Project photos / portfolio", false),
    ];

    public static readonly IReadOnlyList<(string Type, string Label, bool Required)> HandymanSlots =
    [
        (GovernmentId, "Government ID", true),
        (LiabilityInsurance, "General liability insurance", true),
        (BusinessRegistration, "Business registration", false),
        (WorkPhotos, "2–3 photos of completed handyman work", true),
        (References, "References", false),
        (TradeCerts, "Certifications", false),
    ];

    public static IReadOnlyList<(string Type, string Label, bool Required)> GetSlotsForTrade(string? tradeCode)
    {
        if (string.Equals(tradeCode, ProviderRegistrationState.PlumbingCategoryId, StringComparison.OrdinalIgnoreCase))
        {
            return PlumbingSlots;
        }

        if (string.Equals(tradeCode, ProviderRegistrationState.HvacCategoryId, StringComparison.OrdinalIgnoreCase))
        {
            return HvacSlots;
        }

        if (string.Equals(tradeCode, ProviderRegistrationState.HandymanCategoryId, StringComparison.OrdinalIgnoreCase))
        {
            return HandymanSlots;
        }

        if (string.Equals(tradeCode, ProviderRegistrationState.ConstructionCategoryId, StringComparison.OrdinalIgnoreCase))
        {
            return ConstructionSlots;
        }

        if (string.Equals(tradeCode, ProviderRegistrationState.BathroomCategoryId, StringComparison.OrdinalIgnoreCase))
        {
            return BathroomSlots;
        }

        if (string.Equals(tradeCode, ProviderRegistrationState.KitchenCategoryId, StringComparison.OrdinalIgnoreCase))
        {
            return KitchenSlots;
        }

        if (string.Equals(tradeCode, ProviderRegistrationState.RoofingCategoryId, StringComparison.OrdinalIgnoreCase))
        {
            return RoofingSlots;
        }

        return DefaultSlots;
    }
}

public record ProviderDocumentSlot(
    string DocumentType,
    string Label,
    bool Required,
    string Status,
    string? FileUrl,
    string? DisplayFileName);
