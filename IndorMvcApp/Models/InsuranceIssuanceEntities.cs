using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

/// <summary>
/// A manual insurance issuance request. The provider fills the carrier's
/// "Business Quote Sheet" fields and INDOR emails them to the partner carrier
/// so a policy can be issued manually (before the API integration exists).
/// </summary>
[Table("IndorInsuranceIssuanceRequests")]
public class IndorInsuranceIssuanceRequest
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [Required, MaxLength(40)]
    public string RequestCode { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Plan { get; set; }

    // --- Business Quote Sheet fields ---
    [Required, MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string BusinessAddress { get; set; } = string.Empty;

    public bool WorkersComp { get; set; }

    public bool GeneralLiability { get; set; }

    [Required, MaxLength(160)]
    public string OwnerName { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime? OwnerDateOfBirth { get; set; }

    [MaxLength(40)]
    public string? OwnerPhone { get; set; }

    [MaxLength(256)]
    public string? OwnerEmail { get; set; }

    [MaxLength(160)]
    public string? TypeOfBusiness { get; set; }

    [MaxLength(60)]
    public string? NumberOfEmployees { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? EmployeePayroll { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CompanyGross { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // --- Workflow / notification tracking ---
    [Required, MaxLength(30)]
    public string Status { get; set; } = "Submitted";

    [MaxLength(256)]
    public string? CarrierEmail { get; set; }

    [MaxLength(30)]
    public string? CarrierEmailStatus { get; set; }

    public DateTime? CarrierEmailSentUtc { get; set; }

    public DateTime? SubmittedUtc { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }
}
