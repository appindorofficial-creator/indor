namespace IndorMvcApp.Services;

public static class MovingDisplayLabels
{
    public static string FormatMoveType(string? value) => value switch
    {
        "MoveIn" => "Move-In",
        "MoveOut" => "Move-Out",
        "LocalMove" => "Local Move",
        "FullMove" => "Full Move",
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

    public static string FormatHomeSize(string? value) => value switch
    {
        "Studio" => "Studio",
        "OneTwoBedrooms" => "1-2 Bedrooms",
        "ThreePlusBedrooms" => "3+ Bedrooms",
        _ => value ?? "—"
    };

    public static string FormatMoveSize(string? value) => value switch
    {
        "FewItems" => "Few items",
        "Studio" => "Studio",
        "OneTwoBedroom" => "1-2 Bedroom",
        "ThreePlusBedroom" => "3+ Bedroom",
        _ => value ?? "—"
    };

    public static string FormatServiceType(string? value) => value switch
    {
        "MoversOnly" => "Movers only",
        "TruckAndMovers" => "Truck + Movers",
        _ => value ?? "—"
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning" => "9:00 AM – 11:00 AM",
        "MidMorning" => "11:00 AM – 1:00 PM",
        "Afternoon" => "2:00 PM – 4:00 PM",
        "Evening" => "4:00 PM – 6:00 PM",
        _ => value ?? "—"
    };

    public static string FormatItem(string value) => value switch
    {
        "Sofa" => "Sofa",
        "Bed" => "Bed",
        "Mattress" => "Mattress",
        "DiningTable" => "Dining table",
        "Appliances" => "Appliances",
        "Boxes" => "Boxes",
        "OfficeDesk" => "Office desk",
        "Other" => "Other",
        _ => value
    };

    public static string FormatAccessCondition(string value) => value switch
    {
        "Stairs" => "Stairs",
        "Elevator" => "Elevator",
        "LongWalk" => "Long walk",
        "TightHallway" => "Tight hallway",
        "ParkingIssue" => "Parking issue",
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

    public static string FormatYesNo(string? value) =>
        string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMMM d, yyyy") ?? "—";

    public static string FormatPriceRange(decimal min, decimal max) =>
        $"${min:0} – ${max:0}";

    public static string FormatDurationRange(int minHours, int maxHours) =>
        $"{minHours} – {maxHours} hours";

    public static (decimal Min, decimal Max, int DurMin, int DurMax) CalculateEstimate(
        string? tamanoHogar,
        string? tamanoMovimiento,
        string? tipoServicio,
        decimal baseMin,
        decimal baseMax,
        int baseDurMin,
        int baseDurMax)
    {
        var min = baseMin;
        var max = baseMax;
        var durMin = baseDurMin;
        var durMax = baseDurMax;

        if (tamanoHogar == "ThreePlusBedrooms" || tamanoMovimiento == "ThreePlusBedroom")
        {
            min += 120;
            max += 180;
            durMin += 2;
            durMax += 2;
        }
        else if (tamanoHogar == "OneTwoBedrooms" || tamanoMovimiento == "OneTwoBedroom")
        {
            min += 40;
            max += 60;
            durMin += 1;
            durMax += 1;
        }
        else if (tamanoMovimiento == "FewItems")
        {
            min -= 80;
            max -= 100;
            durMin = Math.Max(1, durMin - 1);
            durMax = Math.Max(2, durMax - 2);
        }

        if (string.Equals(tipoServicio, "TruckAndMovers", StringComparison.OrdinalIgnoreCase))
        {
            min += 80;
            max += 120;
        }

        return (Math.Max(0, min), Math.Max(min + 50, max), Math.Max(1, durMin), Math.Max(durMin + 1, durMax));
    }
}
