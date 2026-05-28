namespace IndorMvcApp.Services;

public static class ExteriorPaintDisplayLabels
{
    public static string FormatLastPainted(string? code) => code switch
    {
        "LessThan3Years" => "Less than 3 years",
        "ThreeToFiveYears" => "3–5 years",
        "FiveToSevenYears" => "5–7 years",
        "SevenPlusYears" => "7+ years",
        _ => "I don't know"
    };

    public static string FormatSurface(string? code) => code switch
    {
        "FiberCement" => "Fiber cement / Hardie",
        "Stucco" => "Stucco",
        "Brick" => "Brick",
        "VinylSiding" => "Vinyl siding",
        "MetalAluminum" => "Metal / aluminum",
        _ => "Wood siding"
    };

    public static string FormatYesNo(string? code) =>
        string.Equals(code, "Yes", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";

    public static string FormatYesNoNotSure(string? code) => code switch
    {
        "Yes" => "Yes",
        "No" => "No",
        _ => "Not sure"
    };

    public static string FormatIssue(string code) => code switch
    {
        "PeelingFlaking" => "Peeling / flaking",
        "Fading" => "Fading",
        "CrackedCaulk" => "Cracked caulk",
        "MildewStains" => "Mildew / stains",
        "WoodRot" => "Wood rot",
        "NoVisibleIssues" => "No visible issues",
        _ => code
    };

    public static string FormatArea(string code) => code switch
    {
        "FullExterior" => "Full exterior",
        "TrimFascia" => "Trim & fascia",
        "DoorsShutters" => "Doors / shutters",
        "GarageDoor" => "Garage door",
        "PorchRails" => "Porch / rails",
        "TouchUpOnly" => "Touch-up only",
        _ => code
    };

    public static string FormatPipeList(string? pipe, Func<string, string> formatter) =>
        string.IsNullOrWhiteSpace(pipe)
            ? "General review"
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(formatter));

    public static string FormatStories(string? code) => code switch
    {
        "Two" => "2 stories",
        "ThreePlus" => "3+ stories",
        _ => "1 story"
    };

    public static string FormatTiming(string? code) => code switch
    {
        "ThisMonth" => "This month",
        "JustEstimate" => "Just getting an estimate",
        _ => "As soon as possible"
    };
}
