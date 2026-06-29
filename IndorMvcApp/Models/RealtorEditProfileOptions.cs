namespace IndorMvcApp.Models;

public static class RealtorEditProfileOptions
{
    public static IReadOnlyList<string> Specialties { get; } =
    [
        "Residential",
        "Investment",
        "Luxury",
        "Commercial",
        "Relocation"
    ];

    public static IReadOnlyList<string> ExperienceLevels { get; } =
    [
        "Less than 1 year",
        "1-2 years",
        "3-5 years",
        "5-7 years",
        "7+ years",
        "10+ years",
        "15+ years"
    ];

    public static IReadOnlyList<string> SupportedLanguages { get; } =
    [
        "English",
        "Spanish"
    ];
}

public static class RealtorEditProfileActions
{
    public const string BusinessInformation = "BusinessInformation";
    public const string EditProfileContact = "EditProfileContact";
    public const string EditProfileLicense = "EditProfileLicense";
    public const string EditProfileReview = "EditProfileReview";
}
