namespace IndorMvcApp.Services;

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
        "ScheduleService" => "Schedule service",
        _ => "Set a reminder"
    };

    public static string FormatLastService(string? code) => code switch
    {
        "Within12Months" => "Within 12 months",
        "MoreThan12Months" => "More than 12 months",
        _ => "I don't know"
    };

    public static string FormatSign(string code) => code switch
    {
        "Ants" => "Ants",
        "Roaches" => "Roaches",
        "TermitesWoodDamage" => "Termites / wood damage",
        "Rodents" => "Rodents",
        "SpidersWasps" => "Spiders / wasps",
        "NoSigns" => "No signs",
        _ => code
    };

    public static string FormatArea(string code) => code switch
    {
        "Kitchen" => "Kitchen",
        "Bathrooms" => "Bathrooms",
        "Garage" => "Garage",
        "Attic" => "Attic",
        "CrawlspaceBasement" => "Crawlspace / basement",
        "ExteriorPerimeter" => "Exterior perimeter",
        "Yard" => "Yard",
        _ => code
    };

    public static string FormatYesNo(string? code) =>
        string.Equals(code, "Yes", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";

    public static string FormatServiceType(string? code) => code switch
    {
        "AnnualInspection" => "Annual pest inspection",
        "ProtectionPlan" => "Protection plan",
        _ => "Yearly reminder"
    };

    public static string FormatTiming(string? code) => code switch
    {
        "NextMonth" => "Next month",
        "In3Months" => "In 3 months",
        "EveryYearSpring" => "Every year in spring",
        _ => "This month"
    };

    public static string FormatPipeList(string? pipe, Func<string, string> formatter) =>
        string.IsNullOrWhiteSpace(pipe)
            ? "General check"
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(formatter));

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
