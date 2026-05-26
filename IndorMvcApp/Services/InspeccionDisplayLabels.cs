namespace IndorMvcApp.Services;

public static class InspeccionDisplayLabels
{
    public static string MotivoRevision(string? value) => value switch
    {
        "BuyingHome" => "Buying a home",
        "SafetyCheck" => "Safety check",
        "IssueAtHome" => "Issue at home",
        "InspectionFollowUp" => "Inspection follow-up",
        _ => "General review"
    };

    public static string PreocupacionPrincipal(string? value) => value switch
    {
        "BreakerTrips" => "Breaker trips",
        "LightsFlicker" => "Lights flicker",
        "OutletsNotWorking" => "Outlets not working",
        "OldPanel" => "Old panel",
        "BurningSmell" => "Burning smell",
        "GeneralReview" => "General electrical review",
        _ => "General electrical review"
    };

    public static string ObjetivoPrincipal(string? value) => value switch
    {
        "BuyWithConfidence" => "Buy with confidence",
        "UnderstandRepairRisks" => "Understand repair risks",
        "NegotiateRepairs" => "Negotiate repairs",
        "SecondOpinion" => "Second opinion",
        _ => "Home purchase review"
    };

    public static string RolComprador(string? value) => value switch
    {
        "Buyer" => "Buyer",
        "Realtor" => "Realtor",
        "Investor" => "Investor",
        _ => value ?? "Buyer"
    };

    public static string FormatElectricalConcern(string? preocupacion, string? motivo)
    {
        return $"{PreocupacionPrincipal(preocupacion)} / {MotivoRevision(motivo).ToLowerInvariant()}";
    }

    public static string FormatPurchaseConcern(string? objetivo, string? notas, string? rol)
    {
        var summary = ObjetivoPrincipal(objetivo);
        if (!string.IsNullOrWhiteSpace(notas))
        {
            var trimmed = notas.Trim();
            if (trimmed.Length > 60)
            {
                trimmed = trimmed[..57] + "...";
            }

            return $"{summary} — {trimmed}";
        }

        return $"{summary} ({RolComprador(rol)})";
    }

    public static string FormatTime(TimeSpan time)
    {
        var hours = time.Hours;
        var minutes = time.Minutes;
        var period = hours >= 12 ? "PM" : "AM";
        var displayHour = hours % 12;
        if (displayHour == 0)
        {
            displayHour = 12;
        }

        return minutes == 0
            ? $"{displayHour}:00 {period}"
            : $"{displayHour}:{minutes:D2} {period}";
    }

    public static string MotivoInspeccionCompleta(string? value) => value switch
    {
        "BuyingHome" => "Buying a home",
        "AnnualReview" => "Annual review",
        "SellingHome" => "Selling a home",
        "InspectionFollowUp" => "Inspection follow-up",
        _ => "Home review"
    };

    public static string AreaEnfoque(string? value) => value switch
    {
        "Electrical" => "Electrical",
        "HVAC" => "HVAC",
        "GeneralStructure" => "General structure",
        "Plumbing" => "Plumbing",
        "Roof" => "Roof",
        "Moisture" => "Moisture",
        "Safety" => "Safety",
        _ => value ?? string.Empty
    };

    public static string FormatAreasEnfoque(string? areasPipeSeparated)
    {
        if (string.IsNullOrWhiteSpace(areasPipeSeparated))
        {
            return "General structure";
        }

        return string.Join(" / ",
            areasPipeSeparated
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(AreaEnfoque));
    }
}
