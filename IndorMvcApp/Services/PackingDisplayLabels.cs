namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class PackingDisplayLabels
{
    public static string FormatPackingType(string? value) => value switch
    {
        "FullPacking" => DisplayLabelsLocalization.L("Full packing"),
        "PartialPacking" => DisplayLabelsLocalization.L("Partial packing"),
        "FragileOnly" => DisplayLabelsLocalization.L("Fragile items only"),
        "LastMinute" => DisplayLabelsLocalization.L("Last-minute help"),
        _ => value ?? "—"
    };

    public static string FormatWhenMoving(string? value) => value switch
    {
        "Today" => DisplayLabelsLocalization.L("Today"),
        "Tomorrow" => DisplayLabelsLocalization.L("Tomorrow"),
        "ThisWeek" => DisplayLabelsLocalization.L("This week"),
        "LaterDate" => DisplayLabelsLocalization.L("Later date"),
        _ => value ?? "—"
    };

    public static string FormatPropertyType(string? value) => value switch
    {
        "Apartment" => DisplayLabelsLocalization.L("Apartment"),
        "House" => DisplayLabelsLocalization.L("House"),
        "Townhome" => DisplayLabelsLocalization.L("Townhome"),
        "Office" => DisplayLabelsLocalization.L("Office"),
        _ => value ?? "—"
    };

    public static string FormatHomeSize(string? value) => value switch
    {
        "OneTwoRooms" => DisplayLabelsLocalization.L("1-2 rooms"),
        "ThreeFourRooms" => DisplayLabelsLocalization.L("3-4 rooms"),
        "FivePlusRooms" => DisplayLabelsLocalization.L("5+ rooms"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "—"
    };

    public static string FormatRoom(string value) => value switch
    {
        "Kitchen" => DisplayLabelsLocalization.L("Kitchen"),
        "Bedroom" => DisplayLabelsLocalization.L("Bedroom"),
        "LivingRoom" => DisplayLabelsLocalization.L("Living room"),
        "Bathroom" => DisplayLabelsLocalization.L("Bathroom"),
        "Closet" => DisplayLabelsLocalization.L("Closet"),
        "Garage" => DisplayLabelsLocalization.L("Garage"),
        "Office" => DisplayLabelsLocalization.L("Office"),
        _ => value
    };

    public static string FormatSpecialItem(string value) => value switch
    {
        "TVs" => DisplayLabelsLocalization.L("TVs"),
        "Glassware" => DisplayLabelsLocalization.L("Glassware"),
        "Artwork" => DisplayLabelsLocalization.L("Artwork"),
        "Electronics" => DisplayLabelsLocalization.L("Electronics"),
        "Books" => DisplayLabelsLocalization.L("Books"),
        "Clothing" => DisplayLabelsLocalization.L("Clothing"),
        "Kitchenware" => DisplayLabelsLocalization.L("Kitchenware"),
        _ => value
    };

    public static string FormatSupply(string value) => value switch
    {
        "Boxes" => DisplayLabelsLocalization.L("Boxes"),
        "Tape" => DisplayLabelsLocalization.L("Tape"),
        "BubbleWrap" => DisplayLabelsLocalization.L("Bubble wrap"),
        "WardrobeBoxes" => DisplayLabelsLocalization.L("Wardrobe boxes"),
        "Labels" => DisplayLabelsLocalization.L("Labels"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value
    };

    public static string FormatAccess(string value) => value switch
    {
        "Stairs" => DisplayLabelsLocalization.L("Stairs"),
        "Elevator" => DisplayLabelsLocalization.L("Elevator"),
        "LongWalk" => DisplayLabelsLocalization.L("Long walk"),
        "EasyAccess" => DisplayLabelsLocalization.L("Easy access"),
        _ => value
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning" => DisplayLabelsLocalization.L("11:00 AM"),
        "MidMorning" => DisplayLabelsLocalization.L("1:00 PM"),
        "Afternoon" => DisplayLabelsLocalization.L("3:00 PM"),
        "Evening" => DisplayLabelsLocalization.L("5:00 PM"),
        _ => value ?? "11:00 AM"
    };

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMM d, yyyy") ?? "To be scheduled";

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

    public static (string? TipoPropiedad, string? TamanoHogar, string? TipoEmpaque) MapBestForSelection(string? bestFor) =>
        bestFor switch
        {
            "MoveOut" => (null, null, "FullPacking"),
            "BusyFamilies" => (null, null, "PartialPacking"),
            "Apartments" => ("Apartment", "OneTwoRooms", null),
            "LargeHomes" => ("House", "FivePlusRooms", null),
            _ => (null, null, null)
        };

    public static DateTime? ResolveServiceDate(string? cuandoMudanza, DateTime? fechaServicio) =>
        cuandoMudanza switch
        {
            "Today" => DateTime.Today,
            "Tomorrow" => DateTime.Today.AddDays(1),
            "ThisWeek" => DateTime.Today.AddDays(3),
            "LaterDate" => fechaServicio,
            _ => fechaServicio ?? DateTime.Today.AddDays(7)
        };

    public static decimal CalculateEstimate(
        decimal basePrice,
        string? tipoEmpaque,
        string? tamanoHogar,
        string? rooms,
        string? specialItems,
        string? supplies)
    {
        var price = basePrice;

        price += tipoEmpaque switch
        {
            "FullPacking" => 60,
            "FragileOnly" => -10,
            "LastMinute" => 40,
            _ => 0
        };

        price += tamanoHogar switch
        {
            "OneTwoRooms" => 0,
            "ThreeFourRooms" => 30,
            "FivePlusRooms" => 70,
            _ => 15
        };

        var roomCount = string.IsNullOrWhiteSpace(rooms)
            ? 0
            : rooms.Split('|', StringSplitOptions.RemoveEmptyEntries).Length;
        price += Math.Max(0, roomCount - 2) * 8;

        var itemCount = string.IsNullOrWhiteSpace(specialItems)
            ? 0
            : specialItems.Split('|', StringSplitOptions.RemoveEmptyEntries).Length;
        price += itemCount * 10;

        var supplyCount = string.IsNullOrWhiteSpace(supplies)
            ? 0
            : supplies.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Count(s => !string.Equals(s, "NotSure", StringComparison.OrdinalIgnoreCase));
        price += supplyCount * 5;

        return Math.Max(basePrice, price);
    }
}
