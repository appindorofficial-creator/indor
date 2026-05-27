namespace IndorMvcApp.Services;

public static class PackingDisplayLabels
{
    public static string FormatPackingType(string? value) => value switch
    {
        "FullPacking" => "Full packing",
        "PartialPacking" => "Partial packing",
        "FragileOnly" => "Fragile items only",
        "LastMinute" => "Last-minute help",
        _ => value ?? "—"
    };

    public static string FormatWhenMoving(string? value) => value switch
    {
        "Today" => "Today",
        "Tomorrow" => "Tomorrow",
        "ThisWeek" => "This week",
        "LaterDate" => "Later date",
        _ => value ?? "—"
    };

    public static string FormatPropertyType(string? value) => value switch
    {
        "Apartment" => "Apartment",
        "House" => "House",
        "Townhome" => "Townhome",
        "Office" => "Office",
        _ => value ?? "—"
    };

    public static string FormatHomeSize(string? value) => value switch
    {
        "OneTwoRooms" => "1-2 rooms",
        "ThreeFourRooms" => "3-4 rooms",
        "FivePlusRooms" => "5+ rooms",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatRoom(string value) => value switch
    {
        "Kitchen" => "Kitchen",
        "Bedroom" => "Bedroom",
        "LivingRoom" => "Living room",
        "Bathroom" => "Bathroom",
        "Closet" => "Closet",
        "Garage" => "Garage",
        "Office" => "Office",
        _ => value
    };

    public static string FormatSpecialItem(string value) => value switch
    {
        "TVs" => "TVs",
        "Glassware" => "Glassware",
        "Artwork" => "Artwork",
        "Electronics" => "Electronics",
        "Books" => "Books",
        "Clothing" => "Clothing",
        "Kitchenware" => "Kitchenware",
        _ => value
    };

    public static string FormatSupply(string value) => value switch
    {
        "Boxes" => "Boxes",
        "Tape" => "Tape",
        "BubbleWrap" => "Bubble wrap",
        "WardrobeBoxes" => "Wardrobe boxes",
        "Labels" => "Labels",
        "NotSure" => "Not sure",
        _ => value
    };

    public static string FormatAccess(string value) => value switch
    {
        "Stairs" => "Stairs",
        "Elevator" => "Elevator",
        "LongWalk" => "Long walk",
        "EasyAccess" => "Easy access",
        _ => value
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning" => "11:00 AM",
        "MidMorning" => "1:00 PM",
        "Afternoon" => "3:00 PM",
        "Evening" => "5:00 PM",
        _ => value ?? "11:00 AM"
    };

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMM d, yyyy") ?? "To be scheduled";

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
