namespace IndorMvcApp.Services;

public static class InspeccionFlowRules
{
    public const string PrePurchaseHomeInspectionName = "Pre-Purchase Home Inspection";
    public const string ElectricalInspectionName = "Electrical Inspection";
    public const string CompleteHomeInspectionName = "Complete Home Inspection";

    public static bool SupportsPurchaseFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            PrePurchaseHomeInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsElectricalFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            ElectricalInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsCompleteHomeFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            CompleteHomeInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsCustomFlow(string? nombreInspeccion)
    {
        return SupportsPurchaseFlow(nombreInspeccion)
               || SupportsElectricalFlow(nombreInspeccion)
               || SupportsCompleteHomeFlow(nombreInspeccion);
    }

    public static string? GetFlowAction(string? nombreInspeccion)
    {
        if (SupportsPurchaseFlow(nombreInspeccion))
        {
            return "PurchaseDetails";
        }

        if (SupportsElectricalFlow(nombreInspeccion))
        {
            return "ElectricalDetails";
        }

        if (SupportsCompleteHomeFlow(nombreInspeccion))
        {
            return "HomeReviewDetails";
        }

        return null;
    }
}
