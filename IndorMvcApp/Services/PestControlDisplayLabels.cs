namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class PestControlPricingService
{
    public const decimal InspectionPrice = 99m;
    public const decimal PlanMonthlyPrice = 29m;

    public static decimal GetEstimatedPrice(string? tipoServicio) => tipoServicio switch
    {
        "AnnualInspection" => InspectionPrice,
        "ProtectionPlan" => PlanMonthlyPrice,
        _ => 0m
    };
}

public static class PestControlDisplayLabels
{
    public static string FormatInitialAction(string? code) => code switch
    {
        "ScheduleService" => DisplayLabelsLocalization.L("Schedule service"),
        _ => DisplayLabelsLocalization.L("Set a reminder")
    };

    public static string FormatLastService(string? code) => code switch
    {
        "Within12Months" => DisplayLabelsLocalization.L("Within 12 months"),
        "MoreThan12Months" => DisplayLabelsLocalization.L("More than 12 months"),
        _ => DisplayLabelsLocalization.L("I don't know")
    };

    public static string FormatSign(string code) => code switch
    {
        "Ants" => DisplayLabelsLocalization.L("Ants"),
        "Roaches" => DisplayLabelsLocalization.L("Roaches"),
        "TermitesWoodDamage" => DisplayLabelsLocalization.L("Termites / wood damage"),
        "Rodents" => DisplayLabelsLocalization.L("Rodents"),
        "SpidersWasps" => DisplayLabelsLocalization.L("Spiders / wasps"),
        "NoSigns" => DisplayLabelsLocalization.L("No signs"),
        _ => code
    };

    public static string FormatArea(string code) => code switch
    {
        "Kitchen" => DisplayLabelsLocalization.L("Kitchen"),
        "Bathrooms" => DisplayLabelsLocalization.L("Bathrooms"),
        "Garage" => DisplayLabelsLocalization.L("Garage"),
        "Attic" => DisplayLabelsLocalization.L("Attic"),
        "CrawlspaceBasement" => DisplayLabelsLocalization.L("Crawlspace / basement"),
        "ExteriorPerimeter" => DisplayLabelsLocalization.L("Exterior perimeter"),
        "Yard" => DisplayLabelsLocalization.L("Yard"),
        _ => code
    };

    public static string FormatYesNo(string? code) =>
        string.Equals(code, "Yes", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";

    public static string FormatServiceType(string? code) => code switch
    {
        "AnnualInspection" => DisplayLabelsLocalization.L("Annual pest inspection"),
        "ProtectionPlan" => DisplayLabelsLocalization.L("Protection plan"),
        _ => DisplayLabelsLocalization.L("Yearly reminder")
    };

    public static string FormatTiming(string? code) => code switch
    {
        "NextMonth" => DisplayLabelsLocalization.L("Next month"),
        "In3Months" => DisplayLabelsLocalization.L("In 3 months"),
        "EveryYearSpring" => DisplayLabelsLocalization.L("Every year in spring"),
        _ => DisplayLabelsLocalization.L("This month")
    };

    public static string FormatPipeList(string? pipe, Func<string, string> formatter) =>
        string.IsNullOrWhiteSpace(pipe)
            ? "General check"
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(formatter));

    public static string FormatConfirmedStatus() =>
        DisplayLabelsLocalization.L("Reminder and service saved");

    public static DateTime GetDueDate(string? timing)
    {
        var today = DateTime.Today;
        return timing switch
        {
            "NextMonth" => today.AddMonths(1),
            "In3Months" => today.AddMonths(3),
            "EveryYearSpring" => GetNextSpringDate(today),
            _ => today.AddDays(7)
        };
    }

    private static DateTime GetNextSpringDate(DateTime reference)
    {
        var spring = new DateTime(reference.Year, 3, 15);
        return reference <= spring ? spring : new DateTime(reference.Year + 1, 3, 15);
    }
}
