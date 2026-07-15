namespace IndorMvcApp.Services;

/// <summary>
/// Maps homeowner catalog offerings to Multi-Property Owner service filter keys
/// (all / emergency / homecare / cleaning / outdoor / moving).
/// </summary>
public static class HomeownerServiceFilter
{
    public const string All = "all";
    public const string Emergency = "emergency";
    public const string Homecare = "homecare";
    public const string Cleaning = "cleaning";
    public const string Outdoor = "outdoor";
    public const string Moving = "moving";

    public static string Classify(string? name)
    {
        var n = (name ?? string.Empty).Trim().ToLowerInvariant();
        if (n.Length == 0)
        {
            return Homecare;
        }

        if (ContainsAny(n,
                "emergenc", "emergenc", "24/7", "flood", "inundac", "leak emergency", "fuga de emerg"))
        {
            return Emergency;
        }

        if (ContainsAny(n,
                "mov", "mudanz", "pack", "embalaj", "furniture assembly", "ensamblaje", "donation", "donaci"))
        {
            return Moving;
        }

        if (ContainsAny(n,
                "clean", "limpie", "trash", "basura", "trashout", "turnover", "linen", "restock", "pet deep"))
        {
            return Cleaning;
        }

        if (ContainsAny(n,
                "lawn", "cesped", "césped", "landscap", "jardiner", "pressure", "power wash",
                "lavado a presión", "pest", "plaga", "pool", "piscina", "hot tub", "jacuzzi",
                "gutter", "canaleta", "patio", "terraza", "exterior", "driveway", "concreto",
                "concrete", "outdoor", "fence", "cerca", "roof inspection", "inspección de techo",
                "inspeccion de techo"))
        {
            return Outdoor;
        }

        return Homecare;
    }

    public static string BuildSearchBlob(params string?[] parts)
        => string.Join(' ', parts.Where(p => !string.IsNullOrWhiteSpace(p)));

    private static bool ContainsAny(string haystack, params string[] needles)
        => needles.Any(n => haystack.Contains(n, StringComparison.Ordinal));
}
