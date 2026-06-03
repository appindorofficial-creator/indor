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

    public static readonly IReadOnlyList<OnboardingOption> PlumbingServiceOfferings =
    [
        new("leak_repair", "Leak repair", "fa-droplet"),
        new("drain_cleaning", "Drain cleaning", "fa-sink"),
        new("water_heater", "Water heater service", "fa-fire-flame-simple"),
        new("fixture_install", "Fixture installation", "fa-faucet-drip"),
    ];

    public static readonly IReadOnlyList<OnboardingOption> RoofingServiceOfferings =
    [
        new("shingle_roof_replacement", "Shingle roof replacement", "fa-house-chimney"),
        new("roof_repairs", "Roof repairs", "fa-hammer"),
        new("metal_roofing", "Metal roofing", "fa-sheet-plastic"),
        new("flat_roofing", "Flat roofing", "fa-square"),
        new("leak_detection", "Leak detection", "fa-magnifying-glass-droplet"),
        new("flashing_ventilation", "Flashing & ventilation", "fa-wind"),
        new("gutter_installation", "Gutter installation", "fa-grip-lines"),
        new("emergency_tarp_service", "Emergency tarp service", "fa-house-crack"),
    ];

    public static readonly IReadOnlyList<OnboardingOption> KitchenServiceOfferings =
    [
        new("full_kitchen_remodel", "Full kitchen remodel", "fa-utensils"),
        new("cabinet_installation", "Cabinet installation", "fa-box-archive"),
        new("cabinet_replacement", "Cabinet replacement", "fa-boxes-stacked"),
        new("countertop_installation", "Countertop installation", "fa-table-cells"),
        new("backsplash_installation", "Backsplash installation", "fa-border-all"),
        new("kitchen_flooring", "Flooring", "fa-grip-lines"),
        new("painting_finish_work", "Painting & finish work", "fa-paint-roller"),
        new("appliance_installation", "Appliance installation", "fa-blender"),
        new("sink_faucet_replacement", "Sink & faucet replacement", "fa-faucet"),
        new("lighting_fixture_coordination", "Lighting & fixture coordination", "fa-lightbulb"),
        new("kitchen_demolition", "Demolition", "fa-hammer"),
        new("trim_finish_carpentry", "Trim & finish carpentry", "fa-ruler-combined"),
    ];

    public static readonly IReadOnlyList<OnboardingOption> BathroomServiceOfferings =
    [
        new("full_bathroom_renovation", "Full Bathroom Renovation", "fa-bath"),
        new("shower_tub_install", "Shower / Tub Installation", "fa-shower"),
        new("vanity_install", "Vanity Installation", "fa-sink"),
        new("tile_flooring", "Tile & Flooring", "fa-border-all"),
        new("toilet_install", "Toilet Installation", "fa-toilet"),
        new("fixture_replacement", "Fixture Replacement", "fa-faucet"),
        new("waterproofing", "Waterproofing", "fa-droplet"),
        new("drywall_paint", "Drywall & Paint", "fa-paint-roller"),
        new("accessibility_upgrades", "Accessibility Upgrades", "fa-wheelchair"),
        new("glass_door_install", "Glass Door Installation", "fa-door-open"),
    ];

    public static readonly IReadOnlyList<OnboardingOption> ConstructionServiceOfferings =
    [
        new("home_additions", "Home additions", "fa-house-medical"),
        new("full_remodels", "Full home remodels", "fa-house-chimney"),
        new("structural_framing", "Structural framing", "fa-hammer"),
        new("drywall", "Drywall", "fa-border-all"),
        new("concrete_work", "Concrete work", "fa-road"),
        new("finish_carpentry", "Finish carpentry", "fa-ruler-combined"),
        new("decks_porches", "Decks & porches", "fa-umbrella-beach"),
        new("exterior_renovations", "Exterior renovations", "fa-house"),
        new("demolition", "Demolition", "fa-person-digging"),
        new("project_management", "Project management", "fa-clipboard-list"),
    ];

    public static readonly IReadOnlyList<OnboardingOption> HandymanServiceOfferings =
    [
        new("drywall_patch", "Drywall patch & repair", "fa-trowel"),
        new("door_adjustments", "Door adjustments", "fa-door-open"),
        new("tv_mounting", "TV / picture mounting", "fa-tv"),
        new("shelving_install", "Shelving installation", "fa-boxes-stacked"),
        new("furniture_assembly", "Furniture assembly", "fa-chair"),
        new("hardware_replacement", "Hardware replacement", "fa-screwdriver"),
        new("caulking_sealing", "Caulking & sealing", "fa-fill-drip"),
        new("punch_list", "Minor punch-list repairs", "fa-clipboard-list"),
    ];

    public static readonly IReadOnlyList<OnboardingOption> HvacServiceOfferings =
    [
        new("ac_repair", "AC Repair", "fa-snowflake"),
        new("ac_install", "AC Installation", "fa-fan"),
        new("heating_repair", "Heating Repair", "fa-fire-flame-simple"),
        new("heat_pump", "Heat Pump Service", "fa-wind"),
        new("ductwork", "Ductwork", "fa-layer-group"),
        new("thermostat", "Thermostat", "fa-temperature-half"),
        new("indoor_air_quality", "Indoor Air Quality", "fa-wind"),
        new("preventive_maintenance", "Preventive Maintenance", "fa-clipboard-check"),
        new("mini_split", "Mini-Split", "fa-border-all"),
        new("commercial_hvac", "Commercial HVAC", "fa-building"),
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
