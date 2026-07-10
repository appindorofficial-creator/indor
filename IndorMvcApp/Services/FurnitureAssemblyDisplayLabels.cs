namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class FurnitureAssemblyDisplayLabels
{
    public static string FormatFurnitureType(string value) => value switch
    {
        "BedFrame" => DisplayLabelsLocalization.L("Bed frame"),
        "Dresser" => DisplayLabelsLocalization.L("Dresser"),
        "Desk" => DisplayLabelsLocalization.L("Desk"),
        "DiningTable" => DisplayLabelsLocalization.L("Dining table"),
        "Bookshelf" => DisplayLabelsLocalization.L("Bookshelf"),
        "TvStand" => DisplayLabelsLocalization.L("TV stand"),
        "OfficeChair" => DisplayLabelsLocalization.L("Office chair"),
        "Shelving" => DisplayLabelsLocalization.L("Shelving"),
        "MultipleItems" => DisplayLabelsLocalization.L("Multiple items"),
        _ => value
    };

    public static string FormatItemCount(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1"),
        "Two" => DisplayLabelsLocalization.L("2"),
        "Three" => DisplayLabelsLocalization.L("3"),
        "FourPlus" => DisplayLabelsLocalization.L("4+"),
        _ => value ?? "â€”"
    };

    public static string FormatCondition(string? value) => value switch
    {
        "NewInBox" => DisplayLabelsLocalization.L("New in box"),
        "PartiallyAssembled" => DisplayLabelsLocalization.L("Partially assembled"),
        "NeedsReassembly" => DisplayLabelsLocalization.L("Needs re-assembly"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "â€”"
    };

    public static string FormatWallAnchor(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "â€”"
    };

    public static string FormatRoom(string? value) => value switch
    {
        "LivingRoom" => DisplayLabelsLocalization.L("Living room"),
        "Bedroom" => DisplayLabelsLocalization.L("Bedroom"),
        "Office" => DisplayLabelsLocalization.L("Office"),
        "DiningRoom" => DisplayLabelsLocalization.L("Dining room"),
        "Patio" => DisplayLabelsLocalization.L("Patio"),
        "MultipleRooms" => DisplayLabelsLocalization.L("Multiple rooms"),
        _ => value ?? "â€”"
    };

    public static string FormatAccess(string value) => value switch
    {
        "FirstFloor" => DisplayLabelsLocalization.L("1st floor"),
        "Stairs" => DisplayLabelsLocalization.L("Stairs"),
        "Elevator" => DisplayLabelsLocalization.L("Elevator"),
        "ParkingNearby" => DisplayLabelsLocalization.L("Parking nearby"),
        _ => value
    };

    public static string FormatMovingHelp(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "â€”"
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning" => DisplayLabelsLocalization.L("8:00 AM - 12:00 PM"),
        "Afternoon" => DisplayLabelsLocalization.L("12:00 PM - 5:00 PM"),
        "Evening" => DisplayLabelsLocalization.L("5:00 PM - 8:00 PM"),
        _ => value ?? "â€”"
    };

    public static string FormatTimeShort(string? value) => value switch
    {
        "Morning" => DisplayLabelsLocalization.L("10:00 AM"),
        "Afternoon" => DisplayLabelsLocalization.L("2:00 PM"),
        "Evening" => DisplayLabelsLocalization.L("6:00 PM"),
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
            return DisplayLabelsLocalization.L("â€”");
        }

        return string.Join(", ", pipe
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(formatter));
    }

    public static string FormatItemsSummary(string? types, string? count)
    {
        var list = FormatPipeList(types, FormatFurnitureType);
        if (list == "â€”")
        {
            return DisplayLabelsLocalization.L("â€”");
        }

        var qty = FormatItemCount(count);
        return qty == "â€”" ? list : $"{list} ({qty} items)";
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
