namespace IndorMvcApp.Models;

/// <summary>
/// Configuration for the manual insurance issuance flow.
/// The carrier email receives the "Business Quote Sheet" so it can issue a
/// policy manually until the API integration is available.
/// </summary>
public class InsuranceSettings
{
    /// <summary>Partner carrier inbox that receives issuance requests.</summary>
    public string CarrierEmail { get; set; } = string.Empty;

    /// <summary>Display name for the carrier (used in the email greeting).</summary>
    public string CarrierName { get; set; } = "Insurance Carrier";

    /// <summary>
    /// Optional address(es) that also receive a copy (CC).
    /// Supports multiple emails separated by comma or semicolon.
    /// Default: Carolina Coverage Insurance issuance copy.
    /// </summary>
    public string? CopyToEmail { get; set; } = "jeanna@carolinacoverageinsurance.com";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(CarrierEmail);
}
