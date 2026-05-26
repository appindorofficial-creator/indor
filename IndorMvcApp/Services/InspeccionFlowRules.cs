namespace IndorMvcApp.Services;

public static class InspeccionFlowRules
{
    public const string PrePurchaseHomeInspectionName = "Pre-Purchase Home Inspection";
    public const string ElectricalInspectionName = "Electrical Inspection";
    public const string CompleteHomeInspectionName = "Complete Home Inspection";
    public const string PlumbingInspectionName = "Plumbing Inspection";
    public const string HvacInspectionName = "HVAC Inspection";
    public const string StructuralInspectionName = "Structural Inspection";
    public const string RoofInspectionName = "Roof Inspection";

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

    public static bool SupportsPlumbingFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            PlumbingInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsHvacFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            HvacInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsStructuralFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            StructuralInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsRoofFlow(string? nombreInspeccion)
    {
        return string.Equals(
            nombreInspeccion?.Trim(),
            RoofInspectionName,
            StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsCustomFlow(string? nombreInspeccion)
    {
        return SupportsPurchaseFlow(nombreInspeccion)
               || SupportsElectricalFlow(nombreInspeccion)
               || SupportsCompleteHomeFlow(nombreInspeccion)
               || SupportsPlumbingFlow(nombreInspeccion)
               || SupportsHvacFlow(nombreInspeccion)
               || SupportsStructuralFlow(nombreInspeccion)
               || SupportsRoofFlow(nombreInspeccion);
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

        if (SupportsPlumbingFlow(nombreInspeccion))
        {
            return "PlumbingDetails";
        }

        if (SupportsHvacFlow(nombreInspeccion))
        {
            return "HvacDetails";
        }

        if (SupportsStructuralFlow(nombreInspeccion))
        {
            return "StructuralDetails";
        }

        if (SupportsRoofFlow(nombreInspeccion))
        {
            return "RoofDetails";
        }

        return null;
    }
}
