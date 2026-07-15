namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class TvWallMountingDisplayLabels
{
    public static string FormatRequestType(string? value) => value switch
    {
        "MountTv" => DisplayLabelsLocalization.L("Mount TV"),
        "DismountTv" => DisplayLabelsLocalization.L("Dismount TV"),
        "RemountTv" => DisplayLabelsLocalization.L("Re-mount existing TV"),
        "AddSoundbar" => DisplayLabelsLocalization.L("Add soundbar"),
        _ => value ?? "—"
    };

    public static string FormatTvSize(string? value) => value switch
    {
        "Under43" => DisplayLabelsLocalization.L("Under 43\""),
        "Size43_55" => DisplayLabelsLocalization.L("43-55\""),
        "Size56_64" => DisplayLabelsLocalization.L("56-64\""),
        "Size65_75" => DisplayLabelsLocalization.L("65-75\""),
        "Size75Plus" => DisplayLabelsLocalization.L("75\"+"),
        _ => value ?? "—"
    };

    public static string FormatTvCount(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1 TV"),
        "Two" => DisplayLabelsLocalization.L("2 TVs"),
        "ThreePlus" => DisplayLabelsLocalization.L("3+ TVs"),
        _ => value ?? "—"
    };

    public static string FormatRoom(string? value) => value switch
    {
        "LivingRoom" => DisplayLabelsLocalization.L("Living room"),
        "Bedroom" => DisplayLabelsLocalization.L("Bedroom"),
        "Office" => DisplayLabelsLocalization.L("Office"),
        "OtherRoom" => DisplayLabelsLocalization.L("Other room"),
        _ => value ?? "—"
    };

    public static string FormatWallType(string? value) => value switch
    {
        "Drywall" => DisplayLabelsLocalization.L("Drywall"),
        "Brick" => DisplayLabelsLocalization.L("Brick"),
        "Concrete" => DisplayLabelsLocalization.L("Concrete"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "—"
    };

    public static string FormatWallMount(string? value) => value switch
    {
        "YesHaveIt" => DisplayLabelsLocalization.L("Customer has wall mount"),
        "NeedProvided" => DisplayLabelsLocalization.L("Need one provided"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "—"
    };

    public static string FormatCableSetup(string? value) => value switch
    {
        "BasicVisible" => DisplayLabelsLocalization.L("Basic visible cables"),
        "HideInCover" => DisplayLabelsLocalization.L("Hide cables in cover"),
        "InWallConcealment" => DisplayLabelsLocalization.L("In-wall concealment"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "—"
    };

    public static string FormatYesNoNotSure(string? value) => value switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => value ?? "—"
    };

    public static string FormatAccess(string? value) => value switch
    {
        "GroundFloor" => DisplayLabelsLocalization.L("Ground floor"),
        "Stairs" => DisplayLabelsLocalization.L("Stairs"),
        "Elevator" => DisplayLabelsLocalization.L("Elevator"),
        "ParkingLimited" => DisplayLabelsLocalization.L("Parking limited"),
        _ => value ?? "—"
    };

    public static string FormatArrival(string? value) => value switch
    {
        "Morning" => DisplayLabelsLocalization.L("Morning"),
        "Afternoon" => DisplayLabelsLocalization.L("Afternoon"),
        "Evening" => DisplayLabelsLocalization.L("Evening"),
        "AsSoonAsPossible" => DisplayLabelsLocalization.L("As soon as possible"),
        _ => value ?? "—"
    };

    public static string FormatTimeShort(string? value) => value switch
    {
        "Morning" => DisplayLabelsLocalization.L("10:00 AM"),
        "Afternoon" => DisplayLabelsLocalization.L("2:00 PM"),
        "Evening" => DisplayLabelsLocalization.L("6:00 PM"),
        "AsSoonAsPossible" => DisplayLabelsLocalization.L("ASAP"),
        _ => DisplayLabelsLocalization.L("2:00 PM")
    };

    public static string FormatDate(DateTime? date) =>
        date?.ToString("MMM d, yyyy") ?? "To be scheduled";

    public static decimal CalculateEstimate(
        decimal basePrice,
        string? tamanoTv,
        string? cantidadTvs,
        string? tipoPared,
        string? configuracionCables,
        string? tieneSoporte,
        string? tipoSolicitud)
    {
        var price = basePrice;

        price += tamanoTv switch
        {
            "Size56_64" => 20,
            "Size65_75" => 40,
            "Size75Plus" => 60,
            _ => 0
        };

        price += cantidadTvs switch
        {
            "Two" => 50,
            "ThreePlus" => 90,
            _ => 0
        };

        price += tipoPared switch
        {
            "Brick" => 30,
            "Concrete" => 45,
            _ => 0
        };

        price += configuracionCables switch
        {
            "HideInCover" => 25,
            "InWallConcealment" => 55,
            _ => 0
        };

        if (string.Equals(tieneSoporte, "NeedProvided", StringComparison.OrdinalIgnoreCase))
        {
            price += 35;
        }

        if (string.Equals(tipoSolicitud, "AddSoundbar", StringComparison.OrdinalIgnoreCase))
        {
            price += 40;
        }

        return Math.Max(basePrice, price);
    }
}
