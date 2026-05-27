namespace IndorMvcApp.Services;

public static class CleaningDisplayLabels
{
    public static string FormatCleaningType(string? value) => value switch
    {
        "MoveIn" => "Move-In Cleaning",
        "MoveOut" => "Move-Out Cleaning",
        "Both" => "Move-In & Move-Out Cleaning",
        _ => value ?? "—"
    };

    public static string FormatPropertyType(string? value) => value switch
    {
        "Apartment" => "Apartment",
        "House" => "House",
        "Condo" => "Condo",
        "Townhome" => "Townhome",
        _ => value ?? "—"
    };

    public static string FormatBedrooms(string? value) => value switch
    {
        "One" => "1 bed",
        "Two" => "2 bed",
        "Three" => "3 bed",
        "FourPlus" => "4+ bed",
        _ => value ?? "—"
    };

    public static string FormatBathrooms(string? value) => value switch
    {
        "One" => "1 bath",
        "Two" => "2 bath",
        "ThreePlus" => "3+ bath",
        _ => value ?? "—"
    };

    public static string FormatHomeSize(string? beds, string? baths) =>
        $"{FormatBedrooms(beds)} / {FormatBathrooms(baths)}";

    public static string FormatCondition(string? value) => value switch
    {
        "Empty" => "Empty property",
        "LightlyFurnished" => "Lightly furnished",
        "Occupied" => "Occupied",
        _ => value ?? "—"
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning" => "10:00 AM - 12:00 PM",
        "MidMorning" => "12:00 PM - 2:00 PM",
        "Afternoon" => "2:00 PM - 4:00 PM",
        "Evening" => "4:00 PM - 6:00 PM",
        _ => string.IsNullOrWhiteSpace(value) ? "To be scheduled" : value
    };

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMM d, yyyy") ?? "To be scheduled";

    public static string FormatSupplies(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatAccessMethod(string? value) => value switch
    {
        "Lockbox" => "Lockbox",
        "SomeoneHome" => "Someone will be home",
        "Doorman" => "Doorman / front desk",
        _ => value ?? "Lockbox"
    };

    public static string FormatPriorityArea(string value) => value switch
    {
        "Kitchen" => "Kitchen",
        "Bathrooms" => "Bathrooms",
        "Floors" => "Floors",
        "Bedrooms" => "Bedrooms",
        "LivingRoom" => "Living room",
        "Closets" => "Closets",
        _ => value
    };

    public static string FormatExtraTask(string value) => value switch
    {
        "InsideFridge" => "Inside fridge",
        "InsideOven" => "Inside oven",
        "InsideCabinets" => "Inside cabinets",
        "InteriorWindows" => "Interior windows",
        "Baseboards" => "Baseboards",
        "LaundryRoom" => "Laundry room",
        "Garage" => "Garage",
        "BalconyPatio" => "Balcony / patio",
        "TrashLeftBehind" => "Trash left behind",
        "PetHair" => "Pet hair",
        _ => value
    };

    public static string FormatPipeList(string? pipe, Func<string, string> formatter)
    {
        if (string.IsNullOrWhiteSpace(pipe))
        {
            return "—";
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
