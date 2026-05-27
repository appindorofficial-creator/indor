namespace IndorMvcApp.Services;

public static class GeneralHelpDisplayLabels
{
    public static string FormatHelpType(string? value) => value switch
    {
        "MovingQuestion" => "Moving question",
        "ExtraHands" => "Extra hands",
        "SmallTask" => "Small task",
        "SuppliesPickup" => "Supplies pickup",
        "DonationDropOff" => "Donation drop-off",
        "Other" => "Other",
        _ => value ?? "—"
    };

    public static string FormatTiming(string? value) => value switch
    {
        "Today" => "Today",
        "Tomorrow" => "Tomorrow",
        "ThisWeek" => "This week",
        "Flexible" => "Flexible",
        _ => value ?? "—"
    };

    public static string FormatUrgency(string? value) => value switch
    {
        "Normal" => "Normal",
        "Priority" => "Priority",
        "Urgent" => "Urgent",
        _ => value ?? "—"
    };

    public static string FormatContact(string? value) => value switch
    {
        "Call" => "Call",
        "Text" => "Text",
        "Either" => "Either",
        _ => value ?? "—"
    };

    public static string FormatAccess(string? value) => value switch
    {
        "Apartment" => "Apartment",
        "House" => "House",
        "GateCode" => "Gate code",
        "Stairs" => "Stairs",
        "Elevator" => "Elevator",
        _ => value ?? "—"
    };

    public static string FormatAccessList(string? pipeValue) =>
        string.IsNullOrWhiteSpace(pipeValue)
            ? "—"
            : string.Join(", ", pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(FormatAccess));
}
