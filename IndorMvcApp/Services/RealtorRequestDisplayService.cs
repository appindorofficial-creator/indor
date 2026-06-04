using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class RealtorRequestDisplayService
{
    public static string NeedLabel(string? value) => value switch
    {
        "Buy" => "Buy a home",
        "Sell" => "Sell a home",
        "Rent" => "Rent a home",
        "GeneralGuidance" => "General guidance",
        _ => "Buy a home"
    };

    public static string TimeframeLabel(string? value) => value switch
    {
        "ASAP" => "ASAP",
        "Days30" => "30 days",
        "Days60Plus" => "60+ days",
        _ => value ?? "ASAP"
    };

    public static string StatusLabel(string? value) => value switch
    {
        "Draft" => "In progress",
        "MatchingInProgress" => "Matching in progress",
        "Matched" => "Matched",
        "Completed" => "Completed",
        "Cancelled" => "Cancelled",
        _ => "Matching in progress"
    };

    public static string? PriceRangeLabel(string? value) => string.IsNullOrWhiteSpace(value) ? null : value switch
    {
        "Under300k" => "Under $300k",
        "300k-500k" => "$300k – $500k",
        "500k-750k" => "$500k – $750k",
        "750kPlus" => "$750k+",
        _ => value
    };

    public static string RentComfortLabel(string? value) => value switch
    {
        "Under1200" => "Under $1,200",
        "1200-1500" => "$1,200–$1,500",
        "1500-2000" => "$1,500–$2,000",
        "2000-3000" => "$2,000–$3,000",
        "3000-4000" => "$3,000–$4,000",
        "4000Plus" => "$4,000+",
        _ => value ?? "—"
    };

    public static string HomeTypeLabel(string? value) => value switch
    {
        "Apartment" => "Apartment",
        "Townhouse" => "Townhouse",
        "House" => "House",
        "Condo" => "Condo",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string CountLabel(string? value) => value ?? "—";

    public static string PetsLabel(string? value) => value switch
    {
        "NoPets" => "No pets",
        "Dog" => "Dog",
        "Cat" => "Cat",
        "MultiPets" => "Multi-pets",
        _ => value ?? "—"
    };

    public static string OutdoorSpaceLabel(string? value) => value switch
    {
        "VeryImportant" => "Very important",
        "NiceToHave" => "Nice to have",
        "NotNeeded" => "Not needed",
        _ => value ?? "—"
    };

    public static string ParkingLabel(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "Flexible" => "Flexible",
        _ => value ?? "—"
    };

    public static string ContactMethodLabel(string? value) => value switch
    {
        "Call" => "Call",
        "Text" => "Text",
        "Email" => "Email",
        "WhatsApp" => "WhatsApp",
        "ZoomMeet" => "Zoom Meet",
        _ => value ?? "—"
    };

    public static string PriorityLabel(string value) => value switch
    {
        "LowerPayment" => "Lower payment",
        "MoreSpace" => "More space",
        "OutdoorSpace" => "Outdoor space",
        "GoodLocation" => "Good location",
        "QuietArea" => "Quiet area",
        "PetFriendly" => "Pet friendly",
        _ => value
    };

    public static RealtorGuidanceReviewSummaryViewModel BuildGuidanceSummary(SolicitudRealtor record) =>
        new()
        {
            RentComfortLabel = RentComfortLabel(record.RentComfortRange),
            MoveTimelineLabel = TimeframeLabel(record.Timeframe),
            HomeTypeLabel = HomeTypeLabel(record.HomeType),
            BedroomsLabel = CountLabel(record.Bedrooms),
            BathroomsLabel = CountLabel(record.Bathrooms),
            PetsLabel = PetsLabel(record.Pets),
            AreaLabel = string.IsNullOrWhiteSpace(record.PreferredArea) ? "—" : record.PreferredArea,
        };

    public static readonly (string Value, string Label)[] PriceRangeOptions =
    [
        ("Under300k", "Under $300k"),
        ("300k-500k", "$300k – $500k"),
        ("500k-750k", "$500k – $750k"),
        ("750kPlus", "$750k+")
    ];

    public static readonly (string Value, string Label)[] RentComfortOptions =
    [
        ("Under1200", "Under $1,200"),
        ("1200-1500", "$1,200–$1,500"),
        ("1500-2000", "$1,500–$2,000"),
        ("2000-3000", "$2,000–$3,000"),
        ("3000-4000", "$3,000–$4,000"),
        ("4000Plus", "$4,000+")
    ];

    public static readonly (string Value, string Label, string Icon)[] HomeTypeOptions =
    [
        ("Apartment", "Apartment", "fa-building"),
        ("Townhouse", "Townhouse", "fa-house"),
        ("House", "House", "fa-house-chimney"),
        ("Condo", "Condo", "fa-city"),
        ("NotSure", "Not sure", "fa-circle-question")
    ];

    public static readonly string[] CountOptions = ["1", "2", "3", "4+"];

    public static readonly string[] BathroomOptions = ["1", "2", "3+", "Flexible"];

    public static readonly (string Value, string Label, string Icon)[] PetOptions =
    [
        ("NoPets", "No pets", "fa-ban"),
        ("Dog", "Dog", "fa-dog"),
        ("Cat", "Cat", "fa-cat"),
        ("MultiPets", "Multi-pets", "fa-paw")
    ];

    public static readonly (string Value, string Label, string Icon)[] OutdoorOptions =
    [
        ("VeryImportant", "Very important", "fa-tree"),
        ("NiceToHave", "Nice to have", "fa-leaf"),
        ("NotNeeded", "Not needed", "fa-circle-xmark")
    ];

    public static readonly (string Value, string Label, string Icon)[] ParkingOptions =
    [
        ("Yes", "Yes", "fa-square-parking"),
        ("No", "No", "fa-ban"),
        ("Flexible", "Flexible", "fa-arrows-left-right")
    ];

    public static readonly (string Value, string Label)[] PriorityOptions =
    [
        ("LowerPayment", "Lower payment"),
        ("MoreSpace", "More space"),
        ("OutdoorSpace", "Outdoor space"),
        ("GoodLocation", "Good location"),
        ("QuietArea", "Quiet area"),
        ("PetFriendly", "Pet friendly")
    ];

    public static readonly (string Value, string Label, string Icon)[] ContactMethodOptions =
    [
        ("Call", "Call", "fa-phone"),
        ("Text", "Text", "fa-comment-dots"),
        ("Email", "Email", "fa-envelope"),
        ("WhatsApp", "WhatsApp", "fa-brands fa-whatsapp"),
        ("ZoomMeet", "Zoom Meet", "fa-video")
    ];

    public static string? ExtractCityFromAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        var parts = address.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return parts[^2].Trim();
        }

        return null;
    }
}
