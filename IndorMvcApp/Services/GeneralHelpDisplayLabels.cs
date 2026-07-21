namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class GeneralHelpDisplayLabels
{
    public static string FormatHelpType(string? value) => value switch
    {
        "MovingQuestion" => DisplayLabelsLocalization.L("Moving question"),
        "ExtraHands" => DisplayLabelsLocalization.L("Extra hands"),
        "SmallTask" => DisplayLabelsLocalization.L("Small task"),
        "SuppliesPickup" => DisplayLabelsLocalization.L("Supplies pickup"),
        "DonationDropOff" => DisplayLabelsLocalization.L("Donation drop-off"),
        "Other" => DisplayLabelsLocalization.L("Other"),
        _ => value ?? "—"
    };

    public static string FormatTiming(string? value) => value switch
    {
        "Today" => DisplayLabelsLocalization.L("Today"),
        "Tomorrow" => DisplayLabelsLocalization.L("Tomorrow"),
        "ThisWeek" => DisplayLabelsLocalization.L("This week"),
        "Flexible" => DisplayLabelsLocalization.L("Flexible"),
        _ => value ?? "—"
    };

    public static string FormatUrgency(string? value) => value switch
    {
        "Normal" => DisplayLabelsLocalization.L("Normal"),
        "Priority" => DisplayLabelsLocalization.L("Priority"),
        "Urgent" => DisplayLabelsLocalization.L("Urgent"),
        _ => value ?? "—"
    };

    public static string FormatContact(string? value) => value switch
    {
        "Call" => DisplayLabelsLocalization.L("Call"),
        "Text" => DisplayLabelsLocalization.L("Text"),
        "Either" => DisplayLabelsLocalization.L("Either"),
        _ => value ?? "—"
    };

    public static string FormatAccess(string? value) => value switch
    {
        "Apartment" => DisplayLabelsLocalization.L("Apartment"),
        "House" => DisplayLabelsLocalization.L("House"),
        "GateCode" => DisplayLabelsLocalization.L("Gate code"),
        "Stairs" => DisplayLabelsLocalization.L("Stairs"),
        "Elevator" => DisplayLabelsLocalization.L("Elevator"),
        _ => value ?? "—"
    };

    public static string FormatPendingConfirmationStatus() =>
        DisplayLabelsLocalization.L("Pending confirmation");

    public static string FormatAccessList(string? pipeValue) =>
        string.IsNullOrWhiteSpace(pipeValue)
            ? "—"
            : string.Join(", ", pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(FormatAccess));
}
