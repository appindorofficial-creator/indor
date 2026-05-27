namespace IndorMvcApp.Services;

public static class TvWallMountingDisplayLabels
{
    public static string FormatRequestType(string? value) => value switch
    {
        "MountTv" => "Mount TV",
        "DismountTv" => "Dismount TV",
        "RemountTv" => "Re-mount existing TV",
        "AddSoundbar" => "Add soundbar",
        _ => value ?? "—"
    };

    public static string FormatTvSize(string? value) => value switch
    {
        "Under43" => "Under 43\"",
        "Size43_55" => "43-55\"",
        "Size56_64" => "56-64\"",
        "Size65_75" => "65-75\"",
        "Size75Plus" => "75\"+",
        _ => value ?? "—"
    };

    public static string FormatTvCount(string? value) => value switch
    {
        "One" => "1 TV",
        "Two" => "2 TVs",
        "ThreePlus" => "3+ TVs",
        _ => value ?? "—"
    };

    public static string FormatRoom(string? value) => value switch
    {
        "LivingRoom" => "Living room",
        "Bedroom" => "Bedroom",
        "Office" => "Office",
        "OtherRoom" => "Other room",
        _ => value ?? "—"
    };

    public static string FormatWallType(string? value) => value switch
    {
        "Drywall" => "Drywall",
        "Brick" => "Brick",
        "Concrete" => "Concrete",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatWallMount(string? value) => value switch
    {
        "YesHaveIt" => "Customer has wall mount",
        "NeedProvided" => "Need one provided",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatCableSetup(string? value) => value switch
    {
        "BasicVisible" => "Basic visible cables",
        "HideInCover" => "Hide cables in cover",
        "InWallConcealment" => "In-wall concealment",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatYesNoNotSure(string? value) => value switch
    {
        "Yes" => "Yes",
        "No" => "No",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatAccess(string? value) => value switch
    {
        "GroundFloor" => "Ground floor",
        "Stairs" => "Stairs",
        "Elevator" => "Elevator",
        "ParkingLimited" => "Parking limited",
        _ => value ?? "—"
    };

    public static string FormatArrival(string? value) => value switch
    {
        "Morning" => "Morning",
        "Afternoon" => "Afternoon",
        "Evening" => "Evening",
        "AsSoonAsPossible" => "As soon as possible",
        _ => value ?? "—"
    };

    public static string FormatTimeShort(string? value) => value switch
    {
        "Morning" => "10:00 AM",
        "Afternoon" => "2:00 PM",
        "Evening" => "6:00 PM",
        "AsSoonAsPossible" => "ASAP",
        _ => "2:00 PM"
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
