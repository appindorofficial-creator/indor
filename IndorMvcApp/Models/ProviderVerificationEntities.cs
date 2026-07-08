using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

/// <summary>
/// Operator verification record for a contractor (IndorProveedor). Powers the
/// "Verify Contractors" console: queue status, per-item review decisions,
/// operator notes, and the final approval. One row per contractor reviewed.
/// </summary>
[Table("IndorProveedorVerificaciones")]
public class IndorProveedorVerificacion
{
    public int Id { get; set; }

    /// <summary>The contractor under review.</summary>
    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = VerificationStatuses.Pending;

    public bool LicenseVerified { get; set; }
    public DateTime? LicenseExpiry { get; set; }

    public bool InsuranceVerified { get; set; }
    public DateTime? InsuranceExpiry { get; set; }

    public bool W9Verified { get; set; }

    [Required, MaxLength(20)]
    public string BackgroundStatus { get; set; } = BackgroundCheckStatuses.Pending;

    [MaxLength(600)]
    public string? OperatorNotes { get; set; }

    [MaxLength(300)]
    public string? FollowUpNote { get; set; }

    [MaxLength(160)]
    public string? ReviewerName { get; set; }

    public DateTime? ApprovedUtc { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

public static class VerificationStatuses
{
    public const string Pending = "Pending";
    public const string InReview = "InReview";
    public const string Approved = "Approved";
    public const string Flagged = "Flagged";
}

public static class BackgroundCheckStatuses
{
    public const string Pending = "Pending";
    public const string Clear = "Clear";
    public const string Flagged = "Flagged";
}
