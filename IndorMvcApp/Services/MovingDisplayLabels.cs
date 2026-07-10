using System.Globalization;
using IndorMvcApp.Localization;

namespace IndorMvcApp.Services;

public static class MovingDisplayLabels
{
    private static bool IsSpanish => UiCulture.IsSpanish(CultureInfo.CurrentUICulture.Name);

    private static string L(string english) => CatalogText.PickWithUiFallback(english, null, IsSpanish);

    public static string FormatMoveType(string? value) => value switch
    {
        "MoveIn" => L("Move-In"),
        "MoveOut" => L("Move-Out"),
        "LocalMove" => L("Local Move"),
        "FullMove" => L("Full Move"),
        _ => value ?? "—"
    };

    public static string FormatPropertyType(string? value) => value switch
    {
        "Apartment" => L("Apartment"),
        "House" => L("House"),
        "Condo" => L("Condo"),
        "Townhome" => L("Townhome"),
        _ => value ?? "—"
    };

    public static string FormatHomeSize(string? value) => value switch
    {
        "Studio" => L("Studio"),
        "OneTwoBedrooms" => L("1-2 Bedrooms"),
        "ThreePlusBedrooms" => L("3+ Bedrooms"),
        _ => value ?? "—"
    };

    public static string FormatMoveSize(string? value) => value switch
    {
        "FewItems" => L("Few items"),
        "Studio" => L("Studio"),
        "OneTwoBedroom" => L("1-2 Bedroom"),
        "ThreePlusBedroom" => L("3+ Bedroom"),
        _ => value ?? "—"
    };

    public static string FormatServiceType(string? value) => value switch
    {
        "MoversOnly" => L("Movers only"),
        "TruckAndMovers" => L("Truck + Movers"),
        _ => value ?? "—"
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning" => L("9:00 AM – 11:00 AM"),
        "MidMorning" => L("11:00 AM – 1:00 PM"),
        "Afternoon" => L("2:00 PM – 4:00 PM"),
        "Evening" => L("4:00 PM – 6:00 PM"),
        _ => value ?? "—"
    };

    public static string FormatItem(string value) => value switch
    {
        "Sofa" => L("Sofa"),
        "Bed" => L("Bed"),
        "Mattress" => L("Mattress"),
        "DiningTable" => L("Dining table"),
        "Appliances" => L("Appliances"),
        "Boxes" => L("Boxes"),
        "OfficeDesk" => L("Office desk"),
        "Other" => L("Other"),
        _ => L(value)
    };

    public static string FormatAccessCondition(string value) => value switch
    {
        "Stairs" => L("Stairs"),
        "Elevator" => L("Elevator"),
        "LongWalk" => L("Long walk"),
        "TightHallway" => L("Tight hallway"),
        "ParkingIssue" => L("Parking issue"),
        _ => L(value)
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
        string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase) ? L("Yes") : L("No");

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMMM d, yyyy", CultureInfo.CurrentCulture) ?? "—";

    public static string FormatPriceRange(decimal min, decimal max) =>
        $"${min:0} – ${max:0}";

    public static string FormatDurationRange(int minHours, int maxHours) =>
        L("{0} – {1} hours").Replace("{0}", minHours.ToString()).Replace("{1}", maxHours.ToString());

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
