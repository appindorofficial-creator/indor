namespace IndorMvcApp.ViewModels;

/// <summary>Session-held state for the 5-step insurance quote + payment wizard.</summary>
public class ProviderProInsuranceQuoteDraft
{
    public string Plan { get; set; } = "Basic";

    // Step 1 — Coverage (fixed by plan)
    public List<string> Coverages { get; set; } = ["General Liability"];

    // Step 2 — Business & Owner info
    public string Trade { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = "NC";
    public string ZipCode { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerDateOfBirth { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;

    // Step 3 — Business details
    public string NumberOfEmployees { get; set; } = string.Empty;
    public decimal? EmployeePayroll { get; set; }
    public decimal? CompanyGrossRevenue { get; set; }
    public string YearsInBusiness { get; set; } = string.Empty;
    public bool? WorksAtCustomerHomes { get; set; }
    public bool? UsesSubcontractors { get; set; }
    public bool? NeedsCOI { get; set; }

    // Step 3 — Payment setup
    public bool AutoPayMonthly { get; set; } = true;
    public string CardLast4 { get; set; } = "4242";

    // Step 4 — Payment
    public string PaymentMethod { get; set; } = "Card";

    /// <summary>Composed single-line address used in review/persistence.</summary>
    public string FullAddress
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(StreetAddress)) parts.Add(StreetAddress.Trim());
            var cityState = string.Join(" ", new[] { City, State }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
            var line2 = string.Join(" ", new[] { cityState, ZipCode }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
            if (!string.IsNullOrWhiteSpace(line2)) parts.Add(line2);
            return string.Join(", ", parts);
        }
    }
}

public abstract class InsuranceWizardStepViewModel
{
    public string CompanyInitial { get; set; } = "P";
    public string Plan { get; set; } = "Basic";

    public string PlanDisplayName => $"{Plan} Plan";
    public string Coverage => "General Liability";
    public bool IsPopular => string.Equals(Plan, "Standard", StringComparison.OrdinalIgnoreCase);
    public bool IsPremium => string.Equals(Plan, "Premium", StringComparison.OrdinalIgnoreCase);
    public string PlanSubtitle => IsPremium
        ? "Premium protection + INDOR business benefits"
        : "General Liability + INDOR business benefits";
    public string CoverageTagline => IsPremium ? "Premium protection" : "General Liability";
    public decimal PayToday => InsuranceCatalog.Pricing(Plan).PayToday;
    public decimal Monthly => InsuranceCatalog.Pricing(Plan).Monthly;
}

public class InsuranceCoverageStepViewModel : InsuranceWizardStepViewModel
{
}

public class InsuranceBusinessInfoStepViewModel : InsuranceWizardStepViewModel
{
    public string Trade { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = "NC";
    public string ZipCode { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerDateOfBirth { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
}

public class InsuranceBusinessDetailsStepViewModel : InsuranceWizardStepViewModel
{
    public string NumberOfEmployees { get; set; } = string.Empty;
    public decimal? EmployeePayroll { get; set; }
    public decimal? CompanyGrossRevenue { get; set; }
    public string YearsInBusiness { get; set; } = string.Empty;
    public bool? WorksAtCustomerHomes { get; set; }
    public bool? UsesSubcontractors { get; set; }
    public bool? NeedsCOI { get; set; }

    public bool AutoPayMonthly { get; set; } = true;
    public string CardLast4 { get; set; } = "4242";
    public DateTime FirstBillingDate { get; set; } = DateTime.Today.AddDays(30);
}

public class InsuranceReviewViewModel : InsuranceWizardStepViewModel
{
    public ProviderProInsuranceQuoteDraft Draft { get; set; } = new();
    public string PaymentMethod { get; set; } = "Card";
    public bool Authorize { get; set; }
}

public class InsuranceSubmittedViewModel : InsuranceWizardStepViewModel
{
    public decimal PaidToday { get; set; }
    public decimal MonthlyPlan { get; set; }
    public string Status { get; set; } = "Pending Carrier Approval";
    public string ReceiptNumber { get; set; } = string.Empty;
}

public record InsuranceFeature(string Icon, string Title, string Desc);

public static class InsuranceCatalog
{
    public static (decimal PayToday, decimal Monthly) Pricing(string? plan) =>
        (plan?.Trim().ToLowerInvariant()) switch
        {
            "standard" => (325m, 183m),
            "premium" => (325m, 203m),
            _ => (325m, 163m)
        };

    public static readonly (string Value, string Icon)[] Trades =
    {
        ("HVAC installation & repair", "fa-snowflake"),
        ("Handyman", "fa-wrench"),
        ("Painting", "fa-paint-roller"),
        ("Electrical", "fa-bolt"),
        ("Plumbing", "fa-faucet-drip"),
        ("Roofing", "fa-house-chimney"),
        ("Landscaping", "fa-leaf"),
        ("General Contractor", "fa-helmet-safety"),
        ("Cleaning", "fa-broom"),
        ("Flooring", "fa-rug"),
        ("Drywall Installation", "fa-trowel"),
        ("Other", "fa-ellipsis")
    };

    public static readonly string[] YearsOptions =
    {
        "Less than 1 year", "1–2 years", "3–5 years", "6–10 years", "More than 10 years"
    };

    public static readonly string[] EmployeeOptions =
    {
        "Just me", "2", "3–5", "6–10", "11–25", "More than 25"
    };

    public static readonly string[] States =
    {
        "AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA","HI","ID","IL","IN","IA","KS","KY","LA","ME","MD",
        "MA","MI","MN","MS","MO","MT","NE","NV","NH","NJ","NM","NY","NC","ND","OH","OK","OR","PA","RI","SC",
        "SD","TN","TX","UT","VT","VA","WA","WV","WI","WY"
    };

    /// <summary>"What's included in {plan}" feature grid (Basic shows descriptions; tiers are title-only).</summary>
    public static IReadOnlyList<InsuranceFeature> IncludedFor(string? plan)
    {
        switch ((plan ?? "").Trim().ToLowerInvariant())
        {
            case "standard":
                // Ordered so a 2-col row-major grid matches the design columns.
                return new List<InsuranceFeature>
                {
                    new("fa-shield-halved", "General Liability coverage", ""),
                    new("fa-id-badge", "INDOR verified business badge", ""),
                    new("fa-house", "Property damage claim support", ""),
                    new("fa-eye", "Better visibility in INDOR", ""),
                    new("fa-user", "Third-party injury claim support", ""),
                    new("fa-clock", "Faster quote review", ""),
                    new("fa-gavel", "Legal defense support", ""),
                    new("fa-people-group", "More trust with customers", ""),
                    new("fa-file-lines", "COI / proof of insurance support", ""),
                    new("fa-briefcase", "More job opportunities through INDOR", ""),
                    new("fa-bolt", "Priority COI support", ""),
                    new("fa-building", "Stronger business presence", "")
                };
            case "premium":
                // Ordered so a 2-col row-major grid matches the design columns.
                return new List<InsuranceFeature>
                {
                    new("fa-house", "Property damage claim support", "Help with claims for damage you may cause."),
                    new("fa-id-badge", "Premium INDOR business profile", "Stand out with advanced business tools."),
                    new("fa-user", "Third-party injury claim support", "Support for injuries to others on the job."),
                    new("fa-award", "Featured placement in INDOR", "Get noticed by more customers."),
                    new("fa-gavel", "Legal defense support", "Access to legal support when you need it."),
                    new("fa-arrow-trend-up", "Lead boost inside INDOR", "Increase your visibility and lead potential."),
                    new("fa-file-lines", "COI / proof of insurance support", "Get Certificates of Insurance quickly."),
                    new("fa-bolt", "Faster COI requests", "Get COIs when you need them, faster."),
                    new("fa-headset", "Priority support", "Faster response from our expert support team."),
                    new("fa-briefcase", "More job opportunities through INDOR", "Win more jobs and grow your business.")
                };
            default:
                return new List<InsuranceFeature>
                {
                    new("fa-shield-halved", "General Liability coverage", "Protection for common third-party claims."),
                    new("fa-house", "Property damage claim support", "Help with claims for damage you may cause."),
                    new("fa-user", "Third-party injury claim support", "Support for injuries to others on the job."),
                    new("fa-gavel", "Legal defense support", "Access to legal support when you need it."),
                    new("fa-file-lines", "COI / proof of insurance support", "Get Certificates of Insurance quickly."),
                    new("fa-id-card", "Basic INDOR profile", "Get listed in the INDOR network with your business profile."),
                    new("fa-headset", "Standard support", "Reach our support team for your questions."),
                    new("fa-people-group", "More trust with customers", "Show customers you're protected and professional."),
                    new("fa-briefcase", "More job opportunities through INDOR", "Get discovered by more customers and win more jobs.")
                };
        }
    }

    /// <summary>"What this plan helps protect" cards (same across plans).</summary>
    public static readonly InsuranceFeature[] Protects =
    {
        new("fa-house", "Customer property damage", "Coverage for damage you may cause to customer property."),
        new("fa-user-injured", "Third-party injury claims", "Protection if others are injured on or near your jobsite."),
        new("fa-shield-halved", "Jobsite credibility", "Show you're insured and take your business seriously."),
        new("fa-file-lines", "Proof of insurance requests", "Provide COIs to clients and win more work.")
    };

    /// <summary>"Included With Your {plan}" check grid (review screen) — icon + label.</summary>
    public static IReadOnlyList<InsuranceFeature> ReviewIncludesFor(string? plan)
    {
        switch ((plan ?? "").Trim().ToLowerInvariant())
        {
            case "standard":
                return new[]
                {
                    new InsuranceFeature("fa-shield-halved", "General Liability", ""),
                    new InsuranceFeature("fa-house", "Property damage claim support", ""),
                    new InsuranceFeature("fa-user", "Third-party injury claim support", ""),
                    new InsuranceFeature("fa-gavel", "Legal defense support", ""),
                    new InsuranceFeature("fa-file-lines", "COI / proof of insurance support", ""),
                    new InsuranceFeature("fa-bolt", "Priority COI support", ""),
                    new InsuranceFeature("fa-id-badge", "INDOR verified badge", ""),
                    new InsuranceFeature("fa-eye", "Better visibility in INDOR", ""),
                    new InsuranceFeature("fa-clock", "Faster quote review", "")
                };
            case "premium":
                return new[]
                {
                    new InsuranceFeature("fa-house", "Property damage claim support", ""),
                    new InsuranceFeature("fa-user", "Third-party injury claim support", ""),
                    new InsuranceFeature("fa-gavel", "Legal defense support", ""),
                    new InsuranceFeature("fa-file-lines", "COI / proof of insurance support", ""),
                    new InsuranceFeature("fa-headset", "Priority support", ""),
                    new InsuranceFeature("fa-id-badge", "Premium INDOR profile", ""),
                    new InsuranceFeature("fa-eye", "Featured visibility in INDOR", ""),
                    new InsuranceFeature("fa-arrow-trend-up", "Lead boost inside INDOR", ""),
                    new InsuranceFeature("fa-bolt", "Faster COI support", "")
                };
            default:
                return new[]
                {
                    new InsuranceFeature("fa-shield-halved", "General Liability", ""),
                    new InsuranceFeature("fa-house", "Property damage claim support", ""),
                    new InsuranceFeature("fa-user", "Third-party injury claim support", ""),
                    new InsuranceFeature("fa-gavel", "Legal defense support", ""),
                    new InsuranceFeature("fa-file-lines", "COI / proof of insurance support", ""),
                    new InsuranceFeature("fa-id-card", "Basic INDOR profile", ""),
                    new InsuranceFeature("fa-headset", "Standard support", "")
                };
        }
    }

    /// <summary>"With INDOR {plan}, you get" success benefits.</summary>
    public static IReadOnlyList<InsuranceFeature> BenefitsFor(string? plan)
    {
        switch ((plan ?? "").Trim().ToLowerInvariant())
        {
            case "standard":
                return new[]
                {
                    new InsuranceFeature("fa-people-group", "More trust with customers", ""),
                    new InsuranceFeature("fa-bolt", "Priority COI support", ""),
                    new InsuranceFeature("fa-id-badge", "INDOR verified business badge", ""),
                    new InsuranceFeature("fa-eye", "Better visibility in INDOR", ""),
                    new InsuranceFeature("fa-clock", "Faster quote review", "")
                };
            case "premium":
                return new[]
                {
                    new InsuranceFeature("fa-headset", "Priority support", ""),
                    new InsuranceFeature("fa-star", "Featured provider visibility", ""),
                    new InsuranceFeature("fa-arrow-trend-up", "Lead boost inside INDOR", ""),
                    new InsuranceFeature("fa-stopwatch", "Faster proof of insurance support", "")
                };
            default:
                return new[]
                {
                    new InsuranceFeature("fa-people-group", "More trust with customers", ""),
                    new InsuranceFeature("fa-shield-halved", "Proof of insurance support", ""),
                    new InsuranceFeature("fa-briefcase", "Better job readiness through INDOR", "")
                };
        }
    }
}
