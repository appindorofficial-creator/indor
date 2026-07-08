using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

/// <summary>
/// Contractor Network — a job a provider posts to get matched with verified
/// subcontractors from the INDOR network. Written by the "Post a Job" flow.
/// </summary>
[Table("IndorProveedorNetworkJobs")]
public class IndorProveedorNetworkJob
{
    public int Id { get; set; }

    /// <summary>The provider (contractor) who posted the job.</summary>
    public int PosterProveedorId { get; set; }

    [MaxLength(40)]
    public string? TradeId { get; set; }

    [MaxLength(120)]
    public string? TradeLabel { get; set; }

    [MaxLength(600)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public DateTime? DateNeeded { get; set; }

    [MaxLength(40)]
    public string? BudgetRange { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = NetworkJobStatuses.Open;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    // ---- Post-a-Job wizard fields (Details / Location & Budget / Review) ----

    /// <summary>Short job title (e.g. "Fix leak under kitchen sink").</summary>
    [MaxLength(160)]
    public string? JobTitle { get; set; }

    /// <summary>When the job is needed: ASAP, ThisWeek, Flexible.</summary>
    [MaxLength(20)]
    public string? Urgency { get; set; }

    /// <summary>House, Townhome, Condo, Commercial.</summary>
    [MaxLength(30)]
    public string? PropertyType { get; set; }

    /// <summary>Who meets the pro: Homeowner, Tenant, PropertyManager.</summary>
    [MaxLength(30)]
    public string? WhoMeets { get; set; }

    /// <summary>Fixed or Hourly.</summary>
    [MaxLength(20)]
    public string? QuoteType { get; set; }

    [MaxLength(300)]
    public string? AccessNotes { get; set; }

    /// <summary>JSON array of uploaded photo URLs.</summary>
    public string? PhotoUrlsJson { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}

public static class NetworkJobStatuses
{
    public const string Draft = "Draft";
    public const string Open = "Open";
    public const string Matched = "Matched";
    public const string Hired = "Hired";
    public const string Closed = "Closed";
}

public static class NetworkJobUrgencies
{
    public const string Asap = "ASAP";
    public const string ThisWeek = "ThisWeek";
    public const string Flexible = "Flexible";
}

public static class NetworkJobQuoteTypes
{
    public const string Fixed = "Fixed";
    public const string Hourly = "Hourly";
}

/// <summary>
/// A hire record created from the "Hire With Confidence" flow, linking the
/// hiring provider with the subcontractor they chose (and optionally the job).
/// </summary>
[Table("IndorProveedorNetworkHires")]
public class IndorProveedorNetworkHire
{
    public int Id { get; set; }

    /// <summary>The provider who is hiring.</summary>
    public int HirerProveedorId { get; set; }

    /// <summary>The subcontractor (also an IndorProveedor) being hired.</summary>
    public int SubcontractorProveedorId { get; set; }

    public int? NetworkJobId { get; set; }

    [MaxLength(160)]
    public string? ProjectTitle { get; set; }

    [MaxLength(120)]
    public string? TradeLabel { get; set; }

    [MaxLength(40)]
    public string? BudgetRange { get; set; }

    public DateTime? StartDate { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = NetworkHireStatuses.Hired;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public static class NetworkHireStatuses
{
    public const string Hired = "Hired";
    public const string Started = "Started";
    public const string AgreementSent = "AgreementSent";
}

/// <summary>
/// A rating/review left for a subcontractor. Powers the star rating and review
/// counts shown on subcontractor cards and profiles.
/// </summary>
[Table("IndorProveedorNetworkResenas")]
public class IndorProveedorNetworkResena
{
    public int Id { get; set; }

    public int SubcontractorProveedorId { get; set; }

    public int? AuthorProveedorId { get; set; }

    [MaxLength(120)]
    public string? AuthorName { get; set; }

    /// <summary>1–5 stars.</summary>
    public int Rating { get; set; }

    [MaxLength(600)]
    public string? Comment { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A subcontractor a provider saved for later ("Save" action on the profile).
/// </summary>
[Table("IndorProveedorNetworkGuardados")]
public class IndorProveedorNetworkGuardado
{
    public int Id { get; set; }

    public int OwnerProveedorId { get; set; }

    public int SubcontractorProveedorId { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
