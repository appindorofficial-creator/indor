using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("IndorNearbyNetworkSettings")]
public class IndorNearbyNetworkSetting
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    [MaxLength(200)]
    public string CenterLabel { get; set; } = "Your service area";

    [MaxLength(250)]
    public string? CenterAddress { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? CenterLatitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? CenterLongitude { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal RadiusMiles { get; set; } = 3m;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

[Table("IndorNearbyNetworkItems")]
public class IndorNearbyNetworkItem
{
    public int Id { get; set; }

    public int? OwnerRealtorId { get; set; }

    [ForeignKey(nameof(OwnerRealtorId))]
    public IndorRealtor? OwnerRealtor { get; set; }

    [Required, MaxLength(30)]
    public string CardType { get; set; } = NearbyNetworkCardTypes.Listing;

    [Required, MaxLength(30)]
    public string FilterCategory { get; set; } = NearbyNetworkFilterCategories.Homes;

    [MaxLength(40)]
    public string BadgeLabel { get; set; } = string.Empty;

    [MaxLength(30)]
    public string BadgeCss { get; set; } = "listing";

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Subtitle { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? Price { get; set; }

    [Column(TypeName = "decimal(3,1)")]
    public decimal? Bedrooms { get; set; }

    [Column(TypeName = "decimal(3,1)")]
    public decimal? Bathrooms { get; set; }

    public int? SquareFeet { get; set; }

    [MaxLength(200)]
    public string? SpecsLabel { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(60)]
    public string? IconClass { get; set; }

    [MaxLength(200)]
    public string? MetaLabel { get; set; }

    [MaxLength(500)]
    public string? TagsJson { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DistanceMiles { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Longitude { get; set; }

    [MaxLength(40)]
    public string? StatusBadge { get; set; }

    [MaxLength(30)]
    public string? StatusCss { get; set; }

    [MaxLength(60)]
    public string PrimaryActionLabel { get; set; } = "View";

    [MaxLength(300)]
    public string PrimaryActionUrl { get; set; } = "#";

    [MaxLength(60)]
    public string? SecondaryActionLabel { get; set; }

    [MaxLength(300)]
    public string? SecondaryActionUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsOwnedListing { get; set; }

    public int SortOrder { get; set; }

    public int? RelatedClientId { get; set; }

    [ForeignKey(nameof(RelatedClientId))]
    public IndorRealtorClient? RelatedClient { get; set; }

    [MaxLength(120)]
    public string? ProviderName { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedUtc { get; set; }

    public DateTime? ExpiresUtc { get; set; }
}

public static class NearbyNetworkCardTypes
{
    public const string Listing = "Listing";
    public const string OpenHouse = "OpenHouse";
    public const string Lead = "Lead";
    public const string Provider = "Provider";
    public const string Promotion = "Promotion";
    public const string Emergency = "Emergency";
    public const string NeighborRequest = "NeighborRequest";
}

public static class NearbyNetworkFilterCategories
{
    public const string Homes = "Homes";
    public const string OpenHouses = "OpenHouses";
    public const string Leads = "Leads";
    public const string Providers = "Providers";
    public const string Promotions = "Promotions";
    public const string Emergency = "Emergency";
    public const string NeighborRequests = "NeighborRequests";
}
