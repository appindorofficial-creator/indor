namespace IndorMvcApp.Services;

public static class FurnitureAssemblyDisplayLabels
{
    public static string FormatFurnitureType(string value) => value switch
    {
        "BedFrame" => "Bed frame",
        "Dresser" => "Dresser",
        "Desk" => "Desk",
        "DiningTable" => "Dining table",
        "Bookshelf" => "Bookshelf",
        "TvStand" => "TV stand",
        "OfficeChair" => "Office chair",
        "Shelving" => "Shelving",
        "MultipleItems" => "Multiple items",
        _ => value
    };

    public static string FormatItemCount(string? value) => value switch
    {
        "One" => "1",
        "Two" => "2",
        "Three" => "3",
        "FourPlus" => "4+",
        _ => value ?? "—"
    };

    public static string FormatCondition(string? value) => value switch
    {
        "NewInBox" => "New in box",
        "PartiallyAssembled" => "Partially assembled",
        "NeedsReassembly" => "Needs re-assembly",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatWallAnchor(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatRoom(string? value) => value switch
    {
        "LivingRoom" => "Living room",
        "Bedroom" => "Bedroom",
        "Office" => "Office",
        "DiningRoom" => "Dining room",
        "Patio" => "Patio",
        "MultipleRooms" => "Multiple rooms",
        _ => value ?? "—"
    };

    public static string FormatAccess(string value) => value switch
    {
        "FirstFloor" => "1st floor",
        "Stairs" => "Stairs",
        "Elevator" => "Elevator",
        "ParkingNearby" => "Parking nearby",
        _ => value
    };

    public static string FormatMovingHelp(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning" => "8:00 AM - 12:00 PM",
        "Afternoon" => "12:00 PM - 5:00 PM",
        "Evening" => "5:00 PM - 8:00 PM",
        _ => value ?? "—"
    };

    public static string FormatTimeShort(string? value) => value switch
    {
        "Morning" => "10:00 AM",
        "Afternoon" => "2:00 PM",
        "Evening" => "6:00 PM",
        _ => "2:00 PM"
    };

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMM d, yyyy") ?? "To be scheduled";

    public static string FormatSchedule(DateTime? date, string? window) =>
        $"{FormatDate(date)} - {FormatTimeWindow(window).Split('-')[0].Trim()}";

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

    public static string FormatItemsSummary(string? types, string? count)
    {
        var list = FormatPipeList(types, FormatFurnitureType);
        if (list == "—")
        {
            return "—";
        }

        var qty = FormatItemCount(count);
        return qty == "—" ? list : $"{list} ({qty} items)";
    }

    public static decimal CalculateEstimate(
        decimal basePrice,
        string? cantidadItems,
        string? tiposMueble,
        string? condicionItems,
        string? anclajePared,
        string? ayudaMover)
    {
        var price = basePrice;

        price += cantidadItems switch
        {
            "Two" => 20,
            "Three" => 40,
            "FourPlus" => 70,
            _ => 0
        };

        var typeCount = string.IsNullOrWhiteSpace(tiposMueble)
            ? 0
            : tiposMueble.Split('|', StringSplitOptions.RemoveEmptyEntries).Length;
        price += Math.Max(0, typeCount - 1) * 15;

        if (condicionItems == "PartiallyAssembled") price += 10;
        if (condicionItems == "NeedsReassembly") price += 20;

        if (string.Equals(anclajePared, "Yes", StringComparison.OrdinalIgnoreCase))
        {
            price += 25;
        }

        if (string.Equals(ayudaMover, "Yes", StringComparison.OrdinalIgnoreCase))
        {
            price += 30;
        }

        return Math.Max(basePrice, price);
    }
}
