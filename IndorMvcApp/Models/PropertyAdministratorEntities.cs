using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("IndorPropertyAdministrators")]
public class IndorPropertyAdministrator
{
    public int Id { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    public Guid RegistrationToken { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)]
    public string RegistrationStatus { get; set; } = PropertyAdministratorRegistrationStatuses.Draft;

    public int CurrentStep { get; set; } = 1;

    [MaxLength(120)]
    public string? DisplayName { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? PortfolioBusinessName { get; set; }

    public bool TermsAccepted { get; set; }

    public bool MarketingOptIn { get; set; }

    public DateTime? TermsAcceptedUtc { get; set; }

    [MaxLength(20)]
    public string? PropertyCountRange { get; set; }

    [MaxLength(40)]
    public string? PortfolioType { get; set; }

    [MaxLength(40)]
    public string? OwnershipType { get; set; }

    [MaxLength(120)]
    public string? PrimaryMarket { get; set; }

    [MaxLength(40)]
    public string? ManagementStyle { get; set; }

    public bool ToolMaintenanceRequests { get; set; } = true;
    public bool ToolTurnoverCleaning { get; set; } = true;
    public bool ToolGuestMessaging { get; set; } = true;
    public bool ToolInvoicesPayments { get; set; } = true;
    public bool ToolDocumentsWarranties { get; set; }
    public bool ToolServiceProviders { get; set; } = true;
    public bool ToolTeamAccess { get; set; } = true;

    public bool NotifyUrgentMaintenance { get; set; } = true;
    public bool NotifyWeeklySummary { get; set; } = true;
    public bool NotifyBookingLeaseUpdates { get; set; } = true;

    public bool NotifyPushEnabled { get; set; } = true;
    public bool NotifyEmailEnabled { get; set; } = true;
    public bool NotifySmsEnabled { get; set; }
    public bool NotifyPropertyUpdates { get; set; } = true;
    public bool NotifyServiceUpdates { get; set; } = true;
    public bool NotifyTaskReminders { get; set; } = true;
    public bool NotifyPaymentsBilling { get; set; } = true;

    [MaxLength(5)]
    public string QuietHoursStart { get; set; } = "22:00";

    [MaxLength(5)]
    public string QuietHoursEnd { get; set; } = "07:00";

    public bool PlatformTermsAccepted { get; set; }

    public DateTime? RegistrationCompletedUtc { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<IndorPropertyAdminPortfolioProperty> PortfolioProperties { get; set; } = [];

    public ICollection<IndorPropertyAdminServiceRequest> ServiceRequests { get; set; } = [];

    public ICollection<IndorPropertyAdminHomecarePlan> HomecarePlans { get; set; } = [];

    public ICollection<IndorPropertyAdminScheduledVisit> ScheduledVisits { get; set; } = [];
}

[Table("IndorPropertyAdminPortfolioProperties")]
public class IndorPropertyAdminPortfolioProperty
{
    public int Id { get; set; }

    public int AdministratorId { get; set; }

    [ForeignKey(nameof(AdministratorId))]
    public IndorPropertyAdministrator? Administrator { get; set; }

    [Required, MaxLength(150)]
    public string PropertyName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string PropertyType { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Added";

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("IndorPropertyAdminServiceRequests")]
public class IndorPropertyAdminServiceRequest
{
    public int Id { get; set; }
    public int AdministratorId { get; set; }
    [ForeignKey(nameof(AdministratorId))]
    public IndorPropertyAdministrator? Administrator { get; set; }
    public int? PortfolioPropertyId { get; set; }
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(200)]
    public string PropertyName { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;
    [MaxLength(30)]
    public string Status { get; set; } = "Open";
    [MaxLength(30)]
    public string Category { get; set; } = "General";
    public DateTime? ScheduledUtc { get; set; }
    [MaxLength(80)]
    public string? EtaLabel { get; set; }
    [MaxLength(80)]
    public string? TeamLabel { get; set; }
    [MaxLength(300)]
    public string? ImageUrl { get; set; }
    public bool IsEmergency { get; set; }
    [MaxLength(4000)]
    public string? DetailsJson { get; set; }
    [MaxLength(80)]
    public string? TechnicianName { get; set; }
    public decimal? TechnicianRating { get; set; }
    [MaxLength(80)]
    public string? TechnicianTitle { get; set; }
    [MaxLength(80)]
    public string? VehicleLabel { get; set; }
    public int TimelineStep { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("IndorPropertyAdminHomecarePlans")]
public class IndorPropertyAdminHomecarePlan
{
    public int Id { get; set; }
    public int AdministratorId { get; set; }
    [ForeignKey(nameof(AdministratorId))]
    public IndorPropertyAdministrator? Administrator { get; set; }
    [MaxLength(120)]
    public string PlanName { get; set; } = string.Empty;
    [MaxLength(60)]
    public string Frequency { get; set; } = string.Empty;
    public int HomesCovered { get; set; }
    public DateTime? NextDueDate { get; set; }
    [MaxLength(50)]
    public string IconClass { get; set; } = "fa-wrench";
    [MaxLength(30)]
    public string ToneClass { get; set; } = "tone-blue";
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}

[Table("IndorPropertyAdminScheduledVisits")]
public class IndorPropertyAdminScheduledVisit
{
    public int Id { get; set; }
    public int AdministratorId { get; set; }
    [ForeignKey(nameof(AdministratorId))]
    public IndorPropertyAdministrator? Administrator { get; set; }
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(150)]
    public string PropertyName { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    [MaxLength(60)]
    public string TimeWindow { get; set; } = string.Empty;
    [MaxLength(300)]
    public string? ImageUrl { get; set; }
}

[Table("IndorPropertyAdminServiceCatalog")]
public class IndorPropertyAdminServiceCatalogItem
{
    public int Id { get; set; }
    [MaxLength(40)]
    public string CategoryKey { get; set; } = string.Empty;
    [MaxLength(80)]
    public string CategoryTitle { get; set; } = string.Empty;
    public int CategoryOrder { get; set; }
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;
    [MaxLength(80)]
    public string ServiceSlug { get; set; } = string.Empty;
    [MaxLength(50)]
    public string IconClass { get; set; } = "fa-wrench";
    [MaxLength(30)]
    public string ToneClass { get; set; } = "tone-blue";
    [MaxLength(80)]
    public string? LinkController { get; set; }
    [MaxLength(80)]
    public string? LinkAction { get; set; }
    public int? LinkRouteId { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}

[Table("IndorPropertyAdminPreventiveServiceCatalog")]
public class IndorPropertyAdminPreventiveServiceCatalogItem
{
    public int Id { get; set; }
    [MaxLength(60)]
    public string ServiceKey { get; set; } = string.Empty;
    [MaxLength(120)]
    public string ServiceName { get; set; } = string.Empty;
    [MaxLength(40)]
    public string DefaultFrequency { get; set; } = string.Empty;
    [MaxLength(50)]
    public string IconClass { get; set; } = "fa-wrench";
    [MaxLength(30)]
    public string ToneClass { get; set; } = "tone-blue";
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}

[Table("IndorPropertyAdminPreventivePlans")]
public class IndorPropertyAdminPreventivePlan
{
    public int Id { get; set; }
    public int AdministratorId { get; set; }
    [ForeignKey(nameof(AdministratorId))]
    public IndorPropertyAdministrator? Administrator { get; set; }
    public int PortfolioPropertyId { get; set; }
    [MaxLength(20)]
    public string Status { get; set; } = PropertyAdministratorPreventivePlanStatuses.Draft;
    [MaxLength(30)]
    public string PlanTier { get; set; } = "Basic";
    public decimal MonthlyPrice { get; set; }
    public decimal BundlePrice { get; set; }
    [MaxLength(2000)]
    public string SelectedServicesJson { get; set; } = "[]";
    [MaxLength(30)]
    public string Frequency { get; set; } = "Every3Months";
    [MaxLength(30)]
    public string PreferredTiming { get; set; } = "Flexible";
    [MaxLength(20)]
    public string PreferredDay { get; set; } = "Tue";
    [MaxLength(30)]
    public string EntryAccess { get; set; } = "HostPresent";
    [MaxLength(80)]
    public string UpdateRecipients { get; set; } = "Me";
    [MaxLength(500)]
    public string? Notes { get; set; }
    public bool AutoReminders { get; set; } = true;
    public DateTime? NextVisitDate { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? ActivatedUtc { get; set; }
}

public static class PropertyAdministratorPreventivePlanStatuses
{
    public const string Draft = "Draft";
    public const string Active = "Active";
}

public static class PropertyAdministratorRegistrationStatuses
{
    public const string Draft = "Draft";
    public const string Completed = "Completed";
}

public static class PropertyAdministratorRequestStatuses
{
    public const string Open = "Open";
    public const string Emergency = "Emergency";
    public const string Scheduled = "Scheduled";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
}

public static class PropertyAdministratorCatalog
{
    public static readonly string[] PropertyCountRanges = ["2-5", "6-10", "11-25", "25+"];

    public static readonly (string Value, string Label)[] PortfolioTypes =
    [
        ("ShortTermRentals", "Short-term rentals"),
        ("LongTermRentals", "Long-term rentals"),
        ("MixedPortfolio", "Mixed portfolio"),
        ("Commercial", "Commercial properties")
    ];

    public static readonly (string Value, string Label)[] OwnershipTypes =
    [
        ("IndividualOwner", "Individual owner"),
        ("Llc", "LLC"),
        ("Trust", "Trust"),
        ("Partnership", "Partnership")
    ];

    public static readonly (string Value, string Label)[] ManagementStyles =
    [
        ("SelfManage", "Self-manage"),
        ("SmallTeam", "Small team"),
        ("PropertyManager", "Property manager")
    ];

    public static readonly (string Value, string Label)[] PropertyTypes =
    [
        ("Condo", "Condo"),
        ("Duplex", "Duplex"),
        ("ShortTermRental", "Short-term rental"),
        ("SingleFamily", "Single-family home"),
        ("MultiFamily", "Multi-family"),
        ("Townhouse", "Townhouse")
    ];

    public static readonly string[] PrimaryMarkets =
    [
        "Charlotte, NC",
        "Gastonia, NC",
        "Concord, NC",
        "Rock Hill, SC",
        "Fort Mill, SC",
        "Huntersville, NC",
        "Matthews, NC",
        "Other"
    ];

    public static string LabelPortfolioType(string? value) =>
        PortfolioTypes.FirstOrDefault(p => p.Value == value).Label ?? value ?? "—";

    public static string LabelOwnershipType(string? value) =>
        OwnershipTypes.FirstOrDefault(p => p.Value == value).Label ?? value ?? "—";

    public static string LabelManagementStyle(string? value) =>
        ManagementStyles.FirstOrDefault(p => p.Value == value).Label ?? value ?? "—";

    public static string LabelPropertyType(string? value) =>
        PropertyTypes.FirstOrDefault(p => p.Value == value).Label ?? value ?? "—";

    public static readonly string[] UsStateCodes =
    [
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "DC"
    ];

    public static string FormatPropertyLocation(string city, string state, string? streetAddress = null, string? zipCode = null)
    {
        var normalizedCity = city.Trim();
        var normalizedState = state.Trim().ToUpperInvariant();
        var cityLine = string.IsNullOrWhiteSpace(zipCode)
            ? $"{normalizedCity}, {normalizedState}"
            : $"{normalizedCity}, {normalizedState} {zipCode.Trim()}";

        return string.IsNullOrWhiteSpace(streetAddress)
            ? cityLine
            : $"{streetAddress.Trim()}, {cityLine}";
    }

    public static string BuildStreetLine(string? houseNumber, string streetName)
    {
        var street = streetName.Trim();
        return string.IsNullOrWhiteSpace(houseNumber)
            ? street
            : $"{houseNumber.Trim()} {street}";
    }

    public static string DefaultImageForType(string propertyType) => propertyType switch
    {
        "Condo" => "/priority-crawlspace-check.png",
        "Duplex" => "/servicio1.jpeg",
        "ShortTermRental" => "/servicio3.jpeg",
        _ => "/inspeccion2.jpeg"
    };
}
