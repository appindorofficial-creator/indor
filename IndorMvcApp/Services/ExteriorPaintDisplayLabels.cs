namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class ExteriorPaintDisplayLabels
{
    public static string FormatLastPainted(string? code) => code switch
    {
        "LessThan3Years" => DisplayLabelsLocalization.L("Less than 3 years"),
        "ThreeToFiveYears" => DisplayLabelsLocalization.L("3â€“5 years"),
        "FiveToSevenYears" => DisplayLabelsLocalization.L("5â€“7 years"),
        "SevenPlusYears" => DisplayLabelsLocalization.L("7+ years"),
        _ => DisplayLabelsLocalization.L("I don't know")
    };

    public static string FormatSurface(string? code) => code switch
    {
        "FiberCement" => DisplayLabelsLocalization.L("Fiber cement / Hardie"),
        "Stucco" => DisplayLabelsLocalization.L("Stucco"),
        "Brick" => DisplayLabelsLocalization.L("Brick"),
        "VinylSiding" => DisplayLabelsLocalization.L("Vinyl siding"),
        "MetalAluminum" => DisplayLabelsLocalization.L("Metal / aluminum"),
        _ => DisplayLabelsLocalization.L("Wood siding")
    };

    public static string FormatYesNo(string? code) =>
        string.Equals(code, "Yes", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";

    public static string FormatYesNoNotSure(string? code) => code switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string FormatIssue(string code) => code switch
    {
        "PeelingFlaking" => DisplayLabelsLocalization.L("Peeling / flaking"),
        "Fading" => DisplayLabelsLocalization.L("Fading"),
        "CrackedCaulk" => DisplayLabelsLocalization.L("Cracked caulk"),
        "MildewStains" => DisplayLabelsLocalization.L("Mildew / stains"),
        "WoodRot" => DisplayLabelsLocalization.L("Wood rot"),
        "NoVisibleIssues" => DisplayLabelsLocalization.L("No visible issues"),
        _ => code
    };

    public static string FormatArea(string code) => code switch
    {
        "FullExterior" => DisplayLabelsLocalization.L("Full exterior"),
        "TrimFascia" => DisplayLabelsLocalization.L("Trim & fascia"),
        "DoorsShutters" => DisplayLabelsLocalization.L("Doors / shutters"),
        "GarageDoor" => DisplayLabelsLocalization.L("Garage door"),
        "PorchRails" => DisplayLabelsLocalization.L("Porch / rails"),
        "TouchUpOnly" => DisplayLabelsLocalization.L("Touch-up only"),
        _ => code
    };

    public static string FormatPipeList(string? pipe, Func<string, string> formatter) =>
        string.IsNullOrWhiteSpace(pipe)
            ? "General review"
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(formatter));

    public static string FormatStories(string? code) => code switch
    {
        "Two" => DisplayLabelsLocalization.L("2 stories"),
        "ThreePlus" => DisplayLabelsLocalization.L("3+ stories"),
        _ => DisplayLabelsLocalization.L("1 story")
    };

    public static string FormatTiming(string? code) => code switch
    {
        "ThisMonth" => DisplayLabelsLocalization.L("This month"),
        "JustEstimate" => DisplayLabelsLocalization.L("Just getting an estimate"),
        _ => DisplayLabelsLocalization.L("As soon as possible")
    };
}
