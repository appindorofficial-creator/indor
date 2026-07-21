namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class RemodelingServicioDisplayLabels
{
    public static string FormatScope(string? code) => code switch
    {
        "FullRemodel" => DisplayLabelsLocalization.L("Full remodel"),
        "PartialUpdate" => DisplayLabelsLocalization.L("Partial update"),
        "DesignConsult" => DisplayLabelsLocalization.L("Design consultation only"),
        _ => DisplayLabelsLocalization.L("Full remodel")
    };

    public static string FormatTiming(string? code) => code switch
    {
        "ASAP" => DisplayLabelsLocalization.L("As soon as possible"),
        "OneToThreeMonths" => DisplayLabelsLocalization.L("1–3 months"),
        "ThreeToSixMonths" => DisplayLabelsLocalization.L("3–6 months"),
        "Flexible" => DisplayLabelsLocalization.L("Flexible"),
        _ => DisplayLabelsLocalization.L("Flexible")
    };

    public static string FormatBudget(string? code) => code switch
    {
        "Under5k" => DisplayLabelsLocalization.L("Under $5,000"),
        "5kTo15k" => DisplayLabelsLocalization.L("$5,000 – $15,000"),
        "15kTo50k" => DisplayLabelsLocalization.L("$15,000 – $50,000"),
        "Over50k" => DisplayLabelsLocalization.L("Over $50,000"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure yet"),
        _ => DisplayLabelsLocalization.L("Not sure yet")
    };

    public static string FormatContact(string? code) => code switch
    {
        "Call" => DisplayLabelsLocalization.L("Phone call"),
        "Text" => DisplayLabelsLocalization.L("Text message"),
        "Email" => DisplayLabelsLocalization.L("Email"),
        _ => DisplayLabelsLocalization.L("Text message")
    };

    public static string FormatPendingQuoteStatus() =>
        DisplayLabelsLocalization.L("Pending quote");
}
