namespace IndorMvcApp.Models;

public static class OnboardingCatalog
{
    public static readonly IReadOnlyList<OnboardingOption> ProviderCategories =
    [
        new("electrical", "Electrical", "fa-bolt"),
        new("plumbing", "Plumbing", "fa-faucet"),
        new("hvac", "HVAC", "fa-fan"),
        new("handyman", "Handyman", "fa-wrench"),
        new("construction", "Construction Company", "fa-hard-hat"),
        new("bathroom", "Bathroom Remodeling", "fa-bath"),
        new("kitchen", "Kitchen Remodeling", "fa-utensils"),
        new("roofing", "Roofing", "fa-house-chimney"),
        new("painting", "Painting", "fa-paint-roller"),
        new("flooring", "Flooring", "fa-border-all"),
        new("cleaning", "Cleaning", "fa-spray-can-sparkles"),
        new("landscaping", "Landscaping", "fa-leaf"),
        new("pest", "Pest Control", "fa-bug"),
        new("appliance", "Appliance Repair", "fa-plug-circle-bolt"),
    ];

    public static readonly IReadOnlyList<OnboardingOption> ServiceOfferings =
    [
        new("installations", "Installations", "fa-plug"),
        new("repairs", "Repairs", "fa-screwdriver-wrench"),
        new("maintenance", "Maintenance", "fa-calendar-check"),
        new("upgrades", "Upgrades", "fa-arrow-up"),
        new("inspections", "Inspections", "fa-magnifying-glass"),
        new("emergency", "Emergency Services", "fa-truck-medical"),
    ];

    public static readonly IReadOnlyList<string> AllowedElectricalServices =
    [
        "Electrical repairs",
        "Panel work",
        "Outlets & switches",
        "Lighting installation",
    ];

    public static readonly IReadOnlyList<string> DisallowedForElectrical =
    [
        "Plumbing jobs",
        "HVAC jobs",
        "Roofing jobs",
        "General handyman jobs",
    ];
}
