using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

/// <summary>
/// A homeowner-initiated service request that matching providers can browse and claim
/// (first-come). Once claimed it disappears from every other provider's available list.
/// </summary>
[Table("IndorServiceRequests")]
public class IndorServiceRequest
{
    public int Id { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(40)]
    public string CategoryId { get; set; } = string.Empty;

    [ForeignKey(nameof(CategoryId))]
    public IndorProveedorCategoriaCatalogo? Category { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(120)]
    public string? City { get; set; }

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    [Column(TypeName = "date")]
    public DateTime? PreferredDate { get; set; }

    [MaxLength(40)]
    public string? PreferredTime { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? BudgetAmount { get; set; }

    [Required, MaxLength(20)]
    public string Urgency { get; set; } = ServiceRequestUrgencies.Standard;

    [Required, MaxLength(20)]
    public string Status { get; set; } = ServiceRequestStatuses.Open;

    public int? ClaimedByProveedorId { get; set; }

    [ForeignKey(nameof(ClaimedByProveedorId))]
    public IndorProveedor? ClaimedByProveedor { get; set; }

    public DateTime? ClaimedUtc { get; set; }

    public DateTime? CancelledUtc { get; set; }

    public int NotifiedProviderCount { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

public static class ServiceRequestStatuses
{
    public const string Open = "Open";
    public const string Claimed = "Claimed";
    public const string Cancelled = "Cancelled";
    public const string Completed = "Completed";
}

public static class ServiceRequestUrgencies
{
    public const string Standard = "Standard";
    public const string Urgent = "Urgent";
    public const string Emergency = "Emergency";
}

/// <summary>
/// Unified in-app notification for homeowners and providers. Both English and Spanish
/// copies are stored at creation time so display simply picks the current culture.
/// </summary>
[Table("IndorAppNotifications")]
public class IndorAppNotification
{
    public int Id { get; set; }

    [Required, MaxLength(450)]
    public string RecipientUserId { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Audience { get; set; } = AppNotificationAudiences.Homeowner;

    [Required, MaxLength(200)]
    public string TitleEn { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string TitleEs { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? BodyEn { get; set; }

    [MaxLength(500)]
    public string? BodyEs { get; set; }

    [MaxLength(40)]
    public string? CategoryTag { get; set; }

    [MaxLength(60)]
    public string? IconClass { get; set; }

    [MaxLength(300)]
    public string? TargetUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadUtc { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public static class AppNotificationAudiences
{
    public const string Homeowner = "Homeowner";
    public const string Provider = "Provider";
}
