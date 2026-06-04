using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class RealtorRequestFormViewModel
{
    public int? PropiedadId { get; set; }

    [Required(ErrorMessage = "Select what you need.")]
    public string NeedType { get; set; } = "Buy";

    [MaxLength(120)]
    public string? PreferredArea { get; set; }

    [Required]
    public string Timeframe { get; set; } = "ASAP";

    [MaxLength(80)]
    public string? PriceRange { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class RealtorGuidanceStep1ViewModel
{
    public int SolicitudId { get; set; }
    public int? PropiedadId { get; set; }

    [Required(ErrorMessage = "Select a rent range.")]
    public string RentComfortRange { get; set; } = "2000-3000";

    [Required]
    public string Timeframe { get; set; } = "ASAP";
}

public class RealtorGuidanceStep2ViewModel
{
    public int SolicitudId { get; set; }

    [Required]
    public string HomeType { get; set; } = "Apartment";

    [Required]
    public string Bedrooms { get; set; } = "2";

    [Required]
    public string Bathrooms { get; set; } = "2";

    [Required]
    public string Occupants { get; set; } = "2";
}

public class RealtorGuidanceStep3ViewModel
{
    public int SolicitudId { get; set; }

    [Required]
    public string Pets { get; set; } = "Dog";

    [Required]
    public string OutdoorSpaceImportance { get; set; } = "VeryImportant";

    [Required]
    public string ParkingNeed { get; set; } = "Yes";

    [MaxLength(120)]
    public string? PreferredArea { get; set; }

    public bool OpenToNearbyAreas { get; set; } = true;

    public List<string> Priorities { get; set; } = [];
}

public class RealtorGuidanceStep4ViewModel
{
    public int SolicitudId { get; set; }

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    [MaxLength(256), EmailAddress]
    public string? ContactEmail { get; set; }

    [Required]
    public string PreferredContactMethod { get; set; } = "Text";

    [MaxLength(500)]
    public string? GuidanceNotes { get; set; }

    public RealtorGuidanceReviewSummaryViewModel Summary { get; set; } = new();
}

public class RealtorGuidanceReviewSummaryViewModel
{
    public string RentComfortLabel { get; set; } = "";
    public string MoveTimelineLabel { get; set; } = "";
    public string HomeTypeLabel { get; set; } = "";
    public string BedroomsLabel { get; set; } = "";
    public string BathroomsLabel { get; set; } = "";
    public string PetsLabel { get; set; } = "";
    public string AreaLabel { get; set; } = "";
}

public class RealtorRequestSentViewModel
{
    public int SolicitudId { get; set; }
    public bool IsGeneralGuidance { get; set; }

    public string NeedLabel { get; set; } = string.Empty;
    public string AreaLabel { get; set; } = "—";
    public string TimeframeLabel { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = "Matching in progress";

    public string? RentComfortLabel { get; set; }
    public string? HomeTypeLabel { get; set; }
    public string? BedroomsLabel { get; set; }
    public string? BathroomsLabel { get; set; }
    public string? PetsLabel { get; set; }
}
