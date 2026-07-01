using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("IndorNeighborRequestCategories")]
public class IndorNeighborRequestCategory
{
    public int Id { get; set; }

    [Required, MaxLength(40)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LabelEn { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DescriptionEn { get; set; }

    [MaxLength(60)]
    public string IconClass { get; set; } = "fa-circle";

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

[Table("IndorNeighborRequests")]
public class IndorNeighborRequest
{
    public int Id { get; set; }

    public int PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public IndorNeighborRequestCategory? Category { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? DetailsSummary { get; set; }

    [MaxLength(500)]
    public string? LocationAddress { get; set; }

    public DateTime? NeededByDate { get; set; }

    public TimeOnly? TimeWindowStart { get; set; }

    public TimeOnly? TimeWindowEnd { get; set; }

    [Required, MaxLength(30)]
    public string TimelineCode { get; set; } = NeighborRequestTimelineCodes.ThisWeek;

    [Required, MaxLength(30)]
    public string AudienceCode { get; set; } = NeighborRequestAudienceCodes.Neighbors;

    [Column(TypeName = "decimal(12,2)")]
    public decimal? BudgetAmount { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = NeighborRequestStatuses.Active;

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedUtc { get; set; }

    public DateTime? UpdatedUtc { get; set; }

    [MaxLength(40)]
    public string? CancelReasonCode { get; set; }

    [MaxLength(500)]
    public string? CancelNote { get; set; }

    public DateTime? CancelledUtc { get; set; }

    public ICollection<IndorNeighborRequestPhoto> Photos { get; set; } = [];

    public ICollection<IndorNeighborRequestOffer> Offers { get; set; } = [];
}

[Table("IndorNeighborRequestPhotos")]
public class IndorNeighborRequestPhoto
{
    public int Id { get; set; }

    public int RequestId { get; set; }

    [ForeignKey(nameof(RequestId))]
    public IndorNeighborRequest? Request { get; set; }

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

[Table("IndorNeighborRequestOffers")]
public class IndorNeighborRequestOffer
{
    public int Id { get; set; }

    public int RequestId { get; set; }

    [ForeignKey(nameof(RequestId))]
    public IndorNeighborRequest? Request { get; set; }

    [Required, MaxLength(30)]
    public string OfferType { get; set; } = NeighborRequestOfferTypes.Neighbor;

    public int? ProviderId { get; set; }

    [MaxLength(450)]
    public string? ResponderUserId { get; set; }

    [Required, MaxLength(120)]
    public string OffererName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? OffererPhotoUrl { get; set; }

    [MaxLength(500)]
    public string? Message { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? PriceAmount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DistanceMiles { get; set; }

    [Column(TypeName = "decimal(3,1)")]
    public decimal? Rating { get; set; }

    [MaxLength(80)]
    public string? ScheduleLabel { get; set; }

    public bool IsVerified { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = NeighborRequestOfferStatuses.Pending;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

public static class NeighborRequestStatuses
{
    public const string Active = "Active";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public static class NeighborRequestCancelReasons
{
    public const string FoundHelp = "FoundHelp";
    public const string NoLongerNeeded = "NoLongerNeeded";
    public const string PostedByMistake = "PostedByMistake";
    public const string TakingTooLong = "TakingTooLong";
    public const string Other = "Other";

    public static IReadOnlyList<(string Value, string Label)> Options { get; } =
    [
        (FoundHelp, "I found help elsewhere"),
        (NoLongerNeeded, "I no longer need this"),
        (PostedByMistake, "I posted this by mistake"),
        (TakingTooLong, "It is taking too long"),
        (Other, "Other")
    ];
}

public static class NeighborRequestTimelineCodes
{
    public const string Asap = "Asap";
    public const string ThisWeek = "ThisWeek";
    public const string ThisMonth = "ThisMonth";
    public const string Flexible = "Flexible";
    public const string Today = "Today";
    public const string Tomorrow = "Tomorrow";
    public const string PickDate = "PickDate";
}

public static class NeighborRequestDurationCodes
{
    public const string OneHour = "1hour";
    public const string TwoHours = "2hours";
    public const string HalfDay = "halfday";
    public const string FullDay = "fullday";
}

public static class NeighborRequestPayTypeCodes
{
    public const string Hourly = "Hourly";
    public const string Fixed = "Fixed";
}

public static class NeighborRequestPreferredTimeCodes
{
    public const string Morning = "morning";
    public const string Afternoon = "afternoon";
    public const string Evening = "evening";
    public const string Flexible = "flexible";
}

public static class NeighborRequestAudienceCodes
{
    public const string Neighbors = "Neighbors";
    public const string CertifiedProviders = "CertifiedProviders";
    public const string Both = "Both";
}

public static class NeighborRequestOfferTypes
{
    public const string Neighbor = "Neighbor";
    public const string Provider = "Provider";
}

public static class NeighborRequestOfferStatuses
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Declined = "Declined";
}

public static class NearbyNetworkHomeownerFilters
{
    public const string All = "All";
    public const string Homes = "Homes";
    public const string Providers = "Providers";
    public const string Promotions = "Promotions";
    public const string Emergency = "Emergency";
    public const string NeighborRequests = "NeighborRequests";
}

public class NeighborRequestDraftState
{
    public int PropiedadId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DetailsSummary { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public bool UseHomeAddress { get; set; } = true;
    public DateTime? NeededByDate { get; set; }
    public TimeOnly? TimeWindowStart { get; set; }
    public TimeOnly? TimeWindowEnd { get; set; }
    public string TimelineCode { get; set; } = string.Empty;
    public string PreferredTimeCode { get; set; } = string.Empty;
    public int HelperCount { get; set; }
    public string DurationCode { get; set; } = string.Empty;
    public string PayTypeCode { get; set; } = string.Empty;
    public bool ScheduleConfigured { get; set; }
    public string AudienceCode { get; set; } = NeighborRequestAudienceCodes.Neighbors;
    public decimal? BudgetAmount { get; set; }
    public List<string> PhotoPaths { get; set; } = [];
    public List<string> ToolsNeeded { get; set; } = [];
    public string? SpecialNotes { get; set; }
    public string? PetsOnProperty { get; set; }
    public string? HasStairs { get; set; }
    public string? GateCode { get; set; }
    public string? ParkingAvailable { get; set; }
    public int? EditingRequestId { get; set; }
}
