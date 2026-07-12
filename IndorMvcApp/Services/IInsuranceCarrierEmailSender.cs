namespace IndorMvcApp.Services;

/// <summary>Data sent to the partner carrier so they can issue a policy manually.</summary>
public sealed record InsuranceIssuanceEmailModel(
    string RequestCode,
    string? Plan,
    string BusinessName,
    string BusinessAddress,
    bool WorkersComp,
    bool GeneralLiability,
    string OwnerName,
    string? OwnerDateOfBirth,
    string? OwnerPhone,
    string? OwnerEmail,
    string? TypeOfBusiness,
    string? NumberOfEmployees,
    string? EmployeePayroll,
    string? CompanyGross,
    string? Notes,
    string? ProviderContactEmail);

public enum InsuranceEmailResult
{
    Sent,
    NotConfigured,
    Failed
}

public interface IInsuranceCarrierEmailSender
{
    /// <summary>Sends the issuance request to the carrier. Never throws.</summary>
    Task<InsuranceEmailResult> SendIssuanceRequestAsync(
        InsuranceIssuanceEmailModel model, CancellationToken cancellationToken = default);
}
