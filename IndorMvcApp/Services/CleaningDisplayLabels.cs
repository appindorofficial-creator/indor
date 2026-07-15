namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class CleaningDisplayLabels
{
    public static string FormatCleaningType(string? value) => value switch
    {
        "MoveIn" => DisplayLabelsLocalization.L("Move-In Cleaning"),
        "MoveOut" => DisplayLabelsLocalization.L("Move-Out Cleaning"),
        "Both" => DisplayLabelsLocalization.L("Move-In & Move-Out Cleaning"),
        _ => value ?? "—"
    };

    public static string FormatPropertyType(string? value) => value switch
    {
        "Apartment" => DisplayLabelsLocalization.L("Apartment"),
        "House" => DisplayLabelsLocalization.L("House"),
        "Condo" => DisplayLabelsLocalization.L("Condo"),
        "Townhome" => DisplayLabelsLocalization.L("Townhome"),
        _ => value ?? "—"
    };

    public static string FormatBedrooms(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1 bed"),
        "Two" => DisplayLabelsLocalization.L("2 bed"),
        "Three" => DisplayLabelsLocalization.L("3 bed"),
        "FourPlus" => DisplayLabelsLocalization.L("4+ bed"),
        _ => value ?? "—"
    };

    public static string FormatBathrooms(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1 bath"),
        "Two" => DisplayLabelsLocalization.L("2 bath"),
        "ThreePlus" => DisplayLabelsLocalization.L("3+ bath"),
        _ => value ?? "—"
    };

    public static string FormatHomeSize(string? beds, string? baths) =>
        $"{FormatBedrooms(beds)} / {FormatBathrooms(baths)}";

    public static string FormatCondition(string? value) => value switch
    {
        "Empty" => DisplayLabelsLocalization.L("Empty property"),
        "LightlyFurnished" => DisplayLabelsLocalization.L("Lightly furnished"),
        "Occupied" => DisplayLabelsLocalization.L("Occupied"),
        _ => value ?? "—"
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning" => DisplayLabelsLocalization.L("10:00 AM - 12:00 PM"),
        "MidMorning" => DisplayLabelsLocalization.L("12:00 PM - 2:00 PM"),
        "Afternoon" => DisplayLabelsLocalization.L("2:00 PM - 4:00 PM"),
        "Evening" => DisplayLabelsLocalization.L("4:00 PM - 6:00 PM"),
        _ => string.IsNullOrWhiteSpace(value) ? "To be scheduled" : value
    };

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMM d, yyyy") ?? "To be scheduled";

    public static string FormatSupplies(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "—"
    };

    public static string FormatAccessMethod(string? value) => value switch
    {
        "Lockbox" => DisplayLabelsLocalization.L("Lockbox"),
        "SomeoneHome" => DisplayLabelsLocalization.L("Someone will be home"),
        "Doorman" => DisplayLabelsLocalization.L("Doorman / front desk"),
        _ => value ?? "Lockbox"
    };

    public static string FormatPriorityArea(string value) => value switch
    {
        "Kitchen" => DisplayLabelsLocalization.L("Kitchen"),
        "Bathrooms" => DisplayLabelsLocalization.L("Bathrooms"),
        "Floors" => DisplayLabelsLocalization.L("Floors"),
        "Bedrooms" => DisplayLabelsLocalization.L("Bedrooms"),
        "LivingRoom" => DisplayLabelsLocalization.L("Living room"),
        "Closets" => DisplayLabelsLocalization.L("Closets"),
        _ => value
    };

    public static string FormatExtraTask(string value) => value switch
    {
        "InsideFridge" => DisplayLabelsLocalization.L("Inside fridge"),
        "InsideOven" => DisplayLabelsLocalization.L("Inside oven"),
        "InsideCabinets" => DisplayLabelsLocalization.L("Inside cabinets"),
        "InteriorWindows" => DisplayLabelsLocalization.L("Interior windows"),
        "Baseboards" => DisplayLabelsLocalization.L("Baseboards"),
        "LaundryRoom" => DisplayLabelsLocalization.L("Laundry room"),
        "Garage" => DisplayLabelsLocalization.L("Garage"),
        "BalconyPatio" => DisplayLabelsLocalization.L("Balcony / patio"),
        "TrashLeftBehind" => DisplayLabelsLocalization.L("Trash left behind"),
        "PetHair" => DisplayLabelsLocalization.L("Pet hair"),
        _ => value
    };

    public static string FormatPipeList(string? pipe, Func<string, string> formatter)
    {
        if (string.IsNullOrWhiteSpace(pipe))
        {
            return DisplayLabelsLocalization.L("—");
        }

        return string.Join(", ", pipe
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(formatter));
    }

    public static decimal CalculateEstimate(
        decimal basePrice,
        string? beds,
        string? baths,
        string? condition,
        string? areas,
        string? extras,
        string? supplies)
    {
        var price = basePrice;

        price += beds switch
        {
            "Three" => 40,
            "FourPlus" => 80,
            _ => 0
        };

        price += baths switch
        {
            "Two" => 20,
            "ThreePlus" => 40,
            _ => 0
        };

        if (condition == "Occupied") price += 30;
        if (condition == "LightlyFurnished") price += 15;

        var areaCount = string.IsNullOrWhiteSpace(areas)
            ? 0
            : areas.Split('|', StringSplitOptions.RemoveEmptyEntries).Length;
        price += Math.Max(0, areaCount - 3) * 10;

        var extraCount = string.IsNullOrWhiteSpace(extras)
            ? 0
            : extras.Split('|', StringSplitOptions.RemoveEmptyEntries).Length;
        price += extraCount * 15;

        if (string.Equals(supplies, "Yes", StringComparison.OrdinalIgnoreCase))
        {
            price += 25;
        }

        return Math.Max(basePrice, price);
    }

    public static (string TipoLimpieza, string Condicion) MapBestForSelection(string? bestFor) =>
        bestFor switch
        {
            "MoveOut" => ("MoveOut", "Empty"),
            "OccupiedHome" => ("MoveIn", "Occupied"),
            "EmptyProperty" => ("MoveIn", "Empty"),
            _ => ("MoveIn", "Empty")
        };
}
