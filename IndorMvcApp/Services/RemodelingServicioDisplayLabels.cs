namespace IndorMvcApp.Services;

public static class RemodelingServicioDisplayLabels
{
    public static string FormatScope(string? code) => code switch
    {
        "FullRemodel" => "Full remodel",
        "PartialUpdate" => "Partial update",
        "DesignConsult" => "Design consultation only",
        _ => "Full remodel"
    };

    public static string FormatTiming(string? code) => code switch
    {
        "ASAP" => "As soon as possible",
        "OneToThreeMonths" => "1–3 months",
        "ThreeToSixMonths" => "3–6 months",
        "Flexible" => "Flexible",
        _ => "Flexible"
    };

    public static string FormatBudget(string? code) => code switch
    {
        "Under5k" => "Under $5,000",
        "5kTo15k" => "$5,000 – $15,000",
        "15kTo50k" => "$15,000 – $50,000",
        "Over50k" => "Over $50,000",
        "NotSure" => "Not sure yet",
        _ => "Not sure yet"
    };

    public static string FormatContact(string? code) => code switch
    {
        "Call" => "Phone call",
        "Text" => "Text message",
        "Email" => "Email",
        _ => "Text message"
    };
}
