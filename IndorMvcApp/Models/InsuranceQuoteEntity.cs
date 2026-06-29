using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("IndorProviderInsuranceQuotes")]
public class IndorProviderInsuranceQuote
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [Required, MaxLength(40)]
    public string QuoteCode { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Plan { get; set; }

    // Step 1: Coverage
    [MaxLength(300)]
    public string? Coverages { get; set; }

    [MaxLength(120)]
    public string? Trade { get; set; }

    // Step 2: Business & Owner info
    [MaxLength(200)]
    public string? BusinessName { get; set; }

    [MaxLength(300)]
    public string? BusinessAddress { get; set; }

    [MaxLength(120)]
    public string? City { get; set; }

    [MaxLength(40)]
    public string? State { get; set; }

    [MaxLength(160)]
    public string? OwnerName { get; set; }

    [Column(TypeName = "date")]
    public DateTime? OwnerDateOfBirth { get; set; }

    [MaxLength(40)]
    public string? OwnerPhone { get; set; }

    [MaxLength(256)]
    public string? OwnerEmail { get; set; }

    // Step 3: Business details
    public int? NumberOfEmployees { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? EmployeePayroll { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CompanyGrossRevenue { get; set; }

    [MaxLength(40)]
    public string? YearsInBusiness { get; set; }

    [MaxLength(20)]
    public string? ZipCode { get; set; }

    public bool? WorksAtCustomerHomes { get; set; }

    public bool? UsesSubcontractors { get; set; }

    public bool? NeedsCOI { get; set; }

    // Payment (Step 3 setup + Step 4 charge)
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PayTodayAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlyAmount { get; set; }

    [MaxLength(40)]
    public string? PaymentMethod { get; set; }

    [MaxLength(8)]
    public string? CardLast4 { get; set; }

    public bool? AutoPayMonthly { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FirstBillingDate { get; set; }

    [MaxLength(30)]
    public string? PaymentStatus { get; set; }

    public bool PaymentAuthorized { get; set; }

    public DateTime? PaidUtc { get; set; }

    [MaxLength(60)]
    public string? ReceiptNumber { get; set; }

    // Review / submit
    public bool ConfirmedAccurate { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Submitted";

    public DateTime? SubmittedUtc { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }
}
